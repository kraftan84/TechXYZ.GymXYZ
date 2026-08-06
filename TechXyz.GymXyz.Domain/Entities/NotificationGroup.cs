namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// The two headings the Notifications panel groups its switches under. Stored
/// beside each setting rather than derived from the key so the panel can draw
/// its cards from the rows themselves, in the order the gym reads them.
/// </summary>
public enum NotificationGroup
{
    /// <summary>Money and cover: renewals, rejected payments, new sign-ups.</summary>
    MembersAndSubscriptions,

    /// <summary>The timetable: reminders, freed seats, cancellations.</summary>
    CoursesAndAttendance
}
