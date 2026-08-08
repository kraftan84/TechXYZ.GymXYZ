using System.Reflection;
using MediatR;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Queries;

namespace TechXYZ.GymXYZ.Application.Tests.Common;

/// <summary>
/// Which commands only a manager may run, named one by one — and, since the
/// poster, which queries too.
/// <para>
/// The list is the point. A marker interface makes the check impossible to
/// forget inside a handler, but nothing stops a new command from simply not
/// carrying it — and an unmarked command looks exactly like one that is
/// deliberately open. Pinning both sides means a command added later fails this
/// test until somebody says which side of the line it is on.
/// </para>
/// </summary>
public class ManagerOnlyPerimeterTests
{
    /// <summary>
    /// Running the gym: its settings and team, its money, its catalogue, its
    /// roster, its rooms, and the member records themselves.
    /// </summary>
    private static readonly string[] Reserved =
    [
        // Settings and team access — the worst of them: until this lot a coach
        // could open Réglages and promote themselves.
        nameof(UpdateTeamMemberAccessCommand),
        nameof(RevokeAccessCommand),
        nameof(InviteTeamMemberCommand),
        nameof(UpdateGymIdentityCommand),
        nameof(UpdatePaymentMethodsCommand),
        nameof(UpdateNotificationSettingsCommand),

        // Money.
        nameof(CreatePlanCommand),
        nameof(UpdatePlanCommand),
        nameof(DeletePlanCommand),
        nameof(RecordPaymentCommand),
        nameof(SendPaymentReminderCommand),
        nameof(AssignSubscriptionCommand),
        nameof(RenewSubscriptionCommand),
        nameof(DeleteSubscriptionCommand),

        // The people and the premises.
        nameof(CreateMemberCommand),
        nameof(UpdateMemberCommand),
        nameof(DeleteMemberCommand),
        nameof(CreateCoachCommand),
        nameof(UpdateCoachCommand),
        nameof(DeleteCoachCommand),
        nameof(CreateLocationCommand),
        nameof(UpdateLocationCommand),
        nameof(DeleteLocationCommand),
        nameof(CreateSiteCommand),
        nameof(UpdateSiteCommand),
        nameof(DeleteSiteCommand),

        // The catalogue. A coach teaches the course; they do not write it.
        nameof(CreateCourseTemplateCommand),
        nameof(UpdateCourseTemplateCommand),
        nameof(DuplicateCourseTemplateCommand),
        nameof(DeleteCourseTemplateCommand)
    ];

    /// <summary>
    /// A coach's own working day, deliberately left open. Their sessions are
    /// narrowed to their own in PR 3 — by a filter, not by this marker, because
    /// "only mine" is a different question from "only managers".
    /// </summary>
    private static readonly string[] OpenToACoach =
    [
        nameof(MarkAttendanceCommand),
        nameof(MarkWholeSheetCommand),
        nameof(CloseAttendanceSheetCommand),
        nameof(SendAbsenceReminderCommand),
        nameof(CreateSessionCommand),
        nameof(UpdateSessionCommand),
        nameof(CancelSessionCommand),

        // Reserved to a manager by its own in-handler check since lot 6, which
        // this lot left where it was rather than moving the rule twice.
        nameof(ReopenAttendanceSheetCommand),

        // The platform's own, behind the PlatformAdmin policy and marked
        // IPlatformScoped. They are not a coach's day either — they are nobody's
        // gym at all — but they belong on this side of the line for the same
        // reason: this marker asks whether the caller manages the gym being
        // served, and there is no gym being served. Since the impersonation was
        // removed, ManagerOnly refuses a platform admin outright, so carrying it
        // here would close these to the only person who may run them.
        //
        // Two more stood here, BeginTenantImpersonationCommand and its End —
        // deleted with the visit they opened.
        nameof(CreateTenantCommand),
        nameof(UpdateTenantBrandingCommand),
        nameof(UpdateTenantPlanCommand)
    ];

