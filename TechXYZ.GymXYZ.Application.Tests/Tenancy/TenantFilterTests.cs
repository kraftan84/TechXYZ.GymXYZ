using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class TenantFilterTests
{
    [Fact]
    public async Task Query_ShouldHideEntities_OfAnotherTenant()
    {
        var databaseName = nameof(Query_ShouldHideEntities_OfAnotherTenant);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        await dbContext.SaveChangesAsync();

        using (tenantContext.UseTenant(2))
        {
            dbContext.Coaches.Add(new Coach("Aurélie", "Siquier"));
            await dbContext.SaveChangesAsync();
        }

        var firstTenantCoaches = await dbContext.Coaches.AsNoTracking().ToListAsync();

        firstTenantCoaches.Count.ShouldBe(1);
        firstTenantCoaches[0].LastName.ShouldBe("Lemoine");
    }

    [Fact]
    public async Task Query_ShouldFollowAmbientTenant_AndNotFreezeOnTheFirstOne()
    {
        // The model is built once and cached: if the filter captured a value
        // instead of reading the context, every later request would see the
        // first tenant's data.
        var databaseName = nameof(Query_ShouldFollowAmbientTenant_AndNotFreezeOnTheFirstOne);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        await dbContext.SaveChangesAsync();

        using (tenantContext.UseTenant(2))
        {
            dbContext.Coaches.Add(new Coach("Aurélie", "Siquier"));
            await dbContext.SaveChangesAsync();
        }

        await dbContext.Coaches.AsNoTracking().ToListAsync();

        tenantContext.Current = 2;
        var secondTenantCoaches = await dbContext.Coaches.AsNoTracking().ToListAsync();

        secondTenantCoaches.Count.ShouldBe(1);
        secondTenantCoaches[0].LastName.ShouldBe("Siquier");
    }

    [Fact]
    public async Task Query_ShouldReturnNothing_WhenNoTenantIsResolved()
    {
        var databaseName = nameof(Query_ShouldReturnNothing_WhenNoTenantIsResolved);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        await dbContext.SaveChangesAsync();

        // An unknown host must not fall back to somebody else's data.
        tenantContext.Current = 0;

        var coaches = await dbContext.Coaches.AsNoTracking().ToListAsync();

        coaches.ShouldBeEmpty();
    }

    [Fact]
    public async Task Query_ShouldHideInvitations_OfAnotherTenant()
    {
        // An invitation is an e-mail address somebody handed the gym. Leaking
        // one across customers leaks a person, not a row.
        var databaseName = nameof(Query_ShouldHideInvitations_OfAnotherTenant);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Invitations.Add(new Invitation
        {
            Email = "theo.garnier@gymxyz.fr",
            RoleName = "Coach",
            SentOn = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        using (tenantContext.UseTenant(2))
        {
            dbContext.Invitations.Add(new Invitation
            {
                Email = "quelquun@autre.fr",
                RoleName = "Coach",
                SentOn = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        var invitations = await dbContext.Invitations.AsNoTracking().ToListAsync();

        invitations.Select(invitation => invitation.Email).ShouldBe(["theo.garnier@gymxyz.fr"]);
    }

    [Fact]
    public async Task SaveChanges_ShouldStampTheAmbientTenant_OnNewEntities()
    {
        var databaseName = nameof(SaveChanges_ShouldStampTheAmbientTenant_OnNewEntities);
        var tenantContext = new TestTenantContext(7);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        var coach = new Coach("Samir", "El Amrani");
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        coach.TenantId.ShouldBe(7);
    }

    [Fact]
    public async Task SaveChanges_ShouldKeepAnExplicitTenant_WhenTheSeedSetsOne()
    {
        var databaseName = nameof(SaveChanges_ShouldKeepAnExplicitTenant_WhenTheSeedSetsOne);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        var coach = new Coach("Léa", "Fontaine") { TenantId = 42 };
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        coach.TenantId.ShouldBe(42);
    }

    [Fact]
    public async Task SoftDelete_ShouldStillBeExplicit_AlongsideTheTenantFilter()
    {
        // The tenant filter is global; IsActive deliberately is not, so an
        // inactive row stays reachable until a query filters it out.
        var databaseName = nameof(SoftDelete_ShouldStillBeExplicit_AlongsideTheTenantFilter);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Coaches.Add(new Coach("Nora", "Lemoine"));
        dbContext.Coaches.Add(new Coach("Théo", "Garnier") { IsActive = false });
        await dbContext.SaveChangesAsync();

        var all = await dbContext.Coaches.AsNoTracking().ToListAsync();
        var active = await dbContext.Coaches.AsNoTracking().Where(coach => coach.IsActive).ToListAsync();

        all.Count.ShouldBe(2);
        active.Count.ShouldBe(1);
        active[0].LastName.ShouldBe("Lemoine");
    }

    [Fact]
    public async Task Query_ShouldNotFilterInvoices_WhichSitAboveTheTenant()
    {
        // A GymXYZ invoice is what a customer owes TechXYZ, and the only screen
        // that reads it is the console, served as no customer at all. Filtering
        // it by tenant would hide every invoice from the one screen for it.
        var databaseName = nameof(Query_ShouldNotFilterInvoices_WhichSitAboveTheTenant);
        var tenantContext = new TestTenantContext(1);

        await using var dbContext = TestInfrastructure.CreateDbContext(databaseName, tenantContext);
        dbContext.Invoices.AddRange(
            new Invoice { TenantId = 1, Reference = "GX-2026-001", Amount = 948m },
            new Invoice { TenantId = 2, Reference = "GX-2026-002", Amount = 588m });
        await dbContext.SaveChangesAsync();

        tenantContext.Current = 0;

        var invoices = await dbContext.Invoices.AsNoTracking().ToListAsync();

        invoices.Count.ShouldBe(2);
    }

    // A third case stood here: TenantImpersonation, unfiltered so that a customer
    // could not erase the trail of a platform admin's visit into its data. Both
    // the visit and the trail were removed — there is no such visit to record.
    // The rule it demonstrated is still demonstrated, by Tenant and Invoice
    // above: a row about a customer must stay readable from outside it.
}
