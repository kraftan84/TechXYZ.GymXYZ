using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateCourseTemplateCommand : IRequest<bool>
{
    public UpdateCourseTemplateCommand(
        int id,
        string name,
        int disciplineId,
        int durationMinutes,
        int capacity,
        CourseLevel level,
        CourseIntensity intensity,
        int? defaultRoomId = null,
        decimal? price = null,
        string? description = null,
        string? iconKey = null,
        IReadOnlyList<int>? coachIds = null)
    {
        Id = id;
        Name = name;
        DisciplineId = disciplineId;
        DurationMinutes = durationMinutes;
        Capacity = capacity;
        Level = level;
        Intensity = intensity;
        DefaultRoomId = defaultRoomId;
        Price = price;
        Description = description;
        IconKey = iconKey;
        CoachIds = coachIds;
    }

    public int Id { get; }
    public string Name { get; }
    public int DisciplineId { get; }
    public int DurationMinutes { get; }
    public int Capacity { get; }
    public CourseLevel Level { get; }
    public CourseIntensity Intensity { get; }

    /// <summary>
    /// Applied as given: passing null is how the default studio is cleared, and
    /// the drawer always sends the value it shows.
    /// </summary>
    public int? DefaultRoomId { get; }

    /// <summary>
    /// Applied as given too: null is not "unchanged", it is "included in the
    /// subscription", which is the value the drawer's empty price field means.
    /// </summary>
    public decimal? Price { get; }

    public string? Description { get; }
    public string? IconKey { get; }

    /// <summary>Replaces the whole set when given; left untouched when null.</summary>
    public IReadOnlyList<int>? CoachIds { get; }
}
