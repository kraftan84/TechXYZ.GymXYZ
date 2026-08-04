using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Features.Cours;

/// <summary>
/// What the create / edit drawer binds to. The price is a <c>decimal?</c>: an
/// empty field means the course is included in the subscription, which is what
/// the catalogue shows as "Inclus".
/// </summary>
public sealed class CourseEditModel
{
    public string Name { get; set; } = string.Empty;
    public int DisciplineId { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public int Capacity { get; set; } = 16;
    public int? DefaultRoomId { get; set; }
    public CourseLevel Level { get; set; } = CourseLevel.AllLevels;
    public CourseIntensity Intensity { get; set; } = CourseIntensity.Moderate;
    public decimal? Price { get; set; }
    public string? Description { get; set; }

    /// <summary>Only when the course wants a different icon from its discipline.</summary>
    public string? IconKey { get; set; }

    /// <summary>
    /// Selected coaches, in the order they were picked: that is the order their
    /// avatars stack in on the catalogue row.
    /// </summary>
    public List<int> CoachIds { get; init; } = [];

    public static CourseEditModel ForCreate() => new();

    public static CourseEditModel From(CourseTemplateDetailsPageDto course)
    {
        var model = new CourseEditModel
        {
            Name = course.Name,
            DisciplineId = course.DisciplineId,
            DurationMinutes = course.DurationMinutes,
            Capacity = course.Capacity,
            DefaultRoomId = course.DefaultRoomId,
            Level = course.Level,
            Intensity = course.Intensity,
            Price = course.Price,
            Description = course.Description,
            IconKey = course.IconKeyOverride
        };

        // Kept in display order: the drawer edits the rank by reordering.
        model.CoachIds.AddRange(course.Coaches.Select(coach => coach.Id));

        return model;
    }
}
