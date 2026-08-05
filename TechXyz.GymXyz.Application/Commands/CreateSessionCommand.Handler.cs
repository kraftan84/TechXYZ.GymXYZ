using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateSessionCommand> _validator;

    public CreateSessionCommandHandler(IGymDbContext dbContext, IValidator<CreateSessionCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var template = await SessionCompositionHelper.LoadCourseTemplateAsync(
            _dbContext, request.CourseTemplateId, cancellationToken);
        var location = await SessionCompositionHelper.LoadLocationAsync(
            _dbContext, request.LocationId, cancellationToken);
        var coach = await SessionCompositionHelper.LoadCoachAsync(
            _dbContext, request.CoachId, cancellationToken);

        var capacity = request.Capacity ?? template.Capacity;
        SessionCompositionHelper.EnsureFitsInLocation(location, capacity);

        var endsAt = request.StartsAt.AddMinutes(template.DurationMinutes);
        var occurrences = SessionCompositionHelper.Occurrences(
            request.StartsAt, endsAt, request.RecurrenceWeeks);

        await SessionCompositionHelper.EnsureSlotsAreFreeAsync(
            _dbContext, occurrences, location.Id, coach?.Id, [], cancellationToken);

        // Null for a one-off: a series id on a single row would claim a
        // recurrence that does not exist.
        var seriesId = occurrences.Count > 1 ? Guid.NewGuid() : (Guid?)null;

        var sessions = occurrences
            .Select(occurrence => new Session
            {
                CourseTemplate = template,
                Location = location,
                Coach = coach,
                StartsAt = occurrence.StartsAt,
                EndsAt = occurrence.EndsAt,
                Capacity = capacity,
                Status = SessionStatus.Scheduled,
                SeriesId = seriesId
            })
            .ToList();

        _dbContext.Sessions.AddRange(sessions);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return sessions[0].Id;
    }
}
