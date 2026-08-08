using Microsoft.Extensions.Caching.Memory;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// One submission per address per minute, on the forms a stranger can reach.
/// <para>
/// Both of them post by e-mail address and neither asks who is calling, so
/// without this anybody can have a mailbox they do not own filled at the speed of
/// a held-down key — and it is the sending domain that wears the reputation
/// damage, not them. The space request adds a second cost: every send is a row in
/// a queue somebody at GymXYZ has to empty by hand.
/// </para>
/// <para>
/// In memory, which means per process: enough for a single shared-hosting
/// instance, and honest about it. What it is not is a defence against a
/// distributed attacker, and it is not pretending to be one.
/// </para>
/// </summary>
public sealed class PublicFormThrottle
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    private readonly IMemoryCache _cache;

    public PublicFormThrottle(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// True when this address may submit now, and starts its clock. False when one
    /// already went through inside the window.
    /// </summary>
    /// <param name="scope">
    /// Which form is asking. Kept apart so a password reset does not silence a
    /// space request from the same person, which is a perfectly ordinary thing to
    /// do in the same minute.
    /// </param>
    public bool TryBegin(string scope, string email)
    {
        var key = $"{scope}:{email.Trim().ToLowerInvariant()}";

        if (_cache.TryGetValue(key, out _))
        {
            return false;
        }

        _cache.Set(key, true, Window);

        return true;
    }

    /// <summary>Scopes, named once so a typo cannot silently create a third.</summary>
    public const string PasswordReset = "password-reset";

    public const string SpaceRequest = "space-request";
}
