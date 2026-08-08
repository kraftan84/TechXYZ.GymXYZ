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

        // The platform's own, already behind the PlatformAdmin policy.
        nameof(CreateTenantCommand),
        nameof(UpdateTenantBrandingCommand),
        nameof(UpdateTenantPlanCommand),
        nameof(BeginTenantImpersonationCommand),
        nameof(EndTenantImpersonationCommand)
    ];

    [Fact]
    public void EveryCommand_ShouldSayWhichSideOfThePerimeterItIsOn()
    {
        var onDisk = AllCommands().Select(type => type.Name).ToList();

        onDisk.Except(Reserved).Except(OpenToACoach).ShouldBeEmpty(
            "A new command is either reserved to a manager or open to a coach — say which, here.");
        Reserved.Concat(OpenToACoach).Except(onDisk).ShouldBeEmpty(
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
                           && type.GetInterfaces().Any(contract =>
                               contract.IsGenericType
                               && contract.GetGenericTypeDefinition() == typeof(IRequest<>)));
}
