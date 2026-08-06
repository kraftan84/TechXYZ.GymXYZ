using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The three writes of the TechXYZ console: opening a customer, dressing it and
/// billing it. All of them reach a <c>Tenant</c>, which sits above the tenant
/// filter — so these run served as nobody, the way a platform admin is.
/// </summary>
public class TenantConsoleCommandTests
{
    private const int GymXyzId = 1;

    [Fact]
    public async Task Create_ShouldOpenACustomerShowingItsNameAlone()
    {
        await using var dbContext = Context(nameof(Create_ShouldOpenACustomerShowingItsNameAlone));

        var id = await Create(dbContext, new CreateTenantCommand(
            "  Team Trainer's  ", "TeamTrainers", "teamtrainers", IsSolo: false));

        var tenant = await dbContext.Tenants.SingleAsync(candidate => candidate.Id == id);

        tenant.Name.ShouldBe("Team Trainer's");
        tenant.DisplayName.ShouldBe("Team Trainer's");
        // Lowercased: the slug becomes a host prefix, and hosts are not case
        // sensitive — storing "TeamTrainers" would make the lookup miss.
        tenant.Slug.ShouldBe("teamtrainers");
        tenant.WordmarkText.ShouldBe("Team Trainer's");

        // No mark, and no default one to fall back on: GymXYZ's own kettlebell
        // would be a brand leak in a white-label product.
        tenant.LogoPath.ShouldBeNull();
        tenant.WordmarkPrefix.ShouldBeNull();
    }

