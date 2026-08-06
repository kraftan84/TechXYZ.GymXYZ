namespace TechXyz.GymXyz.WebApp.Services;

public interface IUserFeedbackService
{
    Task<bool> ExecuteAsync(Func<Task> action, string actionDescription, string? successMessage = null);

    void Handle(Exception exception, string actionDescription);

    void ShowSuccess(string message);

    /// <summary>
    /// The action worked and something beside it did not — a session cancelled
    /// whose registrants could not all be told. Distinct from
    /// <see cref="Handle"/>, which reports a failure: this says what was done
    /// <em>and</em> what was not, because a red toast on a cancellation that
    /// went through would have somebody cancel it again.
    /// </summary>
    void ShowPartial(string message);
}
