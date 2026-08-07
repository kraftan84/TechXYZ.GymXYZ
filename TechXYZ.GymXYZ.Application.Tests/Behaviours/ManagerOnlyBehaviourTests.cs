using FluentValidation;
using MediatR;
using Shouldly;
using TechXyz.GymXyz.Application.Behaviours;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXYZ.GymXYZ.Application.Tests.Members;

namespace TechXYZ.GymXYZ.Application.Tests.Behaviours;

/// <summary>
/// The behaviour that turns the marker into a refusal. Exercised on stand-in
/// requests rather than on a real command, so it says what the rule is without
/// dragging a database in: <see cref="Common.ManagerOnlyPerimeterTests"/> already
/// pins which commands carry the marker.
/// </summary>
public class ManagerOnlyBehaviourTests
{
    private sealed class ReservedRequest : IRequest<bool>, IManagerOnly;

    private sealed class OpenRequest : IRequest<bool>;

    [Fact]
    public async Task Handle_ShouldRefuseACoach_OnAReservedCommand()
    {
        var error = await Should.ThrowAsync<ValidationException>(() =>
            Run<ReservedRequest>(GymRoleNames.Coach));

        error.Errors.Single().ErrorMessage.ShouldBe(ManagerOnly.Reserved);
    }

    [Fact]
    public async Task Handle_ShouldRefuseAnAccountWithNoRoleAtAll()
    {
        await Should.ThrowAsync<ValidationException>(() => Run<ReservedRequest>());
    }

    [Fact]
    public async Task Handle_ShouldNotReachTheHandler_WhenItRefuses()
    {
        // The refusal has to land before anything is read on the caller's
        // behalf: an error that varies with what exists is an answer about what
        // exists.
        var reached = false;

        await Should.ThrowAsync<ValidationException>(() =>
            Run<ReservedRequest>(GymRoleNames.Coach, onHandler: () => reached = true));

        reached.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldLetAManagerThrough()
    {
        (await Run<ReservedRequest>(GymRoleNames.GymManager)).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldLetAPlatformAdminThrough()
    {
        // GymPolicies.GymManager admits one, and the impersonation trail exists
        // so that visit can act rather than only look.
        (await Run<ReservedRequest>(GymRoleNames.PlatformAdmin)).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldLeaveAnUnmarkedCommandAlone()
    {
        // Pointing a sheet is a coach's job; the behaviour must not be a blanket.
        (await Run<OpenRequest>(GymRoleNames.Coach)).ShouldBeTrue();
    }

    private static Task<bool> Run<TRequest>(string? role = null, Action? onHandler = null)
        where TRequest : IRequest<bool>, new()
    {
        var behaviour = new ManagerOnlyBehaviour<TRequest, bool>(
            new TestCurrentUserService(role is null ? [] : [role]));

        return behaviour.Handle(
            new TRequest(),
            _ =>
            {
                onHandler?.Invoke();
                return Task.FromResult(true);
            },
            CancellationToken.None);
    }
}
