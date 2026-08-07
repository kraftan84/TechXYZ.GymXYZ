namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Where the outside world lives. Configured under <c>ExternalApis</c>, with a
/// default for every value here so the application still starts with no
/// configuration at all.
/// <para>
/// Out of the service body because a public API moves — the school holidays are
/// already served from the second version of their endpoint — and a version bump
/// should be a setting rather than a deployment. It also lets a test point the
/// service at a server that is deliberately broken, which is the only honest way
/// to show what happens when the source is down.
/// </para>
/// </summary>
public sealed class ExternalApiOptions
{
    public const string SectionName = "ExternalApis";

    /// <summary>
    /// Etalab's public holidays for métropole. <c>{year}</c> is substituted with
    /// the year being asked for.
    /// </summary>
    public string PublicHolidaysUrl { get; set; } =
        "https://calendrier.api.gouv.fr/jours-feries/metropole/{year}.json";

    /// <summary>
    /// The Éducation nationale's school holidays, on Opendatasoft. Only the
    /// endpoint is configurable: the query string encodes which zone and which
    /// window are wanted, which is the service's business and not an operator's.
    /// </summary>
    public string SchoolVacationsUrl { get; set; } =
        "https://data.education.gouv.fr/api/explore/v2.1/catalog/datasets/"
        + "fr-en-calendrier-scolaire/records";

    /// <summary>
    /// How long the whole calendar read may take. A screen is waiting on it —
    /// better no banner than a slow page.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 4;

    /// <summary>
    /// Both sources publish once a year and never move, so a day in memory is
    /// conservative.
    /// </summary>
    public int CacheHours { get; set; } = 24;
}
