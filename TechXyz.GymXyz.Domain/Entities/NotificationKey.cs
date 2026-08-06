namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// The six messages the gym can have GymXYZ send. An enum rather than a free
/// string because every send asks this model whether it is allowed to go out:
/// a mistyped key would read as "switched off" and the message would silently
/// never leave, which is the one failure nobody notices.
/// </summary>
public enum NotificationKey
{
    /// <summary>To the member, seven days before their cover ends.</summary>
    RenewalReminder,

    /// <summary>To the gym, as soon as a direct debit comes back rejected.</summary>
    LatePayment,

    /// <summary>To the gym, on every new member registered.</summary>
    NewRegistration,

    /// <summary>To the member, two hours before a booked course.</summary>
    CourseReminder,

    /// <summary>To the waiting list, when a seat comes free.</summary>
    SeatFreed,

    /// <summary>To everybody holding a seat, when a course is called off.</summary>
    CourseCancelled
}
