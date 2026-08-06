using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What every screen and handler touching an attendance sheet has to agree on:
/// when a sheet is open, who may reopen a closed one, and what a refusal says.
/// Spelled out once so the four presentations and the four commands cannot
/// disagree about it.
/// </summary>
public static class AttendanceRules
{
    /// <summary>
    /// Statuses that count as the member having attended. A late arrival is an
    /// attendance — the sheet records it separately so a coach can see it, not
    /// so it counts against the member.
    /// </summary>
    public static bool CountsAsAttended(AttendanceStatus status) =>
        status is AttendanceStatus.Present or AttendanceStatus.Late;

    /// <summary>
    /// Whether the status is one somebody actually recorded. This is the divider
    /// an attendance rate is computed over: a sheet nobody pointed has no rate,
    /// not a rate of nought.
    /// </summary>
    public static bool IsMarked(AttendanceStatus status) =>
        status != AttendanceStatus.Pending;

    /// <summary>An arrival time belongs to the statuses that mean the member came.</summary>
    public static DateTime? CheckInFor(AttendanceStatus status, DateTime now) =>
        CountsAsAttended(status) ? now : null;

    /// <summary>
    /// How far back a sheet left open still shows in « à pointer ». It reaches a
    /// week back rather than stopping at today so a sheet somebody forgot on
    /// Friday is still in front of them on Monday instead of quietly
    /// disappearing.
    /// </summary>
    public const int ForgottenSheetDays = 7;

    /// <summary>That week, up to the end of today.</summary>
    public static (DateTime From, DateTime To) OpenSheetWindow(DateTime now) =>
        (now.Date.AddDays(-ForgottenSheetDays), now.Date.AddDays(1));

    /// <summary>
    /// The sheets still waiting to be pointed.
    /// <para>
    /// Written once as a query rather than twice as a predicate: the Présences
    /// screen lists these rows, the Accueil counts them, and the navigation
    /// badge draws that count. Three places showing one figure, so there is one
    /// place that decides which sessions it is.
    /// </para>
    /// <para>
    /// A cancelled session has no sheet, so it is in neither — the same
    /// exclusion <see cref="SessionStatistics.LoadAsync"/> applies to the figures.
    /// </para>
    /// </summary>
    public static IQueryable<Session> OpenSheets(IGymDbContext dbContext, DateTime now)
    {
        var (from, to) = OpenSheetWindow(now);

        return dbContext.Sessions
            .AsNoTracking()
            .Where(session =>
                session.IsActive &&
                session.Status != SessionStatus.Cancelled &&
                session.AttendanceClosedAt == null &&
                session.StartsAt >= from &&
                session.StartsAt < to);
    }

    /// <summary>The role allowed to reopen a validated sheet.</summary>
    public const string ReopenRole = GymRoleNames.GymManager;

    // Refusals, in the words the user reads.

    public const string SheetClosed =
        "Cette feuille est validée : elle ne peut plus être modifiée. Rouvrez-la pour la corriger.";

    public const string ReopenReserved =
        "Seul un responsable de la salle peut rouvrir une feuille validée.";

    public const string SheetNotClosed =
        "Cette feuille n'est pas validée : il n'y a rien à rouvrir.";

    public const string SessionCancelled =
        "Cette séance est annulée : elle n'a pas de feuille de présence.";

    public const string SessionNotStarted =
        "Cette séance n'a pas encore commencé : sa feuille ne peut pas être validée.";

    public const string SessionNotFound = "Séance introuvable.";

    public const string RegistrationNotFound = "Inscription introuvable.";

    public const string NobodyToChase = "Personne à relancer.";
}
