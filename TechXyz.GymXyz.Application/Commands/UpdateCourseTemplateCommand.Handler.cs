using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateCourseTemplateCommandHandler : IRequestHandler<UpdateCourseTemplateCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateCourseTemplateCommand> _validator;

    public UpdateCourseTemplateCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdateCourseTemplateCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateCourseTemplateCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var template = await _dbContext.CourseTemplates
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);
        if (template is null)
        {
            return false;
        }

        if (template.DisciplineId != request.DisciplineId)
        {
            var disciplineExists = await _dbContext.Disciplines
                .AnyAsync(
                    candidate => candidate.Id == request.DisciplineId && candidate.IsActive,
                    cancellationToken);
            if (!disciplineExists)
            {
                throw new ValidationException("Discipline introuvable.");
            }

            template.DisciplineId = request.DisciplineId;
        }

        if (request.DefaultRoomId is { } roomId)
        {
            var roomExists = await _dbContext.Rooms
                .AnyAsync(room => room.Id == roomId && room.IsActive, cancellationToken);
            if (!roomExists)
            {
                throw new ValidationException("Studio introuvable.");
            }
        }

        template.Name = request.Name.Trim();
        template.IconKey = AddressHelper.NormalizeOptional(request.IconKey);
        template.DurationMinutes = request.DurationMinutes;
        template.Capacity = request.Capacity;
        template.DefaultRoomId = request.DefaultRoomId;
        template.Level = request.Level;
        template.Intensity = request.Intensity;
        template.Price = request.Price;
        template.Description = AddressHelper.NormalizeOptional(request.Description);

        if (request.CoachIds is not null)
        {
            await CourseTemplateCompositionHelper.SyncCoachesAsync(
                _dbContext, template, request.CoachIds, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
