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
