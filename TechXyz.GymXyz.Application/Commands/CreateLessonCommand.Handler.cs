using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateLessonCommand> _validator;

    public CreateLessonCommandHandler(IGymDbContext dbContext, IValidator<CreateLessonCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var coach = await _dbContext.Coaches
            .FirstOrDefaultAsync(candidate => candidate.Id == request.CoachId && candidate.IsActive, cancellationToken);
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(candidate => candidate.Id == request.LocationId && candidate.IsActive, cancellationToken);

        if (coach is null || location is null)
        {
            throw new ValidationException("Coach or location not found.");
        }

        LessonTheme? theme = null;
        if (request.ThemeId.HasValue)
        {
            theme = await _dbContext.LessonThemes
                .FirstOrDefaultAsync(candidate => candidate.Id == request.ThemeId.Value && candidate.IsActive, cancellationToken);
            if (theme is null)
            {
                throw new ValidationException("Theme not found.");
            }
        }

        Lesson lesson;
        if (request.Type == LessonType.Collective)
        {
            lesson = new CollectiveLesson
            {
                MaxParticipants = request.MaxParticipants ?? 1,
                Locations = [location]
            };
        }
        else
        {
            lesson = new PrivateLesson
            {
                Location = location
            };
        }

        lesson.Name = request.Name.Trim();
        lesson.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        lesson.Type = request.Type;
        lesson.Theme = theme;
        lesson.Coach = coach;
        lesson.StartDate = request.StartDate;
        lesson.EndDate = request.EndDate;

        _dbContext.Lessons.Add(lesson);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
}
