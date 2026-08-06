using FluentValidation;
using Microsoft.FluentUI.AspNetCore.Components;

namespace TechXyz.GymXyz.WebApp.Services;

public sealed class UserFeedbackService : IUserFeedbackService
{
    private readonly IToastService _toastService;
    private readonly ILogger<UserFeedbackService> _logger;

    public UserFeedbackService(IToastService toastService, ILogger<UserFeedbackService> logger)
    {
        _toastService = toastService;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Func<Task> action, string actionDescription, string? successMessage = null)
    {
        try
        {
            await action();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                ShowSuccess(successMessage);
            }

            return true;
        }
        catch (Exception exception)
        {
            Handle(exception, actionDescription);
            return false;
        }
    }

    public void Handle(Exception exception, string actionDescription)
    {
        if (exception is ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Validation failure while trying to {ActionDescription}", actionDescription);
            _toastService.ShowWarning(BuildValidationMessage(validationException, actionDescription));
            return;
        }

        if (exception is OperationCanceledException)
        {
            _logger.LogInformation(exception, "Operation canceled while trying to {ActionDescription}", actionDescription);
            _toastService.ShowInfo($"Operation annulee ({actionDescription}).");
            return;
        }

        _logger.LogError(exception, "Unexpected error while trying to {ActionDescription}", actionDescription);
        _toastService.ShowError($"Impossible de {actionDescription}. Une erreur technique est survenue.");
    }

    public void ShowSuccess(string message)
    {
        _toastService.ShowSuccess(message);
    }

    public void ShowPartial(string message)
    {
        _logger.LogWarning("Partial success reported to the user: {Message}", message);
        _toastService.ShowWarning(message);
    }

    private static string BuildValidationMessage(ValidationException exception, string actionDescription)
    {
        var messages = exception.Errors
            .Select(error => error.ErrorMessage?.Trim())
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToList();

        if (messages.Count == 0)
        {
            return $"Validation invalide. Verifiez les donnees pour {actionDescription}.";
        }

        var details = string.Join(" ", messages.Select(message => $"• {message}"));
        return $"Impossible de {actionDescription}. {details}";
    }
}
