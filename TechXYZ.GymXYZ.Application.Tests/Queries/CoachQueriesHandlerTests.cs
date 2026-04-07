using Shouldly;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class CoachQueriesHandlerTests
{
    [Fact]
    public async Task GetCoaches_ShouldReturnSortedList()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoaches_ShouldReturnSortedList));
        dbContext.Coaches.AddRange(
            new Coach(faker.Name.FirstName(), "Zulu"),
            new Coach(faker.Name.FirstName(), "Alpha"));
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new GetCoachesQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result[0].LastName.ShouldBe("Alpha");
        result[1].LastName.ShouldBe("Zulu");
    }

    [Fact]
    public async Task GetCoachById_ShouldReturnNull_WhenNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachById_ShouldReturnNull_WhenNotFound));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new GetCoachByIdQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetCoachByIdQuery(12345), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCoachById_ShouldReturnCoachDto_WhenFound()
    {
        var faker = TestInfrastructure.Faker();
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var email = faker.Internet.Email();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(GetCoachById_ShouldReturnCoachDto_WhenFound));

        var coach = new Coach(firstName, lastName)
        {
            Email = email,
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };

        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new GetCoachByIdQueryHandler(unitOfWork);

        var result = await handler.Handle(new GetCoachByIdQuery(coach.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(coach.Id);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
        result.Email.ShouldBe(email);
        result.Address.ShouldNotBeNull();
    }
}
