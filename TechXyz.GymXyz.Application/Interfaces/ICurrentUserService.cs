namespace TechXyz.GymXyz.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserName { get; }

    /// <summary>
    /// Whether the signed-in user holds the role. Asked by handlers that reserve
    /// an action — reopening a validated attendance sheet is the first — because
    /// the handler is the only place a caller cannot go around. Hiding the
    /// button is courtesy; this is the rule.
    /// </summary>
    bool IsInRole(string role);
}
