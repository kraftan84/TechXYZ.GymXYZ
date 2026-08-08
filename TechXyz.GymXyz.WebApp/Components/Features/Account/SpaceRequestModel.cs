using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Features.Account;

/// <summary>
/// What the six steps are filling in, and what each of them refuses to leave.
/// <para>
/// The prototype validated nothing: every "Continuer" advanced. Here a step
/// checks its own fields and says so under them, using the same sentences the
/// server's validator uses — so an error caught on screen and the same error
/// caught at the endpoint read identically.
/// </para>
/// </summary>
public sealed class SpaceRequestModel
{
    public const int StepCount = 6;

    public SpaceRequestType Type { get; set; } = SpaceRequestType.Gym;

    public bool IsSolo => Type == SpaceRequestType.Coach;

    // ---- Step 2 -------------------------------------------------------------

    public string Name { get; set; } = string.Empty;

    public string Siret { get; set; } = string.Empty;

    public string SizeLabel { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string AreaLabel { get; set; } = string.Empty;

    public string Disciplines { get; set; } = string.Empty;

    // ---- Step 3 -------------------------------------------------------------
    //
    // No password field. It was in the mock, and the decision taken before this
    // lot was to send an activation link at opening instead: nothing to store,
    // no dormant secret, and a refused request leaves no hash behind. A field
    // that fed nothing would have been worse than its absence.

    public string ContactFirstName { get; set; } = string.Empty;

    public string ContactLastName { get; set; } = string.Empty;

    public string ContactRole { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    // ---- Step 4 and 5 -------------------------------------------------------

    public string RequestedPlan { get; set; } = PlatformPlans.Default;

    public string AccentHex { get; set; } = "#00ABFC";

    public string AccentLabel { get; set; } = "Azure";

    public string RequestedSubdomain { get; set; } = string.Empty;

    // ---- Step 6 -------------------------------------------------------------

    public bool AcceptedTerms { get; set; }

    public bool AcceptedDataProcessing { get; set; }

    public bool OptedIntoNewsletter { get; set; }

    public bool CanSubmit => AcceptedTerms && AcceptedDataProcessing;

    /// <summary>
    /// The honeypot's landing place. Never shown, never focusable, and empty for
    /// every human who ever fills this form.
    /// </summary>
    public string Website { get; set; } = string.Empty;

    public static IReadOnlyList<string> Sizes { get; } =
    [
        "Moins de 50 membres",
        "50 à 150 membres",
        "150 à 400 membres",
        "400 à 800 membres",
        "Plus de 800 membres"
    ];

    public static IReadOnlyList<string> SoloSizes { get; } =
    [
        "Moins de 20 clients",
        "20 à 50 clients",
        "50 à 120 clients",
        "Plus de 120 clients"
    ];

    public static IReadOnlyList<string> Roles { get; } =
    [
        "Gérant·e",
        "Responsable administratif",
        "Coach & gérant·e",
        "Président·e d'association",
        "Autre"
    ];

    public static IReadOnlyList<(string Label, string Hex)> Accents { get; } =
    [
        ("Azure", "#00ABFC"),
        ("Graphite", "#232327"),
        ("Rose", "#CB5B74"),
        ("Sauge", "#7E8E64"),
        ("Ambre", "#D08A2C"),
        ("Indigo", "#4C5BD4")
    ];

    /// <summary>
    /// What is wrong on this step, keyed by field. Empty means the step may be
    /// left — which is the only thing "Continuer" asks.
    /// </summary>
    public IReadOnlyDictionary<string, string> Validate(int step)
    {
        var errors = new Dictionary<string, string>();

        switch (step)
        {
            case 1:
                if (string.IsNullOrWhiteSpace(Name))
                {
                    errors[nameof(Name)] = IsSolo
                        ? SpaceRequestRules.SoloNameRequired
                        : SpaceRequestRules.NameRequired;
                }

                // Optional, but a wrong one quietly picks the wrong school-holiday
                // zone for the future space — which nobody would ever trace back
                // to a typo made here.
                if (!IsSolo
                    && !string.IsNullOrWhiteSpace(ZipCode)
                    && (ZipCode.Trim().Length != 5 || !ZipCode.Trim().All(char.IsAsciiDigit)))
                {
                    errors[nameof(ZipCode)] = SpaceRequestRules.ZipCodeInvalid;
                }

                break;

            case 2:
                if (string.IsNullOrWhiteSpace(ContactFirstName))
                {
                    errors[nameof(ContactFirstName)] = SpaceRequestRules.FirstNameRequired;
                }

                if (string.IsNullOrWhiteSpace(ContactLastName))
                {
                    errors[nameof(ContactLastName)] = SpaceRequestRules.LastNameRequired;
                }

                if (string.IsNullOrWhiteSpace(ContactEmail))
                {
                    errors[nameof(ContactEmail)] = SpaceRequestRules.EmailRequired;
                }
                else if (!ContactEmail.Contains('@', StringComparison.Ordinal)
                         || ContactEmail.Trim().EndsWith('@'))
                {
                    errors[nameof(ContactEmail)] = SpaceRequestRules.EmailInvalid;
                }

                break;

            case 4:
                if (string.IsNullOrWhiteSpace(RequestedSubdomain))
                {
                    errors[nameof(RequestedSubdomain)] = SpaceRequestRules.SubdomainRequired;
                }
                else if (!Subdomains.IsWellFormed(RequestedSubdomain))
                {
                    errors[nameof(RequestedSubdomain)] = SpaceRequestRules.SubdomainInvalid;
                }
                else if (Subdomains.IsReserved(RequestedSubdomain))
                {
                    errors[nameof(RequestedSubdomain)] = SpaceRequestRules.SubdomainReserved;
                }

                break;
        }

        return errors;
    }
}
