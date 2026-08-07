using Shouldly;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Application.Queries;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

public class MemberDetailsPageQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMemberDetailsPage()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnMemberDetailsPage));

        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Email = faker.Internet.Email(),
            Phone = faker.Phone.PhoneNumber(),
            Address = new Address
            {
                Street = faker.Address.StreetAddress(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Country = faker.Address.Country()
            }
        };

        var coach = new Coach(faker.Name.FirstName(), faker.Name.LastName());
        var pastSession = new Session
        {
            CourseTemplate = new CourseTemplate("Private") { Discipline = new Discipline("Coaching") },
            Coach = coach,
            Location = new Location("Location A"),
            Capacity = 1,
            StartsAt = DateTime.Today.AddDays(-2).AddHours(10),
            EndsAt = DateTime.Today.AddDays(-2).AddHours(11)
        };
        var upcomingSession = new Session
        {
            CourseTemplate = new CourseTemplate("Collective") { Discipline = new Discipline("Renforcement") },
            Coach = coach,
            Location = new Location("Location B"),
            Capacity = 20,
            StartsAt = DateTime.Today.AddDays(2).AddHours(18),
            EndsAt = DateTime.Today.AddDays(2).AddHours(19)
        };
        var plan = TestPlans.Pack(credits: 12);
        var subscription = new Subscription
        {
            Member = member,
            Plan = plan,
            StartedOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
            EndsOn = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
            CreditsRemaining = 12,
            CreditsTotal = 12,
            PriceLabel = plan.FormatPriceLabel()
        };
        dbContext.Plans.Add(plan);

        member.Registrations =
        [
            new Registration { Session = pastSession, RegisteredAt = DateTime.Today.AddDays(-9) },
            new Registration { Session = upcomingSession, RegisteredAt = DateTime.Today.AddDays(-1) }
        ];
        member.Subscriptions = [subscription];

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(member.Id);
        result.Subscriptions.Count.ShouldBe(1);
        // "depuis l'inscription" counts what the member has been to, not the
        // seats already booked for the weeks ahead.
        result.Stats.TotalSessions.ShouldBe(1);

        // Sessions are split by the record's two cards.
        result.UpcomingSessions.Count.ShouldBe(1);
        result.UpcomingSessions[0].Name.ShouldBe("Collective");
        result.PastSessions.Count.ShouldBe(1);
        result.PastSessions[0].Name.ShouldBe("Private");

        // A capacity of one is what makes a session private — there is no type.
        result.PastSessions[0].IsPrivate.ShouldBeTrue();
        result.UpcomingSessions[0].IsPrivate.ShouldBeFalse();
        result.UpcomingSessions[0].RemainingSpots.ShouldBe(19);

        // Nothing on this member's seats has been pointed, so there is no
        // assiduité to show — null, not nought, and the card reads "—".
        result.Stats.AttendanceRate.ShouldBeNull();
        result.Stats.LastVisitOn.ShouldBeNull();

        // Nothing has been recorded against this member, so the payments card
        // is empty — which is not the same as it having no source.
        result.Payments.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCarryJoinDateNotesAndDerivedStanding()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldCarryJoinDateNotesAndDerivedStanding));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            JoinedOn = today.AddMonths(-14),
            BirthDate = today.AddYears(-32),
            Notes = "Préfère les cours du matin.",
            Subscriptions =
            [
                new Subscription
                {
                    Plan = TestPlans.Monthly(),
                    StartedOn = today.AddDays(-20),
                    EndsOn = today.AddDays(3),
                    PriceLabel = "49 € / mois"
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.JoinedOn.ShouldBe(today.AddMonths(-14));
        result.BirthDate.ShouldBe(today.AddYears(-32));
        result.Notes.ShouldBe("Préfère les cours du matin.");
        result.Status.ShouldBe(MemberStatus.ExpiringSoon);
        result.CurrentSubscription.ShouldNotBeNull();
        result.CurrentSubscription!.EndsOn.ShouldBe(today.AddDays(3));
    }

    [Fact]
    public async Task Handle_ShouldReportInactive_WhenNoSubscriptionCoversToday()
    {
        var faker = TestInfrastructure.Faker();
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReportInactive_WhenNoSubscriptionCoversToday));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var member = new Member(faker.Name.FirstName(), faker.Name.LastName())
        {
            Subscriptions =
            [
                new Subscription
                {
                    Plan = TestPlans.Monthly(),
                    StartedOn = today.AddMonths(-4),
                    EndsOn = today.AddMonths(-1),
                    PriceLabel = "49 € / mois"
                }
            ]
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Status.ShouldBe(MemberStatus.Inactive);
        result.CurrentSubscription.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldListThePaymentsNewestFirst()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldListThePaymentsNewestFirst));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var pack = TestPlans.Pack();
        var subscription = new Subscription
        {
            Plan = pack,
            StartedOn = today.AddDays(-90),
            EndsOn = today.AddDays(-25),
            CreditsRemaining = 0,
            CreditsTotal = 10,
            PriceLabel = pack.FormatPriceLabel(),
            Price = pack.Price
        };
        var member = new Member("Théo", "Garnier") { Subscriptions = [subscription] };

        // Both against the cover they paid for: "late" means this subscription
        // is unsettled, so a payment floating free of one cannot make it so.
        //
        // The debit bounced and only part of it was settled at the desk, so 60
        // of the 120 is still owing — a failure alone would not be enough.
        member.Payments =
        [
            new Payment
            {
                Subscription = subscription,
                Date = today.AddDays(-40), Label = pack.Name, Amount = 60m,
                Method = PaymentMethod.Cash, Status = PaymentStatus.Collected
            },
            new Payment
            {
                Subscription = subscription,
                Date = today.AddDays(-4), Label = pack.Name, Amount = 120m,
                Method = PaymentMethod.SepaDirectDebit, Status = PaymentStatus.Rejected
            }
        ];

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var handler = new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager());
        var result = await handler.Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Payments.Count.ShouldBe(2);
        result.Payments[0].Status.ShouldBe(PaymentStatus.Rejected);
        result.Payments[0].Method.ShouldBe(PaymentMethod.SepaDirectDebit);

        // The rejected direct debit is what separates a cover that merely ended
        // from one that is late — and the record folds "En retard" onto the
        // standing the members table shows, "Inactif".
        result.Status.ShouldBe(MemberStatus.Inactive);

        // "En cours" means in force: an expired cover is not one, so the card
        // falls back to offering a new subscription.
        result.CurrentSubscription.ShouldBeNull();
        result.Subscriptions.Single().Status.ShouldBe(SubscriptionStatus.Late);
    }

    [Fact]
    public async Task Handle_ShouldClearTheArrears_OnceTheBouncedPaymentIsSettled()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(
            nameof(Handle_ShouldClearTheArrears_OnceTheBouncedPaymentIsSettled));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var pack = TestPlans.Pack();
        var subscription = new Subscription
        {
            Plan = pack,
            StartedOn = today.AddDays(-90),
            EndsOn = today.AddDays(-25),
            CreditsRemaining = 0,
            CreditsTotal = 10,
            PriceLabel = pack.FormatPriceLabel(),
            Price = pack.Price
        };
        var member = new Member("Théo", "Garnier") { Subscriptions = [subscription] };

        // The direct debit bounced, and the whole 120 was then taken in cash at
        // the desk. The failure is still on the record — it happened — but
        // nothing is owed any more.
        member.Payments =
        [
            new Payment
            {
                Subscription = subscription,
                Date = today.AddDays(-4), Label = pack.Name, Amount = 120m,
                Method = PaymentMethod.SepaDirectDebit, Status = PaymentStatus.Rejected
            },
            new Payment
            {
                Subscription = subscription,
                Date = today.AddDays(-3), Label = pack.Name, Amount = 120m,
                Method = PaymentMethod.Cash, Status = PaymentStatus.Collected
            }
        ];

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var result = await new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager())
            .Handle(new GetMemberDetailsPageQuery(member.Id), CancellationToken.None);

        // A row that kept saying "En retard" after the gym took the money would
        // send somebody to chase it a second time.
        result.ShouldNotBeNull();
        result!.Subscriptions.Single().Status.ShouldBe(SubscriptionStatus.Ended);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMemberNotFound()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext(nameof(Handle_ShouldReturnNull_WhenMemberNotFound));
        var handler = new GetMemberDetailsPageQueryHandler(dbContext, TestCurrentUserService.Manager());

        var result = await handler.Handle(new GetMemberDetailsPageQuery(12345), CancellationToken.None);

        result.ShouldBeNull();
    }
}
