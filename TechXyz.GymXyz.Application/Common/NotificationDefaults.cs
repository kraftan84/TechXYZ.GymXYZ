using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The six switches a customer starts with, in the order the panel prints them
/// and set as the hand-off draws them.
/// <para>
/// Built here rather than seeded once because a customer created before a
/// message existed has no row for it, and a missing row must not read as
/// "switched off" — the gym never chose that. The query tops up what is missing
/// and leaves what the gym has already decided alone.
/// </para>
/// </summary>
public static class NotificationDefaults
{
    private const NotificationChannels Both = NotificationChannels.Email | NotificationChannels.Sms;

    /// <summary>
    /// Definition order is display order: the panel draws the rows as they come
    /// rather than sorting them, so this is the one place the sequence lives.
    /// </summary>
    public static readonly IReadOnlyList<(NotificationGroup Group, NotificationKey Key, bool IsEnabled, NotificationChannels Channels)>
        All =
        [
            (NotificationGroup.MembersAndSubscriptions, NotificationKey.RenewalReminder, true, Both),
            (NotificationGroup.MembersAndSubscriptions, NotificationKey.LatePayment, true, NotificationChannels.Email),
            (NotificationGroup.MembersAndSubscriptions, NotificationKey.NewRegistration, false, NotificationChannels.Email),
            (NotificationGroup.CoursesAndAttendance, NotificationKey.CourseReminder, true, Both),
            (NotificationGroup.CoursesAndAttendance, NotificationKey.SeatFreed, true, Both),
            (NotificationGroup.CoursesAndAttendance, NotificationKey.CourseCancelled, true, Both)
        ];

    /// <summary>Rank of a key in the panel, used to keep the rows in that order.</summary>
    public static int RankOf(NotificationKey key)
    {
        for (var rank = 0; rank < All.Count; rank++)
        {
            if (All[rank].Key == key)
            {
                return rank;
            }
        }

        return All.Count;
    }

    public static NotificationSetting Create(NotificationKey key)
    {
        var (group, _, isEnabled, channels) = All.First(entry => entry.Key == key);

        return new NotificationSetting
        {
            Group = group,
            Key = key,
            IsEnabled = isEnabled,
            Channels = channels
        };
    }

    /// <summary>The keys a customer has no row for yet.</summary>
    public static IEnumerable<NotificationKey> Missing(IEnumerable<NotificationKey> existing)
    {
        var known = existing.ToHashSet();
        return All.Select(entry => entry.Key).Where(key => !known.Contains(key));
    }
}
