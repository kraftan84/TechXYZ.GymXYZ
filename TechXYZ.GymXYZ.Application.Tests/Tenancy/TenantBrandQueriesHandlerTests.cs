using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class TenantBrandQueriesHandlerTests
{
    [Fact]
    public async Task GetTenantBrand_ShouldReturnBrand_WhenSlugMatches()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetTenantBrand_ShouldReturnBrand_WhenSlugMatches));
        dbContext.Tenants.Add(new Tenant("GymXYZ", "gymxyz", "techxyz")
        {
            WordmarkPrefix = "GYM",
            WordmarkAccent = "XYZ"
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetTenantBrandQueryHandler(dbContext);

        var result = await handler.Handle(new GetTenantBrandQuery("gymxyz"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.ThemeKey.ShouldBe("techxyz");
        result.WordmarkPrefix.ShouldBe("GYM");
        result.WordmarkAccent.ShouldBe("XYZ");
        result.IsSolo.ShouldBeFalse();
    }

    /// <summary>
    /// The planning banner reads this off the brand, so it has to survive the
    /// projection — and it has to arrive with the postcode, because one without
    /// the other decides nothing.
    /// </summary>
    [Fact]
    public async Task GetTenantBrand_ShouldCarryTheSchoolHolidaysChoice()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetTenantBrand_ShouldCarryTheSchoolHolidaysChoice));
        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers")
        {
            ZipCode = "74200",
            ShowSchoolVacations = false
        });
        await dbContext.SaveChangesAsync();

        var result = await new GetTenantBrandQueryHandler(dbContext)
            .Handle(new GetTenantBrandQuery("teamtrainers"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.ZipCode.ShouldBe("74200");
        result.ShowSchoolVacations.ShouldBeFalse();
        result.MarksSchoolVacations.ShouldBeFalse();
    }

    /// <summary>
    /// A customer with no address has no département, and the zone table answers
    /// A for want of anything better. Marking that zone's holidays would be
    /// showing a calendar nobody chose, so the setting cannot switch on without
    /// a postcode however it is stored.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetTenantBrand_ShouldNotMarkHolidays_WithoutAPostcode(string? zipCode)
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(GetTenantBrand_ShouldNotMarkHolidays_WithoutAPostcode) + (zipCode?.Length ?? -1));
        dbContext.Tenants.Add(new Tenant("Leyssa Coaching", "leyssa", "leyssa")
        {
            AreaLabel = "Thonon et alentours",
            ZipCode = zipCode,
            ShowSchoolVacations = true
        });
        await dbContext.SaveChangesAsync();

        var result = await new GetTenantBrandQueryHandler(dbContext)
            .Handle(new GetTenantBrandQuery("leyssa"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShowSchoolVacations.ShouldBeTrue();
        result.MarksSchoolVacations.ShouldBeFalse();
    }

    [Fact]
    public async Task GetTenantBrand_ShouldReturnNull_WhenSlugIsUnknown()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetTenantBrand_ShouldReturnNull_WhenSlugIsUnknown));
        var handler = new GetTenantBrandQueryHandler(dbContext);

        var result = await handler.Handle(new GetTenantBrandQuery("inconnu"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetTenantBrand_ShouldReturnNull_WhenTenantIsInactive()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetTenantBrand_ShouldReturnNull_WhenTenantIsInactive));
        dbContext.Tenants.Add(new Tenant("Ancien client", "ancien", "techxyz") { IsActive = false });
        await dbContext.SaveChangesAsync();

        var handler = new GetTenantBrandQueryHandler(dbContext);

        var result = await handler.Handle(new GetTenantBrandQuery("ancien"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetTenantBrandById_ShouldReturnSoloFlag_ForAnIndependentCoach()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetTenantBrandById_ShouldReturnSoloFlag_ForAnIndependentCoach));
        var tenant = new Tenant("Leyssa Coaching", "leyssa", "leyssa")
        {
            IsSolo = true,
            WordmarkText = "Leyssa Coaching",
            AreaLabel = "Thonon et alentours"
        };
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var handler = new GetTenantBrandByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetTenantBrandByIdQuery(tenant.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.IsSolo.ShouldBeTrue();
        result.WordmarkText.ShouldBe("Leyssa Coaching");
    }
}
