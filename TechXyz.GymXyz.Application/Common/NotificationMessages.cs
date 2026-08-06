using System.Globalization;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What the three messages of this lot actually say. Plain text, French, signed
/// by the gym — these are read by members, so they follow the same rule as the
/// screens.
/// <para>
/// Text and not HTML on purpose: a member reads two sentences and a date, every
/// client renders it, and there is no template to keep in step with a brand that
/// changes per customer. When a message needs a layout, it needs a designer
/// first.
/// </para>
/// </summary>
public static class NotificationMessages
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>Chasing a cover that is late or about to run out.</summary>
    public static EmailMessage RenewalReminder(
        string gymName,
        string memberFirstName,
        string toAddress,
        string? toName,
        string planName,
        DateOnly endsOn,
        bool isLate)
    {
        var subject = isLate
            ? $"{gymName} — votre abonnement a expiré"
            : $"{gymName} — votre abonnement arrive à échéance";

        var opening = isLate
            ? $"Votre formule « {planName} » a pris fin le {Date(endsOn)} et n'a pas encore été renouvelée."
            : $"Votre formule « {planName} » prend fin le {Date(endsOn)}.";

        var body =
            $"""
             Bonjour {memberFirstName},

             {opening}

             Passez nous voir à l'accueil ou répondez à ce message pour la renouveler.

             À très vite,
             {gymName}
             """;

        return new EmailMessage(toAddress, toName, subject, body);
    }

    /// <summary>Telling somebody who had a seat that the session is off.</summary>
    public static EmailMessage CourseCancelled(
        string gymName,
        string memberFirstName,
        string toAddress,
        string? toName,
        string courseName,
        DateTime startsAt,
        string? reason)
    {
        var explanation = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}Motif : {reason}";

        var body =
            $"""
             Bonjour {memberFirstName},

             Le cours « {courseName} » du {DateAndTime(startsAt)} est annulé.{explanation}

             Votre place vous reste acquise pour une prochaine séance, et votre carnet n'a pas été décompté.

             Toutes nos excuses,
             {gymName}
             """;

        return new EmailMessage(
            toAddress,
            toName,
            $"{gymName} — cours annulé : {courseName}",
            body);
    }

    /// <summary>Asking somebody who has stopped coming whether all is well.</summary>
    public static EmailMessage AbsenceChase(
        string gymName,
        string memberFirstName,
        string toAddress,
        string? toName,
        DateOnly? lastSeenOn)
    {
        var since = lastSeenOn is { } seen
            ? $"Nous ne vous avons pas vu depuis le {Date(seen)}."
            : "Nous ne vous avons pas vu depuis un moment.";

        // Deliberately not a reproach: the gym is asking, not invoicing.
        var body =
            $"""
             Bonjour {memberFirstName},

             {since} Tout va bien ?

             Si vous souhaitez reprendre, répondez à ce message : nous trouverons un créneau qui vous convient.

             À bientôt,
             {gymName}
             """;

        return new EmailMessage(
            toAddress,
            toName,
            $"{gymName} — on ne vous voit plus",
            body);
    }

    private static string Date(DateOnly value) => value.ToString("d MMMM yyyy", French);

    private static string DateAndTime(DateTime value) => value.ToString("dddd d MMMM 'à' HH'h'mm", French);
}
