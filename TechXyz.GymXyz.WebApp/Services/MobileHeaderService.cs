namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Lets a page swap the mobile header for the sub-screen variant — back arrow
/// plus title — without owning the shell. The desktop shell ignores it: there,
/// the same "go back" affordance is the breadcrumb.
/// </summary>
public sealed class MobileHeaderService
{
    public string? Title { get; private set; }

    public Action? Back { get; private set; }

    public bool IsSubScreen => Title is not null;

    public event Action? Changed;

    public void UseSubHeader(string title, Action back)
    {
        if (Title == title && Back == back)
            return;

        Title = title;
        Back = back;
        Changed?.Invoke();
    }

    /// <summary>Back to the brand header. Pages call this when they go away.</summary>
    public void UseRootHeader()
    {
        if (Title is null && Back is null)
            return;

        Title = null;
        Back = null;
        Changed?.Invoke();
    }
}
