namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// The two filter chips of the catalogue. Not stored: a course is private
/// because it seats one, and deriving it is what keeps the two from drifting.
/// </summary>
public enum CourseFormat
{
    Collective,
    Private
}
