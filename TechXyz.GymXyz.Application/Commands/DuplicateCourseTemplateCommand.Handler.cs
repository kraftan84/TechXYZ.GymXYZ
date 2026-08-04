using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DuplicateCourseTemplateCommandHandler : IRequestHandler<DuplicateCourseTemplateCommand, int?>
{
    /// <summary>Suffix the copy carries, as the prototype writes it.</summary>
    private const string CopySuffix = " (copie)";

    private const int NameMaximumLength = 120;

    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DuplicateCourseTemplateCommand> _validator;

    public DuplicateCourseTemplateCommandHandler(
        IGymDbContext dbContext,
        IValidator<DuplicateCourseTemplateCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int?> Handle(DuplicateCourseTemplateCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var source = await _dbContext.CourseTemplates
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Name,
                candidate.DisciplineId,
                candidate.IconKey,
                candidate.DurationMinutes,
                candidate.Capacity,
                candidate.DefaultRoomId,
                candidate.Level,
                candidate.Intensity,
                candidate.Price,
                candidate.Description,
                CoachIds = candidate.Coaches!
                    .Where(link => link.IsActive && link.Coach!.IsActive)
                    .OrderBy(link => link.Rank)
                    .Select(link => link.CoachId)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return null;
        }

        var copy = new CourseTemplate(BuildCopyName(source.Name))
        {
            DisciplineId = source.DisciplineId,
            IconKey = source.IconKey,
            DurationMinutes = source.DurationMinutes,
            Capacity = source.Capacity,
            DefaultRoomId = source.DefaultRoomId,
            Level = source.Level,
            Intensity = source.Intensity,
            Price = source.Price,
            Description = source.Description
        };

        // Through the navigation: the copy has no key yet, and EF fixes the
        // foreign keys up on insert.
        copy.Coaches = source.CoachIds
            .Select((coachId, rank) => new CourseTemplateCoach { CoachId = coachId, Rank = rank })
            .ToList();

        _dbContext.CourseTemplates.Add(copy);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return copy.Id;
    }

    /// <summary>
    /// The suffix must fit: a name already at the limit would otherwise produce
    /// a copy the update command refuses to save.
    /// </summary>
    private static string BuildCopyName(string name)
    {
        var maximumStem = NameMaximumLength - CopySuffix.Length;

        return name.Length <= maximumStem
            ? name + CopySuffix
            : name[..maximumStem] + CopySuffix;
    }
}
