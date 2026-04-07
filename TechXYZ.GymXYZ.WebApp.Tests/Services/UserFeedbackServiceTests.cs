using FluentValidation.Results;
using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Shouldly;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXYZ.GymXYZ.WebApp.Tests.Services;

public class UserFeedbackServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnTrueAndShowSuccess_WhenActionSucceeds()
    {
        var faker = new Faker("fr");
        var actionDescription = faker.Lorem.Sentence(3);
        var successMessage = faker.Lorem.Sentence(4);

        var toastService = new ToastService();
        var capture = new ToastCapture(toastService);
        var service = new UserFeedbackService(toastService, new TestLogger<UserFeedbackService>());

        var result = await service.ExecuteAsync(() => Task.CompletedTask, actionDescription, successMessage);

        result.ShouldBeTrue();
        capture.Events.Count.ShouldBe(1);
        capture.Events[0].Intent.ShouldBe(ToastIntent.Success);
        capture.Events[0].Title.ShouldBe(successMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalseAndShowWarning_WhenValidationExceptionIsThrown()
    {
        var faker = new Faker("fr");
        var actionDescription = faker.Lorem.Sentence(3);

        var toastService = new ToastService();
        var capture = new ToastCapture(toastService);
        var service = new UserFeedbackService(toastService, new TestLogger<UserFeedbackService>());

        var validationException = new FluentValidation.ValidationException(new List<ValidationFailure>
        {
            new("FirstName", "Le prenom est obligatoire.")
        });

        var result = await service.ExecuteAsync(
            () => throw validationException,
            actionDescription);

        result.ShouldBeFalse();
        capture.Events.Count.ShouldBe(1);
        capture.Events[0].Intent.ShouldBe(ToastIntent.Warning);
        var title = capture.Events[0].Title;
        title.ShouldNotBeNull();
        var nonNullTitle = title!;
        nonNullTitle.ShouldContain("Impossible de");
        nonNullTitle.ShouldContain("Le prenom est obligatoire.");
    }

    [Fact]
    public void Handle_ShouldShowInfo_WhenOperationIsCanceled()
    {
        var faker = new Faker("fr");
        var actionDescription = faker.Lorem.Sentence(3);

        var toastService = new ToastService();
        var capture = new ToastCapture(toastService);
        var service = new UserFeedbackService(toastService, new TestLogger<UserFeedbackService>());

        service.Handle(new OperationCanceledException(), actionDescription);

        capture.Events.Count.ShouldBe(1);
        capture.Events[0].Intent.ShouldBe(ToastIntent.Info);
        var title = capture.Events[0].Title;
        title.ShouldNotBeNull();
        var nonNullTitle = title!;
        nonNullTitle.ShouldContain("Operation annulee");
    }

    [Fact]
    public void Handle_ShouldShowError_WhenUnexpectedExceptionOccurs()
    {
        var faker = new Faker("fr");
        var actionDescription = faker.Lorem.Sentence(3);

        var toastService = new ToastService();
        var capture = new ToastCapture(toastService);
        var service = new UserFeedbackService(toastService, new TestLogger<UserFeedbackService>());

        service.Handle(new InvalidOperationException("boom"), actionDescription);

        capture.Events.Count.ShouldBe(1);
        capture.Events[0].Intent.ShouldBe(ToastIntent.Error);
        var title = capture.Events[0].Title;
        title.ShouldNotBeNull();
        var nonNullTitle = title!;
        nonNullTitle.ShouldContain("Impossible de");
        nonNullTitle.ShouldContain("erreur technique");
    }

    private sealed class ToastCapture
    {
        public List<(ToastIntent Intent, string? Title)> Events { get; } = [];

        public ToastCapture(ToastService toastService)
        {
            toastService.OnShow += (_, parameters, _) =>
            {
                Events.Add((parameters.Intent, parameters.Title));
            };
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
