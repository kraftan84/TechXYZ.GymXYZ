using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateLessonCommand> _validator;

    public UpdateLessonCommandHandler(IGymDbContext dbContext, IValidator<UpdateLessonCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var coach = await _dbContext.Coaches
            .FirstOrDefaultAsync(candidate => candidate.Id == request.CoachId && candidate.IsActive, cancellationToken);
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(candidate => candidate.Id == request.LocationId && candidate.IsActive, cancellationToken);
        if (coach is null || location is null)
        {
            return false;
        }

        LessonTheme? theme = null;
        if (request.ThemeId.HasValue)
        {
            theme = await _dbContext.LessonThemes
                .FirstOrDefaultAsync(candidate => candidate.Id == request.ThemeId.Value && candidate.IsActive, cancellationToken);
            if (theme is null)
            {
                return false;
            }
        }

        var collectiveLesson = await _dbContext.CollectiveLessons
            .Include(lesson => lesson.Locations)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (collectiveLesson is not null)
        {
            if (request.Type != LessonType.Collective)
            {
                throw new ValidationException("Changing lesson type is not supported.");
            }

            ApplyCommonFields(collectiveLesson, request, coach, theme);
            collectiveLesson.MaxParticipants = request.MaxParticipants ?? 1;
            collectiveLesson.Locations = [location];

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        var privateLesson = await _dbContext.PrivateLessons
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (privateLesson is null)
        {
            return false;
        }

        if (request.Type != LessonType.Private)
        {
            throw new ValidationException("Changing lesson type is not supported.");
        }

        ApplyCommonFields(privateLesson, request, coach, theme);
        privateLesson.Location = location;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyCommonFields(Lesson lesson, UpdateLessonCommand request, Coach coach, LessonTheme? theme)
    {
        lesson.Name = request.Name.Trim();
        lesson.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        lesson.Type = request.Type;
        lesson.Theme = theme;
        lesson.Coach = coach;
        lesson.StartDate = request.StartDate;
        lesson.EndDate = request.EndDate;
    }
}
