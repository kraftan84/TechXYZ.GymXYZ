using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Whether a message is allowed out. Every send asks this first — it is the
/// point of the switches lot 8's Notifications panel stores.
/// <para>
/// A customer with no row for a message falls back to
/// <see cref="NotificationDefaults"/> rather than to silence, for the same
/// reason the settings query does: a gym created before a message existed never
/// chose to switch it off, and reading a missing row as "off" would quietly stop
/// mail nobody stopped.
/// </para>
/// </summary>
public static class NotificationPolicy
{
    public static async Task<bool> AllowsAsync(
        IGymDbContext dbContext,
        NotificationKey key,
        NotificationChannels channel,
        CancellationToken cancellationToken = default)
    {
        var stored = await dbContext.NotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.IsActive && setting.Key == key, cancellationToken);

        return (stored ?? NotificationDefaults.Create(key)).Allows(channel);
    }

    /// <summary>
    /// E-mail is the only channel that leaves the building today. Asked by name
    /// so the call sites read as what they mean, and so the day SMS is wired the
    /// change is here rather than in every handler.
    /// </summary>
    public static Task<bool> AllowsEmailAsync(
        IGymDbContext dbContext,
        NotificationKey key,
        CancellationToken cancellationToken = default)
        => AllowsAsync(dbContext, key, NotificationChannels.Email, cancellationToken);
}
