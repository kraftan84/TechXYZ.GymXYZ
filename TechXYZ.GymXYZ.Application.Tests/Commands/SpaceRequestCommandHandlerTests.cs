using FluentValidation;
using Shouldly;
using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

/// <summary>
/// The public form: what it files, what it refuses, and what it deliberately
/// does not say.
/// </summary>
public class SpaceRequestCommandHandlerTests
{
    [Fact]
    public async Task Submit_ShouldFileTheRequestAndAcknowledgeIt()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldFileTheRequestAndAcknowledgeIt));

        var emails = new TestEmailSender();

        var receipt = await Handler(dbContext, emails).Handle(Valid(), default);

        receipt.Reference.ShouldStartWith($"DEM-{DateTime.UtcNow.Year}-");
        receipt.RequestedSubdomain.ShouldBe("atlas-training");

        var filed = dbContext.SpaceRequests.Single();
        filed.Status.ShouldBe(SpaceRequestStatus.ToProcess);
        filed.Source.ShouldBe("Formulaire en ligne");

        // The consents are stored, not merely required: the second one is the
        // promise the purge keeps, and a promise nobody recorded cannot be shown.
        filed.AcceptedTerms.ShouldBeTrue();
        filed.AcceptedDataProcessing.ShouldBeTrue();

        emails.Single.ToAddress.ShouldBe("camille@atlas-training.fr");
        emails.Single.TextBody.ShouldContain(receipt.Reference);
    }

    [Fact]
    public async Task Submit_ShouldNumberRequestsInSequenceWithinTheYear()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldNumberRequestsInSequenceWithinTheYear));

        var handler = Handler(dbContext, new TestEmailSender());

        var first = await handler.Handle(Valid(), default);
        var second = await handler.Handle(Valid(subdomain: "vertika"), default);

        var year = DateTime.UtcNow.Year;
        first.Reference.ShouldBe($"DEM-{year}-0001");
        second.Reference.ShouldBe($"DEM-{year}-0002");
    }

    [Fact]
    public async Task Submit_ShouldKeepOnlyTheLocationTheProfileAsksFor()
    {
        // Somebody who filled an address, then went back to step 1 and said
        // "coach", must not have that address travel to the console anyway.
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldKeepOnlyTheLocationTheProfileAsksFor));

        await Handler(dbContext, new TestEmailSender()).Handle(
            Valid(type: SpaceRequestType.Coach, withLocation: true),
            default);

        var filed = dbContext.SpaceRequests.Single();
        filed.Street.ShouldBeNull();
        filed.ZipCode.ShouldBeNull();
        filed.City.ShouldBeNull();
        filed.AreaLabel.ShouldBe("Thonon et 30 km alentour");
    }

    [Fact]
    public async Task Submit_ShouldRefuseAnAddressACustomerAlreadyAnswersOn()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldRefuseAnAddressACustomerAlreadyAnswersOn));

        dbContext.Tenants.Add(new Tenant("Team Trainer's", "teamtrainers", "teamtrainers"));
        await dbContext.SaveChangesAsync();

        var send = () => Handler(dbContext, new TestEmailSender())
            .Handle(Valid(subdomain: "teamtrainers"), default);

        var failure = await send.ShouldThrowAsync<ValidationException>();
        failure.Errors.Single().ErrorMessage.ShouldBe(SpaceRequestRules.SubdomainTaken);
    }

    [Fact]
    public async Task Submit_ShouldRefuseAnAddressAnotherRequestIsAlreadyWaitingOn()
    {
        // Two requests for the same address is a collision the console would
        // otherwise discover at provisioning, in front of a customer.
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldRefuseAnAddressAnotherRequestIsAlreadyWaitingOn));

        var handler = Handler(dbContext, new TestEmailSender());
        await handler.Handle(Valid(), default);

        var send = () => handler.Handle(Valid(), default);

        await send.ShouldThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Submit_ShouldRefuseAReservedAddress()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldRefuseAReservedAddress));

        var send = () => Handler(dbContext, new TestEmailSender())
            .Handle(Valid(subdomain: "admin"), default);

        var failure = await send.ShouldThrowAsync<ValidationException>();
        failure.Errors.Single().ErrorMessage.ShouldBe(SpaceRequestRules.SubdomainReserved);
    }

    [Fact]
    public async Task Submit_ShouldRefuseWithoutBothMandatoryConsents()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldRefuseWithoutBothMandatoryConsents));

        var send = () => Handler(dbContext, new TestEmailSender())
            .Handle(Valid(acceptedDataProcessing: false), default);

        var failure = await send.ShouldThrowAsync<ValidationException>();
        failure.Errors.ShouldContain(error => error.ErrorMessage == SpaceRequestRules.ConsentsRequired);
    }

    [Fact]
    public async Task Submit_ShouldSwallowABotWithoutSayingSo()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldSwallowABotWithoutSayingSo));

        var emails = new TestEmailSender();

        var receipt = await Handler(dbContext, emails)
            .Handle(Valid(website: "http://spam.example"), default);

        // Looks exactly like success from the outside — that is the point. Telling
        // a bot it was caught tells whoever wrote it which field to leave alone.
        receipt.Reference.ShouldStartWith("DEM-");

        // And nothing at all happened.
        dbContext.SpaceRequests.ShouldBeEmpty();
        emails.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Submit_ShouldStandEvenWhenTheAcknowledgementCannotBeSent()
    {
        // The applicant filled six steps. Losing that because a mail server was
        // down would be the worse failure, so the row stands and the timeline says
        // what actually happened.
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Submit_ShouldStandEvenWhenTheAcknowledgementCannotBeSent));

        var emails = new TestEmailSender(fails: true);

        var receipt = await Handler(dbContext, emails).Handle(Valid(), default);

        receipt.Reference.ShouldNotBeNullOrWhiteSpace();
        dbContext.SpaceRequests.ShouldHaveSingleItem();

        dbContext.SpaceRequestActivities
            .Select(activity => activity.Title)
            .ShouldContain("Accusé de réception non envoyé");
    }

    // ---- The purge ----------------------------------------------------------

    [Fact]
    public async Task Purge_ShouldDeleteARefusalOlderThanThreeMonths()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Purge_ShouldDeleteARefusalOlderThanThreeMonths));

        dbContext.SpaceRequests.Add(Refused("DEM-2026-0001", DateTime.UtcNow.AddDays(-91)));
        dbContext.SpaceRequests.Add(Refused("DEM-2026-0002", DateTime.UtcNow.AddDays(-30)));
        await dbContext.SaveChangesAsync();

        var deleted = await new PurgeRefusedSpaceRequestsCommandHandler(dbContext)
            .Handle(new PurgeRefusedSpaceRequestsCommand(), default);

        deleted.ShouldBe(1);
        dbContext.SpaceRequests.Single().Reference.ShouldBe("DEM-2026-0002");
    }

    [Fact]
    public async Task Purge_ShouldLeaveEverythingThatWasNotRefused()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Purge_ShouldLeaveEverythingThatWasNotRefused));

        // Old, and not refused. The promise was about refusals; deleting a
        // customer's own approved dossier would be a different act entirely.
        dbContext.SpaceRequests.Add(new SpaceRequest("DEM-2025-0001", SpaceRequestType.Gym, "Atlas")
        {
            Status = SpaceRequestStatus.Approved,
            ReceivedOn = DateTime.UtcNow.AddYears(-1)
        });

        // Refused, but with no date to count from: a bug to find, not a row to
        // delete on a guess.
        dbContext.SpaceRequests.Add(new SpaceRequest("DEM-2025-0002", SpaceRequestType.Gym, "Vertika")
        {
            Status = SpaceRequestStatus.Refused,
            RefusedOn = null,
            ReceivedOn = DateTime.UtcNow.AddYears(-1)
        });

        await dbContext.SaveChangesAsync();

        var deleted = await new PurgeRefusedSpaceRequestsCommandHandler(dbContext)
            .Handle(new PurgeRefusedSpaceRequestsCommand(), default);

        deleted.ShouldBe(0);
        dbContext.SpaceRequests.Count().ShouldBe(2);
    }

    private static SpaceRequest Refused(string reference, DateTime refusedOn) =>
        new(reference, SpaceRequestType.Gym, "Atlas Training Club")
        {
            Status = SpaceRequestStatus.Refused,
            RefusedOn = refusedOn,
            ReceivedOn = refusedOn.AddDays(-2)
        };

    private static SubmitSpaceRequestCommandHandler Handler(
        GymDbContext dbContext,
        TestEmailSender emails)
        => new(dbContext, emails, new SubmitSpaceRequestCommandValidator());

    /// <summary>
    /// A request that passes, with the one thing each test varies. Written out
    /// rather than mutated with `with`: the command is a plain class like every
    /// other command here, and making it a record to suit a test would be the
    /// test choosing the production shape.
    /// </summary>
    private static SubmitSpaceRequestCommand Valid(
        string subdomain = "atlas-training",
        SpaceRequestType type = SpaceRequestType.Gym,
        bool withLocation = false,
        bool acceptedDataProcessing = true,
        string? website = null) => new()
    {
        Type = type,
        Name = "Atlas Training Club",
        ContactFirstName = "Camille",
        ContactLastName = "Fournier",
        ContactEmail = "camille@atlas-training.fr",
        RequestedPlan = PlatformPlans.Pro,
        RequestedSubdomain = subdomain,
        AcceptedTerms = true,
        AcceptedDataProcessing = acceptedDataProcessing,
        Website = website,

        // Both shapes filled at once, so the handler has something to drop.
        Street = withLocation ? "12 avenue des Sports" : null,
        ZipCode = withLocation ? "74200" : null,
        City = withLocation ? "Thonon-les-Bains" : null,
        AreaLabel = withLocation ? "Thonon et 30 km alentour" : null
    };
}
