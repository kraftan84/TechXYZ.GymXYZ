using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// How the three screens report what left the building. In one place because
/// they report the same four situations, and « 2 membres n'ont pas pu être
/// prévenus » must not become « 2 échecs » one screen over.
/// </summary>
public static class NotificationOutcomeLabels
{
    /// <summary>
    /// The line to show, or null when the plain success message already says
    /// everything — which is the ordinary case.
    /// </summary>
    public static string? Warning(NotificationOutcomeDto outcome, string done)
    {
        if (!outcome.IsSaved)
        {
            return null;
        }

        if (outcome.WasSuppressed)
        {
            return $"{done} Aucun message envoyé : cette notification est désactivée dans les réglages.";
        }

        if (!outcome.HasFailures)
        {
            return null;
        }

        var failures = outcome.Failed == 1
            ? "1 membre n'a pas pu être prévenu"
            : $"{outcome.Failed} membres n'ont pas pu être prévenus";

        return outcome.Sent > 0
            ? $"{done} {Sent(outcome.Sent)} prévenu{(outcome.Sent > 1 ? "s" : string.Empty)}, mais {failures}."
            : $"{done} {Capitalise(failures)}.";
    }

    /// <summary>What to say when everything worked, so the toast is not silent.</summary>
    public static string Success(NotificationOutcomeDto outcome, string done) =>
        outcome.Sent > 0
            ? $"{done} {Sent(outcome.Sent)} prévenu{(outcome.Sent > 1 ? "s" : string.Empty)}."
            : done;

    private static string Sent(int count) => count == 1 ? "1 membre" : $"{count} membres";

    private static string Capitalise(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
