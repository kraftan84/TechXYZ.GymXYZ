using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.WebApp.Components.Shared;

namespace TechXyz.GymXyz.WebApp.Components.Features.Reglages;

/// <summary>
/// How the Réglages screens word what the model stores. The wordings are the
/// hand-off's own.
/// </summary>
public static class SettingsLabels
{
    // ---- Notifications ------------------------------------------------------

    public static string Title(NotificationKey key) => key switch
    {
        NotificationKey.RenewalReminder => "Relance avant échéance",
        NotificationKey.LatePayment => "Paiement en retard",
        NotificationKey.NewRegistration => "Nouvelle inscription",
        NotificationKey.CourseReminder => "Rappel de cours",
        NotificationKey.SeatFreed => "Place libérée",
        _ => "Annulation de cours"
    };

    /// <summary>Who it goes to and when — the line under each switch.</summary>
    public static string Description(NotificationKey key) => key switch
    {
        NotificationKey.RenewalReminder => "Au membre, 7 jours avant la fin de son abonnement.",
        NotificationKey.LatePayment => "À vous, dès qu'un prélèvement est rejeté.",
        NotificationKey.NewRegistration => "À vous, à chaque nouveau membre enregistré.",
        NotificationKey.CourseReminder => "Au membre, 2 heures avant un cours réservé.",
        NotificationKey.SeatFreed => "À la liste d'attente quand une place se libère.",
        _ => "Aux inscrits si un cours est annulé."
    };

    public static string Title(NotificationGroup group) => group switch
    {
        NotificationGroup.MembersAndSubscriptions => "Membres & abonnements",
        _ => "Cours & présences"
    };

    public static string Label(NotificationChannels channel) =>
        channel == NotificationChannels.Sms ? "SMS" : "Email";

    /// <summary>
    /// Why the SMS switch persists without sending. Shown once per panel rather
    /// than beside every row: six copies of the same caveat is noise.
    /// </summary>
    public const string SmsPending =
        "L'e-mail part réellement. Le SMS est enregistré mais pas encore envoyé : aucun opérateur "
        + "n'est raccordé pour l'instant, et vos choix s'appliqueront dès qu'il le sera.";

    // ---- Team & access ------------------------------------------------------

    public static string RoleLabel(string role) => role switch
    {
        GymRoleNames.GymManager => "Gestionnaire",
        GymRoleNames.Coach => "Coach",
        GymRoleNames.PlatformAdmin => "Admin TechXYZ",
        _ => "Membre"
    };

    public static GxTone RoleTone(string role) => role switch
    {
        GymRoleNames.GymManager => GxTone.Brand,
        GymRoleNames.Coach => GxTone.Neutral,
        GymRoleNames.PlatformAdmin => GxTone.Warning,
        _ => GxTone.Neutral
    };

    public static string Label(MemberAccessState state) => state switch
    {
        MemberAccessState.Active => "Actif",
        MemberAccessState.Invited => "Invitation envoyée",
        MemberAccessState.Revoked => "Accès retiré",
        _ => "Sans accès"
    };

    public static GxTone Tone(MemberAccessState state) => state switch
    {
        MemberAccessState.Active => GxTone.Success,
        MemberAccessState.Invited => GxTone.Warning,
        MemberAccessState.Revoked => GxTone.Danger,
        _ => GxTone.Neutral
    };

    /// <summary>
    /// "Vu aujourd'hui", "il y a 2 h", "jamais" — the relative wording the team
    /// rows print. Days rather than exact times past a day: nobody needs to know
    /// a coach signed in at 14:07 last Tuesday.
    /// </summary>
    public static string LastSeen(DateTime? lastSeenAt, DateTime now)
    {
        if (lastSeenAt is not { } seen)
        {
            return "jamais";
        }

        var elapsed = now - seen;

        return elapsed switch
        {
            { TotalMinutes: < 2 } => "à l'instant",
            { TotalHours: < 1 } => $"il y a {(int)elapsed.TotalMinutes} min",
            { TotalHours: < 24 } => $"il y a {(int)elapsed.TotalHours} h",
            { TotalDays: < 2 } => "hier",
            { TotalDays: < 31 } => $"il y a {(int)elapsed.TotalDays} j",
            _ => $"le {seen:dd/MM/yyyy}"
        };
    }

    // ---- Opening hours ------------------------------------------------------

    private static readonly string[] DayNames =
        ["dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi"];

    public static string DayName(DayOfWeek day) => DayNames[(int)day];

    /// <summary>« Lundi – vendredi », « Samedi ». The first letter is capitalised.</summary>
    public static string DayRange(DayOfWeek from, DayOfWeek to)
    {
        var start = Capitalise(DayName(from));
        return from == to ? start : $"{start} – {DayName(to)}";
    }

    public static string HoursRange(TimeOnly opensAt, TimeOnly closesAt) =>
        $"{opensAt:HH\\:mm} – {closesAt:HH\\:mm}";

    // ---- Money --------------------------------------------------------------

    /// <summary>
    /// The currencies the panel offers. Printed as the hand-off writes them,
    /// stored as the ISO code so an amount can be formatted without parsing.
    /// </summary>
    public static readonly (string Code, string Label)[] Currencies =
    [
        ("EUR", "Euro (€)"),
        ("CHF", "Franc suisse (CHF)"),
        ("CAD", "Dollar canadien (CA$)")
    ];

    public static string CurrencyLabel(string? code) =>
        Currencies.FirstOrDefault(currency => currency.Code == code).Label ?? code ?? "—";

    /// <summary>
    /// The methods in display order — the order the encaissement drawer offers
    /// them in, so the two lists read the same way round.
    /// </summary>
    public static readonly PaymentMethod[] PaymentMethods =
    [
        PaymentMethod.Card,
        PaymentMethod.SepaDirectDebit,
        PaymentMethod.Cash,
        PaymentMethod.Cheque,
        PaymentMethod.PaymentLink
    ];

    public static string Icon(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => GxIconPaths.Card,
        PaymentMethod.SepaDirectDebit => GxIconPaths.Refresh,
        PaymentMethod.Cash => GxIconPaths.Euro,
        PaymentMethod.Cheque => GxIconPaths.Copy,
        _ => GxIconPaths.Send
    };

    private static string Capitalise(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
