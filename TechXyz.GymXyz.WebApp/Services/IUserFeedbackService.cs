namespace TechXyz.GymXyz.WebApp.Services;

public interface IUserFeedbackService
{
    Task<bool> ExecuteAsync(Func<Task> action, string actionDescription, string? successMessage = null);

    void Handle(Exception exception, string actionDescription);

    void ShowSuccess(string message);
}
