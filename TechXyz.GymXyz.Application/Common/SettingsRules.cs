namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What the Réglages screens accept, and the refusals in the words the user
/// reads. In one place so the four panels cannot disagree about what a valid
/// configuration is.
/// </summary>
public static class SettingsRules
{
    public const string NameRequired = "Donnez un nom à la salle.";

    public const string CapacityOutOfRange =
        "La capacité d'accueil doit être comprise entre 1 et 100 000 membres.";

    public const string EmailInvalid = "Cette adresse e-mail n'est pas valide.";

    public const string ZipCodeInvalid = "Un code postal français compte cinq chiffres.";

    public const string ClosingBeforeOpening =
        "L'heure de fermeture doit être postérieure à l'heure d'ouverture.";

    public const string DayRangeReversed =
        "Le premier jour de la plage doit précéder le dernier.";

    public const string CurrencyInvalid = "La devise se note sur trois lettres — EUR, CHF, CAD.";

    public const string NoPaymentMethod =
        "Gardez au moins un moyen de paiement : sans cela, aucun encaissement ne peut être enregistré.";

    public const string TenantNotFound = "Salle introuvable.";

    public const string InvitationEmailRequired = "Indiquez l'adresse e-mail à inviter.";

    public const string InvitationAlreadySent =
        "Une invitation est déjà en attente pour cette adresse.";

    public const string AccountAlreadyExists =
        "Cette adresse a déjà un compte : gérez son accès depuis la liste.";

    public const string RoleNotAssignable =
        "Ce rôle ne peut pas être attribué depuis les réglages.";

    public const string AccountNotFound = "Compte introuvable.";

    public const string InvitationNotFound = "Invitation introuvable.";

    public const string CannotRevokeSelf =
        "Vous ne pouvez pas retirer votre propre accès : demandez-le à un autre gestionnaire.";

    public const string LastManagerStands =
        "Gardez au moins un gestionnaire : sans lui, plus personne ne pourrait administrer la salle.";

    public const string NotificationUnknown = "Réglage de notification inconnu.";

    public const string ChannelRequired =
        "Choisissez au moins un canal, ou désactivez la notification.";
}
