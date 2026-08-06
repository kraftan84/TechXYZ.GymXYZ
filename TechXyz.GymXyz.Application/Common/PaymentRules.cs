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

}
