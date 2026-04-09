using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class DeleteMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSoftDeleteMember_WhenItExists()
    {
        var faker = TestInfrastructure.Faker();

        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldSoftDeleteMember_WhenItExists));
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName());
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteMemberCommandHandler(dbContext, new DeleteMemberCommandValidator());

        var deleted = await handler.Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        deleted.ShouldBeTrue();
        dbContext.Members.Single(candidate => candidate.Id == member.Id).IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenMemberDoesNotExist()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnFalse_WhenMemberDoesNotExist));
        var handler = new DeleteMemberCommandHandler(dbContext, new DeleteMemberCommandValidator());

        var deleted = await handler.Handle(new DeleteMemberCommand(999), CancellationToken.None);

        deleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenIdIsInvalid()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldThrowValidationException_WhenIdIsInvalid));
        var handler = new DeleteMemberCommandHandler(dbContext, new DeleteMemberCommandValidator());

        await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(new DeleteMemberCommand(0), CancellationToken.None));
    }
}
