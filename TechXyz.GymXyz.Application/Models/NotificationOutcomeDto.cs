namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// What a command that also sends came back with.
/// <para>
/// Three states rather than a bool, because saving and sending can now disagree.
/// « La séance est annulée mais deux inscrits n'ont pas été prévenus » is a
/// sentence the screen has to be able to say — a bool could only claim the whole
/// thing worked or that none of it did, and the second would be a lie that
/// leaves somebody re-cancelling a session that is already off.
/// </para>
/// </summary>
public sealed record NotificationOutcomeDto(bool IsSaved, int Sent, int Failed, bool WasSuppressed)
{
    /// <summary>The write did not happen — the target was gone or inactive.</summary>
    public static NotificationOutcomeDto NotFound { get; } = new(false, 0, 0, false);

    /// <summary>Saved, and nobody had to be told.</summary>
    public static NotificationOutcomeDto SavedOnly { get; } = new(true, 0, 0, false);

    /// <summary>
    /// Saved, and the gym has this message switched off. Not a failure: the
    /// silence was asked for, and the screen says so rather than reporting an
    /// error nobody caused.
    /// </summary>
    public static NotificationOutcomeDto Suppressed { get; } = new(true, 0, 0, true);

    public static NotificationOutcomeDto Delivered(int sent, int failed) => new(true, sent, failed, false);

    /// <summary>Something was meant to go out and did not.</summary>
    public bool HasFailures => Failed > 0;
}
