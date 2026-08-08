using Microsoft.Extensions.Caching.Memory;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// One reset link per address per minute.
/// <para>
/// The form is public and posts by e-mail address, so without this anybody can
/// have a mailbox they do not own filled at the speed of a held-down key — and
/// the sending domain wears the reputation damage, not them.
/// </para>
/// <para>
/// In memory, which means per process: enough for a single shared-hosting
/// instance, and honest about it. What it is not is a defence against a
/// distributed attacker, and it is not pretending to be one — it is the anti-spam
/// the entry spec asks for on the resend.
/// </para>
/// </summary>
public sealed class PasswordResetThrottle
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    private readonly IMemoryCache _cache;

    public PasswordResetThrottle(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// True when a link may go out now, and starts the clock. False when one
    /// already left inside the window — the screen says the same thing either
    /// way, because "you asked too recently" would confirm the address exists.
    /// </summary>
    public bool TryBegin(string email)
    {
        var key = $"pwd-reset:{email.Trim().ToLowerInvariant()}";

        if (_cache.TryGetValue(key, out _))
        {
            return false;
        }

        _cache.Set(key, true, Window);

        return true;
    }
}