    [Fact]
    public async Task Create_ShouldRefuseASlugAnotherCustomerAlreadyHas()
    {
        await using var dbContext = Context(nameof(Create_ShouldRefuseASlugAnotherCustomerAlreadyHas));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId });
        await dbContext.SaveChangesAsync();

        var refusal = await Should.ThrowAsync<ValidationException>(
            () => Create(dbContext, new CreateTenantCommand("Autre", "GymXYZ", "techxyz", false)));

        refusal.Errors.ShouldContain(error => error.ErrorMessage == TenantRules.SlugTaken);
    }

    [Theory]
    // A host label takes lowercase, digits and inner hyphens — nothing else.
    [InlineData("team trainers")]
    [InlineData("team_trainers")]
    [InlineData("-teamtrainers")]
    [InlineData("teamtrainers-")]
    [InlineData("team.trainers")]
    public async Task Create_ShouldRefuseASlugThatCannotBeAHostName(string slug)
    {
        await using var dbContext = Context($"{nameof(Create_ShouldRefuseASlugThatCannotBeAHostName)}{slug}");

        var refusal = await Should.ThrowAsync<ValidationException>(
            () => Create(dbContext, new CreateTenantCommand("Client", slug, "techxyz", false)));

        refusal.Errors.ShouldContain(error => error.ErrorMessage == TenantRules.SlugInvalid);
    }

    [Fact]
    public async Task UpdateBranding_ShouldRepaintTheCustomerByChangingItsTheme()
    {
        await using var dbContext = Context(nameof(UpdateBranding_ShouldRepaintTheCustomerByChangingItsTheme));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId });
        await dbContext.SaveChangesAsync();

        var saved = await UpdateBranding(dbContext, new UpdateTenantBrandingCommand(
            GymXyzId, "leyssa", "GymXYZ Lyon", "Révélez-vous", "GymXYZ Lyon", null, null));

        saved.ShouldBeTrue();

        var tenant = await dbContext.Tenants.SingleAsync();
        tenant.ThemeKey.ShouldBe("leyssa");
        tenant.DisplayName.ShouldBe("GymXYZ Lyon");
        tenant.Baseline.ShouldBe("Révélez-vous");
    }

    [Fact]
    public async Task UpdateBranding_ShouldClearTheSplitWordmarkWhenAWholeOneIsGiven()
    {
        await using var dbContext = Context(
            nameof(UpdateBranding_ShouldClearTheSplitWordmarkWhenAWholeOneIsGiven));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            Id = GymXyzId,
            WordmarkPrefix = "GYM",
            WordmarkAccent = "XYZ"
        });
        await dbContext.SaveChangesAsync();

        await UpdateBranding(dbContext, new UpdateTenantBrandingCommand(
            GymXyzId, "techxyz", "GymXYZ", null, "GYMXYZ", null, null));

        var tenant = await dbContext.Tenants.SingleAsync();

        // Leaving the halves behind would let the lockup render yesterday's name:
        // the component prefers WordmarkText only when it is set, so both shapes
        // present would be one shape too many.
        tenant.WordmarkText.ShouldBe("GYMXYZ");
        tenant.WordmarkPrefix.ShouldBeNull();
        tenant.WordmarkAccent.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateBranding_ShouldClearTheWholeWordmarkWhenASplitOneIsGiven()
    {
        await using var dbContext = Context(
            nameof(UpdateBranding_ShouldClearTheWholeWordmarkWhenASplitOneIsGiven));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            Id = GymXyzId,
            WordmarkText = "GYMXYZ"
        });
        await dbContext.SaveChangesAsync();

        await UpdateBranding(dbContext, new UpdateTenantBrandingCommand(
            GymXyzId, "techxyz", "GymXYZ", null, "GYMXYZ", "GYM", "XYZ"));

        var tenant = await dbContext.Tenants.SingleAsync();

        tenant.WordmarkPrefix.ShouldBe("GYM");
        tenant.WordmarkAccent.ShouldBe("XYZ");
        tenant.WordmarkText.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateBranding_ShouldSayNothingHappenedForACustomerThatIsNotThere()
    {
        await using var dbContext = Context(
            nameof(UpdateBranding_ShouldSayNothingHappenedForACustomerThatIsNotThere));

        var saved = await UpdateBranding(dbContext, new UpdateTenantBrandingCommand(
            404, "techxyz", "Fantôme", null, "Fantôme", null, null));

        saved.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdatePlan_ShouldKeepAnEmptyCapAsUnlimited()
    {
        await using var dbContext = Context(nameof(UpdatePlan_ShouldKeepAnEmptyCapAsUnlimited));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            Id = GymXyzId,
            PlanMemberCap = 150
        });
        await dbContext.SaveChangesAsync();

        var renewal = new DateOnly(2027, 1, 1);

        var saved = await UpdatePlan(dbContext, new UpdateTenantPlanCommand(
            GymXyzId, "GymXYZ Pro", "Engagement annuel", 79m, renewal, PlanMemberCap: null));

        saved.ShouldBeTrue();

        var tenant = await dbContext.Tenants.SingleAsync();
        tenant.GymPlan.ShouldBe("GymXYZ Pro");
        tenant.PlanPrice.ShouldBe(79m);
        tenant.PlanRenewalDate.ShouldBe(renewal);

        // Null is a real answer — « 112 / illimité » — not a missing one, so it
        // has to overwrite the cap that was there.
        tenant.PlanMemberCap.ShouldBeNull();
    }

    [Fact]
    public async Task UpdatePlan_ShouldRefuseAPlanCoveringNobody()
    {
        await using var dbContext = Context(nameof(UpdatePlan_ShouldRefuseAPlanCoveringNobody));

        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz") { Id = GymXyzId });
        await dbContext.SaveChangesAsync();

        var refusal = await Should.ThrowAsync<ValidationException>(
            () => UpdatePlan(dbContext, new UpdateTenantPlanCommand(
                GymXyzId, "GymXYZ Pro", null, 79m, null, PlanMemberCap: 0)));

        refusal.Errors.ShouldContain(error => error.ErrorMessage == TenantRules.MemberCapOutOfRange);
    }

    private static GymDbContext Context(string databaseName) =>
        // Served as nobody: a platform admin has no tenant of its own, and these
        // commands have to work in exactly that state.
        TestInfrastructure.CreateDbContext(databaseName, new TestTenantContext(0));

    private static Task<int> Create(GymDbContext dbContext, CreateTenantCommand command) =>
        new CreateTenantCommandHandler(dbContext, new CreateTenantCommandValidator())
            .Handle(command, CancellationToken.None);

    private static Task<bool> UpdateBranding(GymDbContext dbContext, UpdateTenantBrandingCommand command) =>
        new UpdateTenantBrandingCommandHandler(dbContext, new UpdateTenantBrandingCommandValidator())
            .Handle(command, CancellationToken.None);

    private static Task<bool> UpdatePlan(GymDbContext dbContext, UpdateTenantPlanCommand command) =>
        new UpdateTenantPlanCommandHandler(dbContext, new UpdateTenantPlanCommandValidator())
            .Handle(command, CancellationToken.None);
}
