using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// How a standing, a payment and a payment method are written and tinted,
/// everywhere they appear.
/// <para>
/// The wordings are the prototype's own — "Actif", "Expire bientôt",
/// "En retard", "Encaissé", "Rejeté". They live in one place for the reason
/// <see cref="AttendanceLabels"/> does: the member record shows them today, the
/// abonnements screen shows them next, and a subscription chipped "En retard" on
/// one screen and "Impayé" on another would read as two different things.
/// </para>
/// </summary>
public static class SubscriptionLabels
{
    public static string Label(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Active => "Actif",
        SubscriptionStatus.ExpiringSoon => "Expire bientôt",
        SubscriptionStatus.Late => "En retard",
        _ => "Terminé"
    };

    public static GxTone Tone(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Active => GxTone.Success,
        SubscriptionStatus.ExpiringSoon => GxTone.Warning,
        SubscriptionStatus.Late => GxTone.Danger,
        _ => GxTone.Neutral
    };

    public static string Label(PaymentStatus status) => status switch
    {
        PaymentStatus.Collected => "Encaissé",
        PaymentStatus.Rejected => "Rejeté",
        _ => "En attente"
    };

    public static GxTone Tone(PaymentStatus status) => status switch
    {
        PaymentStatus.Collected => GxTone.Success,
        PaymentStatus.Rejected => GxTone.Danger,
        _ => GxTone.Neutral
    };

    /// <summary>
    /// The methods in the words the drawer offers and the encaissements list
    /// prints — "Prélèvement", not "SepaDirectDebit".
    /// </summary>
    public static string Label(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => "Carte",
        PaymentMethod.SepaDirectDebit => "Prélèvement",
        PaymentMethod.Cash => "Espèces",
        PaymentMethod.Cheque => "Chèque",
        _ => "Lien de paiement"
    };
}
