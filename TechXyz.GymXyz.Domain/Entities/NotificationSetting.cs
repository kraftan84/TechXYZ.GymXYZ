using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One switch of the Notifications panel: whether a given message goes out, and
/// down which channels.
/// <para>
/// A row per message per customer rather than a column per message on
/// <see cref="GymSettings"/>: the list grows every time the product learns to
/// say something new, and a table grows by a row where a schema grows by a
/// migration.
/// </para>
/// </summary>
public class NotificationSetting : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    public NotificationGroup Group { get; set; }

    public NotificationKey Key { get; set; }

    public bool IsEnabled { get; set; }

    public NotificationChannels Channels { get; set; }

    /// <summary>
    /// Whether this message may go out down that channel right now. Both halves
    /// have to agree: a setting switched off sends nothing whatever its channels
    /// say, and a channel left unticked is not a channel.
    /// </summary>
    /// <remarks>
    /// <see cref="NotificationChannels.None"/> answers false rather than true:
    /// <c>HasFlag</c> alone would say every setting allows the empty channel.
    /// </remarks>
    public bool Allows(NotificationChannels channel) =>
        channel != NotificationChannels.None
        && IsActive
        && IsEnabled
        && Channels.HasFlag(channel);
}
