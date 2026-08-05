using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCourseTemplateCommand : IRequest<int>
{
    public CreateCourseTemplateCommand(
        string name,
        int disciplineId,
        int durationMinutes,
        int capacity,
        CourseLevel level,
        CourseIntensity intensity,
        int? defaultLocationId = null,
        decimal? price = null,
        string? description = null,
        string? iconKey = null,
        IReadOnlyList<int>? coachIds = null)
    {
        Name = name;
        DisciplineId = disciplineId;
        DurationMinutes = durationMinutes;
        Capacity = capacity;
        Level = level;
        Intensity = intensity;
        DefaultLocationId = defaultLocationId;
        Price = price;
        Description = description;
        IconKey = iconKey;
        CoachIds = coachIds;
    }

    public string Name { get; }
    public int DisciplineId { get; }
    public int DurationMinutes { get; }

    /// <summary>A capacity of one is what makes the course private.</summary>
    public int Capacity { get; }

    public CourseLevel Level { get; }
    public CourseIntensity Intensity { get; }

    /// <summary>Studio the planning proposes first. Optional.</summary>
    public int? DefaultLocationId { get; }

    /// <summary>Null means the course is included in the subscription.</summary>
    public decimal? Price { get; }

    public string? Description { get; }

    /// <summary>Only when the course wants a different icon from its discipline.</summary>
    public string? IconKey { get; }

    /// <summary>The coaches allowed to run it, in avatar-stack order.</summary>
    public IReadOnlyList<int>? CoachIds { get; }
}
