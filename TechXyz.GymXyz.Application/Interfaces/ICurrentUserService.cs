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

    /// <summary>
    /// The <c>Coach</c> this account is, or null for anybody who is not one.
    /// <para>
    /// Read from a claim written at sign-in rather than looked up per query: a
    /// Blazor circuit has no HttpContext to re-read from, and the Présences
    /// screen asks this question on every render. The cost is that linking a
    /// coach to their account mid-session takes effect at their next sign-in —
    /// the same bargain the tenant claim already makes.
    /// </para>
    /// </summary>
    int? CoachId { get; }
}
