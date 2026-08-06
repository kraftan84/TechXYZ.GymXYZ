using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The trail a platform admin leaves when entering a customer. On a product that
/// hosts several gyms' data, knowing who went in where — and when they came out
/// — is not optional, so these pin the shape of the record rather than only the
/// happy path.
/// </summary>
public class TenantImpersonationCommandTests
{
    private const string Admin = "admin-user-id";
    private const string AdminEmail = "admin@techxyz.fr";
    private const int GymXyzId = 1;
    private const int TeamTrainersId = 2;

    [Fact]
    public async Task Begin_ShouldOpenAVisitAndHandBackTheCustomerToSignIn()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Begin_ShouldOpenAVisitAndHandBackTheCustomerToSignIn),
            // Served as nobody: a platform admin has no tenant of its own, which
            // is exactly the state this command runs in.
            new TestTenantContext(0));

        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            Id = TeamTrainersId
        });
        await dbContext.SaveChangesAsync();

        var visit = await Begin(dbContext, TeamTrainersId);

        visit.ShouldNotBeNull();
        visit.Slug.ShouldBe("teamtrainers");
        visit.DisplayName.ShouldBe("Team Trainer's");

        var trail = dbContext.TenantImpersonations.Single();
        trail.Id.ShouldBe(visit.VisitId);
        trail.AdminUserId.ShouldBe(Admin);
        trail.AdminEmail.ShouldBe(AdminEmail);
        trail.TenantId.ShouldBe(TeamTrainersId);
        trail.EndedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Begin_ShouldRefuseACustomerThatDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Begin_ShouldRefuseACustomerThatDoesNotExist),
            new TestTenantContext(0));

        var visit = await Begin(dbContext, 404);

        // Nothing to re-sign the cookie with, so the admin stays outside — and
        // no trail row is written for a visit that never happened.
        visit.ShouldBeNull();
        dbContext.TenantImpersonations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Begin_ShouldRefuseARetiredCustomer()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Begin_ShouldRefuseARetiredCustomer),
            new TestTenantContext(0));

        dbContext.Tenants.Add(new Tenant("Partie", "partie", "techxyz")
        {
            Id = TeamTrainersId,
            IsActive = false
        });
        await dbContext.SaveChangesAsync();

        (await Begin(dbContext, TeamTrainersId)).ShouldBeNull();
    }

    [Fact]
    public async Task Begin_ShouldCloseAVisitTheAdminNeverLeft()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Begin_ShouldCloseAVisitTheAdminNeverLeft),
            new TestTenantContext(0));

        dbContext.Tenants.AddRange(
            new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId },
            new Tenant("Team Trainer's", "teamtrainers", "teamtrainers") { Id = TeamTrainersId });
        await dbContext.SaveChangesAsync();

        // Jumping straight from one customer to another never passes through the
        // exit, so the first visit would otherwise stay open for ever.
        var first = await Begin(dbContext, GymXyzId);
        var second = await Begin(dbContext, TeamTrainersId);

        first.ShouldNotBeNull();
        second.ShouldNotBeNull();

        var trail = dbContext.TenantImpersonations.OrderBy(visit => visit.Id).ToList();
        trail.Count.ShouldBe(2);
        trail[0].EndedAt.ShouldNotBeNull();
        trail[1].EndedAt.ShouldBeNull();
    }

    [Fact]
    public async Task End_ShouldCloseTheVisitItWasGiven()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(End_ShouldCloseTheVisitItWasGiven),
            new TestTenantContext(0));

        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            Id = TeamTrainersId
        });
        await dbContext.SaveChangesAsync();

        var visit = await Begin(dbContext, TeamTrainersId);
        visit.ShouldNotBeNull();

        var closed = await new EndTenantImpersonationCommandHandler(dbContext)
            .Handle(new EndTenantImpersonationCommand(Admin, visit.VisitId), CancellationToken.None);

        closed.ShouldBeTrue();
        dbContext.TenantImpersonations.Single().EndedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task End_ShouldRefuseToCloseSomebodyElsesVisit()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(End_ShouldRefuseToCloseSomebodyElsesVisit),
            new TestTenantContext(0));

        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            Id = TeamTrainersId
        });
        await dbContext.SaveChangesAsync();

        var visit = await Begin(dbContext, TeamTrainersId);
        visit.ShouldNotBeNull();

        var closed = await new EndTenantImpersonationCommandHandler(dbContext)
            .Handle(
                new EndTenantImpersonationCommand("another-admin", visit.VisitId),
                CancellationToken.None);

        // Posting a number must not let one admin rewrite another's trail.
        closed.ShouldBeFalse();
        dbContext.TenantImpersonations.Single().EndedAt.ShouldBeNull();
    }

    [Fact]
    public async Task End_ShouldSayNothingHappenedWhenThereIsNoOpenVisit()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(End_ShouldSayNothingHappenedWhenThereIsNoOpenVisit),
            new TestTenantContext(0));

        var closed = await new EndTenantImpersonationCommandHandler(dbContext)
            .Handle(new EndTenantImpersonationCommand(Admin, 999), CancellationToken.None);

        // False, not an exception: the caller signs the admin out of the customer
        // either way rather than stranding them inside over an odd trail.
        closed.ShouldBeFalse();
    }

    private static Task<TenantImpersonationDto?> Begin(GymDbContext dbContext, int tenantId)
    {
        return new BeginTenantImpersonationCommandHandler(dbContext)
            .Handle(
                new BeginTenantImpersonationCommand(Admin, AdminEmail, tenantId),
                CancellationToken.None);
    }
}
