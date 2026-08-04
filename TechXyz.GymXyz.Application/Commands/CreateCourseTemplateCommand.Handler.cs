using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCourseTemplateCommandHandler : IRequestHandler<CreateCourseTemplateCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateCourseTemplateCommand> _validator;

    public CreateCourseTemplateCommandHandler(
        IGymDbContext dbContext,
        IValidator<CreateCourseTemplateCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateCourseTemplateCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var discipline = await _dbContext.Disciplines
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.DisciplineId && candidate.IsActive,
                cancellationToken);
        if (discipline is null)
        {
            throw new ValidationException("Discipline introuvable.");
        }

        var template = new CourseTemplate(request.Name.Trim())
        {
            Discipline = discipline,
            IconKey = AddressHelper.NormalizeOptional(request.IconKey),
            DurationMinutes = request.DurationMinutes,
            Capacity = request.Capacity,
            DefaultRoomId = await ResolveRoomIdAsync(request.DefaultRoomId, cancellationToken),
            Level = request.Level,
            Intensity = request.Intensity,
            Price = request.Price,
            Description = AddressHelper.NormalizeOptional(request.Description)
        };

        // Written through the navigation rather than the sync helper: the
        // template has no key yet, and EF fixes the foreign keys up on insert.
        if (request.CoachIds is { Count: > 0 } coachIds)
        {
            var coaches = await CourseTemplateCompositionHelper.LoadOrderedCoachesAsync(
                _dbContext, coachIds, cancellationToken);

            for (var rank = 0; rank < coaches.Count; rank++)
            {
                template.AddCoach(coaches[rank], rank);
            }
        }

        _dbContext.CourseTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return template.Id;
    }

    private async Task<int?> ResolveRoomIdAsync(int? roomId, CancellationToken cancellationToken)
    {
        if (roomId is not { } id)
        {
            return null;
        }

        var exists = await _dbContext.Rooms
            .AnyAsync(room => room.Id == id && room.IsActive, cancellationToken);

        return exists ? id : throw new ValidationException("Studio introuvable.");
    }
}