    /// <summary>
    /// Run by somebody who is not signed in at all — a third side of the line,
    /// added by the entry lot.
    /// <para>
    /// Named separately rather than folded into the open list because the reason
    /// differs and the scrutiny does: these two are reachable by anyone on the
    /// internet, so what protects them is not a role but the rules inside them —
    /// no answer that distinguishes a known address from an unknown one, a
    /// single-use token that expires, and a rate limit on the way in.
    /// </para>
    /// </summary>
    private static readonly string[] Anonymous =
    [
        nameof(RequestPasswordResetCommand),
        nameof(ResetPasswordCommand),

        // The onboarding form. Public in the fullest sense — it is the only write
        // in the product reachable by somebody with no account at all, which is
        // why it carries a honeypot and a rate limit instead of a role.
        nameof(SubmitSpaceRequestCommand),

        // Not public: nobody runs it at all. It is swept from a background service
        // with no principal, so demanding a manager would mean it never ran and
        // the deletion promised on the form never happened.
        nameof(PurgeRefusedSpaceRequestsCommand)
    ];

    [Fact]
    public void EveryCommand_ShouldSayWhichSideOfThePerimeterItIsOn()
    {
        var onDisk = AllCommands().Select(type => type.Name).ToList();

        onDisk.Except(Reserved).Except(OpenToACoach).Except(Anonymous).ShouldBeEmpty(
            "A new command is reserved to a manager, open to a coach, or public — say which, here.");
        Reserved.Concat(OpenToACoach).Concat(Anonymous).Except(onDisk).ShouldBeEmpty(
            "This table names a command that no longer exists.");
    }

    [Fact]
    public void TheReservedCommands_ShouldCarryTheMarker()
    {
        var marked = AllCommands()
            .Where(type => type.IsAssignableTo(typeof(IManagerOnly)))
            .Select(type => type.Name)
            .ToList();

        marked.ShouldBe(Reserved, ignoreOrder: true);
    }

    [Fact]
    public void APublicCommand_ShouldNotCarryTheMarker()
    {
        // A marker demanding a manager on a command reached from the login screen
        // would refuse everybody, and the failure would look like a broken reset
        // rather than a misplaced attribute.
        foreach (var name in Anonymous)
        {
            AllCommands()
                .Single(type => type.Name == name)
                .IsAssignableTo(typeof(IManagerOnly))
                .ShouldBeFalse($"{name} runs with nobody signed in.");
        }
    }

    [Fact]
    public void ACoachsOwnCommands_ShouldNotCarryTheMarker()
    {
        // The direction a partitioning lot gets wrong: taking away the work the
        // person actually came to do.
        foreach (var name in OpenToACoach)
        {
            AllCommands()
                .Single(type => type.Name == name)
                .IsAssignableTo(typeof(IManagerOnly))
                .ShouldBeFalse($"{name} is part of a coach's day.");
        }
    }

    /// <summary>
    /// Reading is a narrower perimeter than writing, and almost every query is
    /// open: a coach has to see the gym to work in it. The exception is the
    /// poster, where the content leaving the building is not the caller's own
    /// week but a picture of the club's.
    /// </summary>
    private static readonly string[] ReservedQueries =
    [
        nameof(GetPlanningPosterQuery)
    ];

    [Fact]
    public void EveryQuery_ShouldSayWhichSideOfThePerimeterItIsOn()
    {
        // The commands are pinned above one by one; the queries are pinned by
        // their exceptions, because listing every open query would be a list of
        // the whole application that nobody would keep true.
        var marked = AllRequests("Query")
            .Where(type => type.IsAssignableTo(typeof(IManagerOnly)))
            .Select(type => type.Name)
            .ToList();

        marked.ShouldBe(ReservedQueries, ignoreOrder: true,
            "A query reserved to a manager is unusual enough to be named here.");
    }

    private static IEnumerable<Type> AllCommands() => AllRequests("Command");

    private static IEnumerable<Type> AllRequests(string suffix) =>
        typeof(UpdateTeamMemberAccessCommand).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Name.EndsWith(suffix, StringComparison.Ordinal)
                           && type.GetInterfaces().Any(IsMediatRRequest));

    /// <summary>
    /// Both shapes MediatR offers. The non-generic <c>IRequest</c> is not an
    /// <c>IRequest&lt;Unit&gt;</c> — it is a separate interface — so a scan that
    /// looked only for the generic one let every command returning nothing walk
    /// past this table unclassified. Found when the first such command was
    /// written; the hole was older than the command.
    /// </summary>
    private static bool IsMediatRRequest(Type contract) =>
        contract == typeof(IRequest)
        || (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>));
}
