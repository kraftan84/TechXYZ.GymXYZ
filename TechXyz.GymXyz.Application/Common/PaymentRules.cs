namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What an encaissement has to satisfy, and the refusals in the words the user
/// reads.
/// </summary>
public static class PaymentRules
{
    public const string AmountRequired = "Le montant doit être supérieur à zéro.";

    public const string DateInTheFuture =
        "Un encaissement se saisit après coup : la date ne peut pas être dans le futur.";

    public const string MemberNotFound = "Membre introuvable.";

    public const string SubscriptionNotFound = "Abonnement introuvable.";

    public const string SubscriptionNotOwned =
        "Cet abonnement n'appartient pas à ce membre.";

    public const string NothingToChase =
        "Cet abonnement est à jour : il n'y a rien à relancer.";

    /// <summary>
    /// The tooltip on « Relancer », drawn disabled until messaging arrives. The
    /// same grounds — and very nearly the same sentence — lot 6 used for the
    /// button of the same name on the absentees card.
    /// </summary>
    public const string ReminderHasNoChannel =
        "La relance sera envoyée quand la messagerie arrivera avec les Réglages.";
}
