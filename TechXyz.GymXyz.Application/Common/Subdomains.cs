using System.Text.RegularExpressions;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What may become <c>{ceci}.gymxyz.fr</c>.
/// <para>
/// The prototype normalised as you typed and never checked anything. Both halves
/// are needed in production, and they belong together: the screen and the server
/// have to agree on what a legal prefix is, or the form accepts a name the
/// command then refuses for reasons the person cannot see.
/// </para>
/// </summary>
public static partial class Subdomains
{
    /// <summary>
    /// Names that must never become a customer's, because the platform answers on
    /// them or expects to. Handing out <c>admin.gymxyz.fr</c> to whoever asks
    /// first is not a naming inconvenience — it is a phishing page on the
    /// product's own domain, served over the product's own certificate.
    /// </summary>
    public static IReadOnlySet<string> Reserved { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "www", "api", "app", "admin", "console", "mail", "smtp", "imap", "pop",
        "static", "assets", "cdn", "img", "images", "media", "files", "download",
        "support", "help", "docs", "doc", "status", "blog", "shop", "store",
        "auth", "login", "signin", "account", "compte", "connexion",
        "dev", "test", "staging", "preprod", "prod", "demo", "sandbox",
        "gymxyz", "techxyz", "ns", "ns1", "ns2", "mx", "ftp", "vpn", "webmail"
    };

    /// <summary>Shortest prefix worth registering — two letters is already terse.</summary>
    public const int MinimumLength = 3;

    /// <summary>What fits a DNS label with room to spare.</summary>
    public const int MaximumLength = 40;

    /// <summary>
    /// What the field does as you type: lowercase, and nothing but letters,
    /// digits and dashes. The same function the server applies before storing, so
    /// a pasted "Atlas Training !" becomes the same thing on both sides.
    /// </summary>
    public static string Normalise(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? string.Empty
            : IllegalCharacters().Replace(raw.Trim().ToLowerInvariant(), string.Empty);

    /// <summary>
    /// Whether the shape is right — says nothing about whether it is taken, which
    /// only the store knows.
    /// </summary>
    public static bool IsWellFormed(string? candidate)
    {
        var value = Normalise(candidate);

        return value.Length >= MinimumLength
               && value.Length <= MaximumLength
               && !value.StartsWith('-')
               && !value.EndsWith('-')
               && !value.Contains("--", StringComparison.Ordinal);
    }

    public static bool IsReserved(string? candidate) => Reserved.Contains(Normalise(candidate));

    /// <summary>
    /// Something close enough to keep, when the first choice is taken. Offered
    /// rather than imposed: the applicant may well prefer a different word
    /// entirely, and the field stays theirs.
    /// </summary>
    public static string Suggest(string? candidate, Func<string, bool> isTaken)
    {
        var stem = Normalise(candidate);

        if (stem.Length == 0)
        {
            return string.Empty;
        }

        foreach (var suffix in new[] { "-club", "-fr", "-sport" })
        {
            var proposal = stem + suffix;

            if (proposal.Length <= MaximumLength && !IsReserved(proposal) && !isTaken(proposal))
            {
                return proposal;
            }
        }

        for (var index = 2; index < 100; index++)
        {
            var proposal = $"{stem}-{index}";

            if (!isTaken(proposal))
            {
                return proposal;
            }
        }

        return string.Empty;
    }

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex IllegalCharacters();
}
