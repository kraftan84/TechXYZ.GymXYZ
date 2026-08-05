namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The bounds a venue stays inside, written once so create and update cannot
/// drift apart. The wordings are what the user reads on the drawer.
/// </summary>
public static class LocationRules
{
    public const int MinimumCapacity = 1;
    public const int MaximumCapacity = 500;

    public const decimal MinimumAreaSqm = 1m;
    public const decimal MaximumAreaSqm = 10_000m;

    /// <summary>A session at the member's home is one person, by definition.</summary>
    public const int HomeCapacity = 1;

    public static readonly string CapacityMessage =
        $"La capacité doit être comprise entre {MinimumCapacity} et {MaximumCapacity} places.";

    public static readonly string AreaMessage =
        $"La surface doit être comprise entre {MinimumAreaSqm:0} et {MaximumAreaSqm:0} m².";

    public const string HomeCapacityMessage =
        "Une séance à domicile ne reçoit qu'une personne : la capacité doit être 1.";

    public const string FallbackSelfMessage =
        "Un lieu ne peut pas se replier sur lui-même.";

    public const string FallbackKindMessage =
        "Le lieu de repli doit être une salle : se replier dehors n'abrite de rien.";

    public const string WeatherKindMessage =
        "Seul un lieu en extérieur peut dépendre de la météo.";

    public const string HomeAddressMessage =
        "Une séance à domicile n'a pas d'adresse propre : elle est renseignée sur la fiche du membre.";
}
