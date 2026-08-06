using Shouldly;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The TechXYZ console reads across customers, which every other query in the
/// product is forbidden to do. These tests pin the two halves of that: the list
/// itself, and the one place allowed to count inside a customer it is not
/// serving.
/// </summary>
public class TenantConsoleQueryTests
{
    private const int GymXyzId = 1;
    private const int TeamTrainersId = 2;
    private const int LeyssaId = 3;

    [Fact]
    public async Task CountActiveByTenant_ShouldNotMixOneCustomersMembersIntoAnothers()
    {
        // Served as GymXYZ throughout: the counter has to answer for the other
        // two anyway, and must not hand any of them each other's total.
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(CountActiveByTenant_ShouldNotMixOneCustomersMembersIntoAnothers),
            new TestTenantContext(GymXyzId));

        AddMembers(dbContext, GymXyzId, 4);
        AddMembers(dbContext, TeamTrainersId, 2);
        AddMembers(dbContext, LeyssaId, 1);
        await dbContext.SaveChangesAsync();

        var counts = await dbContext.CountActiveByTenantAsync(CancellationToken.None);

        counts.CountFor(GymXyzId).ShouldBe(4);
        counts.CountFor(TeamTrainersId).ShouldBe(2);
        counts.CountFor(LeyssaId).ShouldBe(1);
    }

    [Fact]
    public async Task CountActiveByTenant_ShouldLeaveOutRetiredMembers()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(CountActiveByTenant_ShouldLeaveOutRetiredMembers),
            new TestTenantContext(GymXyzId));

        AddMembers(dbContext, GymXyzId, 3);
        dbContext.Members.Add(new Member("Retirée", "Ancienne")
        {
            TenantId = GymXyzId,
            IsActive = false
        });
        await dbContext.SaveChangesAsync();

        var counts = await dbContext.CountActiveByTenantAsync(CancellationToken.None);

        counts.CountFor(GymXyzId).ShouldBe(3);
    }

    [Fact]
    public async Task CountActiveByTenant_ShouldAnswerZeroForACustomerWithNobody()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(CountActiveByTenant_ShouldAnswerZeroForACustomerWithNobody),
            new TestTenantContext(GymXyzId));

        AddMembers(dbContext, GymXyzId, 2);
        await dbContext.SaveChangesAsync();

        var counts = await dbContext.CountActiveByTenantAsync(CancellationToken.None);

        // Absent from the grouping rather than present at zero — the console
        // still has to draw a row for a customer who has signed nobody up.
        counts.CountFor(LeyssaId).ShouldBe(0);
    }

    [Fact]
    public async Task GetTenants_ShouldListEveryCustomerWhileServingOnlyOne()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetTenants_ShouldListEveryCustomerWhileServingOnlyOne),
            new TestTenantContext(GymXyzId));

        dbContext.Tenants.AddRange(
            new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId, City = "Lyon 3ᵉ" },
            new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
            {
                Id = TeamTrainersId,
                City = "Lyon 7ᵉ",
                GymPlan = "GymXYZ Studio",
                PlanMemberCap = 150
            },
            new Tenant("Leyssa Coaching", "leyssa", "leyssa")
            {
                Id = LeyssaId,
                AreaLabel = "Thonon et alentours",
                IsSolo = true
            });

        AddMembers(dbContext, GymXyzId, 4);
        AddMembers(dbContext, TeamTrainersId, 30);
        await dbContext.SaveChangesAsync();

        var customers = await new GetTenantsQueryHandler(dbContext)
            .Handle(new GetTenantsQuery(), CancellationToken.None);

        customers.Select(customer => customer.DisplayName)
            .ShouldBe(["GymXYZ", "Leyssa Coaching", "Team Trainer's"]);

        customers.Single(customer => customer.Id == TeamTrainersId).MemberCount.ShouldBe(30);
        customers.Single(customer => customer.Id == LeyssaId).MemberCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetTenants_ShouldLeaveOutARetiredCustomer()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetTenants_ShouldLeaveOutARetiredCustomer),
            new TestTenantContext(GymXyzId));

        dbContext.Tenants.AddRange(
            new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId },
            new Tenant("Partie", "partie", "techxyz") { Id = TeamTrainersId, IsActive = false });
        await dbContext.SaveChangesAsync();

        var customers = await new GetTenantsQueryHandler(dbContext)
            .Handle(new GetTenantsQuery(), CancellationToken.None);

        customers.Select(customer => customer.Slug).ShouldBe(["gymxyz"]);
    }

    [Theory]
    // An itinerant coach has an area and no town: the area is what shows.
    [InlineData(null, "Thonon et alentours", "Thonon et alentours")]
    [InlineData("Lyon 7ᵉ", null, "Lyon 7ᵉ")]
    // Both set is not a state the product creates, but the area wins if it ever is.
    [InlineData("Lyon 7ᵉ", "Thonon et alentours", "Thonon et alentours")]
    public void Where_ShouldPreferTheAreaOverTheTown(string? city, string? area, string expected)
    {
        var customer = Summary() with { City = city, AreaLabel = area };

        customer.Where.ShouldBe(expected);
    }

    [Fact]
    public void UsagePercent_ShouldBeNullOnAnUncappedPlan()
    {
        // "112 / illimité": there is no gauge to fill, so the panel draws none.
        var customer = Summary() with { MemberCount = 112, PlanMemberCap = null };

        customer.UsagePercent.ShouldBeNull();
    }

    [Fact]
    public void UsagePercent_ShouldNotRunPastFullOnAnOverSubscribedPlan()
    {
        var customer = Summary() with { MemberCount = 180, PlanMemberCap = 150 };

        customer.UsagePercent.ShouldBe(100);
    }

    private static TenantSummaryDto Summary() => new(
        1, "gymxyz", "GymXYZ", "techxyz", null, null, false,
        null, null, false, null, null, null, null, 0);

    private static void AddMembers(GymDbContext dbContext, int tenantId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            dbContext.Members.Add(new Member($"Membre{index}", $"Client{tenantId}")
            {
                TenantId = tenantId
            });
        }
    }
}
