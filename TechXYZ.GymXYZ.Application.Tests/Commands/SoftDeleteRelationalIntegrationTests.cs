using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class SoftDeleteRelationalIntegrationTests
{
    [Fact]
    public async Task DeleteMember_ShouldSoftDelete_AndExcludeFromQueries_OnSqlite()
    {
        var faker = TestInfrastructure.Faker();
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Subscriptions =
            [
                new Subscription
                {
                    Plan = TestPlans.Monthly(),
                    StartedOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                    EndsOn = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                    PriceLabel = "49 € / mois"
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var deleteHandler = new DeleteMemberCommandHandler(dbContext, new DeleteMemberCommandValidator());
        var deleted = await deleteHandler.Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Members.Single(candidate => candidate.Id == member.Id).IsActive.ShouldBeFalse();

        var queryHandler = new GetMembersQueryHandler(dbContext, TestCurrentUserService.Manager());
        var members = await queryHandler.Handle(new GetMembersQuery(), CancellationToken.None);
        members.Items.ShouldBeEmpty();
        members.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task DeleteCoach_ShouldSoftDeleteWithoutBreakingRequiredSessionRelation_OnSqlite()
    {
        var faker = TestInfrastructure.Faker();
        await using var scope = await RelationalTestInfrastructure.CreateSqliteDbContextScope();
        var dbContext = scope.DbContext;

        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        var site = new Site(faker.Address.City())
        {
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };
        var location = new Location(faker.Commerce.ProductName());
        site.AddLocation(location);

        dbContext.Coaches.Add(coach);
        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync();

        dbContext.Sessions.Add(new Session
        {
            CourseTemplate = new CourseTemplate(faker.Company.CatchPhrase())
            {
                Discipline = new Discipline(faker.Commerce.Department())
            },
            Coach = coach,
            Location = location,
            Capacity = 1,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddHours(1)
        });
        await dbContext.SaveChangesAsync();

        var deleteHandler = new DeleteCoachCommandHandler(dbContext, new DeleteCoachCommandValidator());
        var deleted = await deleteHandler.Handle(new DeleteCoachCommand(coach.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Coaches.Single(candidate => candidate.Id == coach.Id).IsActive.ShouldBeFalse();
        dbContext.Sessions.Count().ShouldBe(1);
    }
}
