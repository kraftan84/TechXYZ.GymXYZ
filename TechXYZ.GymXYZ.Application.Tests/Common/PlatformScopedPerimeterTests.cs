using System.Reflection;
using MediatR;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Queries;

namespace TechXYZ.GymXYZ.Application.Tests.Common;

/// <summary>
/// Which requests run outside every customer, named one by one.
/// <para>
/// The whole product assumes the opposite: entities are <c>ITenantScoped</c>, a
/// global filter narrows them to the current customer, and anything reading
/// across that line is a partitioning bug. The space request is the first thing
/// that legitimately sits before the line — it is filed by a stranger, for a
/// customer that does not exist yet.
/// </para>
/// <para>
/// Which is why it is written down rather than left to be inferred from an
/// entity that happens not to implement an interface. A list makes the exception
/// countable, and a new one arriving without a reason will fail this test rather
/// than pass unnoticed.
/// </para>
/// </summary>
public class PlatformScopedPerimeterTests
{
    /// <summary>
    /// The public onboarding form and the field inside it. Both are reachable by
    /// anyone on the internet, and neither has a tenant to be scoped to.
    /// </summary>
    private static readonly string[] OutsideEveryTenant =
    [
        nameof(SubmitSpaceRequestCommand),
        nameof(CheckSubdomainAvailabilityQuery),

        // The three-month deletion. Runs from a background sweep with no user and
        // no customer at all — the strongest case for the marker there is.
        nameof(PurgeRefusedSpaceRequestsCommand),

        // The five that were already crossing and did not say so.
        //
        // They predate the marker by two lots. Both queries read every customer
        // at once — GetTenantsQuery counts through TenantMemberCounter, which
        // lifts the global filter in so many words — and the three commands write
        // the Tenant row itself, which is the thing the filter is drawn around
        // rather than something the filter covers. All five run in a scope that
        // resolves no tenant at all, which is the marker's other half.
        //
        // Left out, the list above was true only by omission, and a list of
        // border-crossers that omits five is not a net. Named here before the
        // console adds a dozen more.
        nameof(GetTenantsQuery),
        nameof(GetTenantDetailQuery),
        nameof(CreateTenantCommand),
        nameof(UpdateTenantBrandingCommand),
        nameof(UpdateTenantPlanCommand)
    ];

    [Fact]
    public void OnlyTheNamedRequests_ShouldEscapeTheTenantPerimeter()
    {
        var marked = AllRequests()
            .Where(type => type.IsAssignableTo(typeof(IPlatformScoped)))
            .Select(type => type.Name)
            .ToList();

        marked.ShouldBe(OutsideEveryTenant, ignoreOrder: true,
            "A request that runs outside every customer is unusual enough to be named here.");
    }

    [Fact]
    public void APlatformScopedRequest_ShouldNotAlsoDemandAManager()
    {
        // The two markers answer different questions and would contradict each
        // other here. IManagerOnly asks whether the caller manages the gym being
        // served; nobody here is inside a gym. On the public form the caller is
        // not signed in at all, so the marker would refuse every applicant and
        // the failure would read as a broken form. On the platform's own
        // requests the caller is a PlatformAdmin, who manages no gym and — since
        // the impersonation was removed — is deliberately refused by
        // ManagerOnly. Either way, carrying both would close the request to the
        // only person entitled to run it.
        foreach (var name in OutsideEveryTenant)
        {
            AllRequests()
                .Single(type => type.Name == name)
                .IsAssignableTo(typeof(IManagerOnly))
                .ShouldBeFalse($"{name} runs outside every gym, so no gym's manager guards it.");
        }
    }

    private static IEnumerable<Type> AllRequests() =>
        typeof(SubmitSpaceRequestCommand).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.GetInterfaces().Any(IsMediatRRequest));

    private static bool IsMediatRRequest(Type contract) =>
        contract == typeof(IRequest)
        || (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>));
}
