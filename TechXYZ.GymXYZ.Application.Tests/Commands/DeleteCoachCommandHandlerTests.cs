using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteCoachCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteCoach_WhenItExists()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldDeleteCoach_WhenItExists));
        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        dbContext.Coaches.Add(coach);
        await dbContext.SaveChangesAsync();

        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);
        var handler = new DeleteCoachCommandHandler(unitOfWork, new DeleteCoachCommandValidator());

        var deleted = await handler.Handle(new DeleteCoachCommand(coach.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Coaches.Any(candidate => candidate.Id == coach.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCoachDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenCoachDoesNotExist));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new DeleteCoachCommandHandler(unitOfWork, new DeleteCoachCommandValidator());

        var deleted = await handler.Handle(new DeleteCoachCommand(999), CancellationToken.None);

        deleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        using var unitOfWork = TestInfrastructure.CreateUnitOfWork(dbContext);

        var handler = new DeleteCoachCommandHandler(unitOfWork, new DeleteCoachCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => handler.Handle(new DeleteCoachCommand(0), CancellationToken.None));
    }
}
