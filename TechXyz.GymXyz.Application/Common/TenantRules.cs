namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What the TechXYZ console accepts of a customer, and the refusals in the words
/// the super-admin reads. Separate from <see cref="SettingsRules"/> on purpose:
/// those are a gym's own settings, these are the platform's view of its
/// customers, and the two screens have no rule in common.
/// </summary>
public static class TenantRules
{
    public const string NameRequired = "Donnez un nom à ce client.";

    public const string SlugRequired = "Indiquez l'identifiant d'adresse du client.";

    public const string SlugInvalid =
        "L'identifiant d'adresse ne prend que des minuscules, des chiffres et des tirets — « team-trainers ».";

    public const string SlugTaken = "Cet identifiant d'adresse est déjà pris par un autre client.";

    public const string ThemeRequired = "Choisissez un thème de marque.";

    public const string PlanPriceOutOfRange =
        "Le prix de la formule doit être compris entre 0 et 10 000 € par mois.";

    public const string MemberCapOutOfRange =
        "Le plafond de membres doit être compris entre 1 et 100 000. Laissez vide pour un accès illimité.";

    public const string TenantNotFound = "Client introuvable.";
}

public static class TenantFieldNames
{
    public const string Name = "Name";
    public const string Slug = "Slug";
    public const string ThemeKey = "ThemeKey";
    public const string PlanPrice = "PlanPrice";
    public const string PlanMemberCap = "PlanMemberCap";
    public const string Tenant = "Tenant";
}
