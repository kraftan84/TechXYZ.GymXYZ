using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Asks for a reset link. Public: it is the one command in the product that runs
/// with nobody signed in.
/// <para>
/// It reports nothing. Not out of carelessness — out of the rule: the screen
/// answers the same sentence whether the address has an account or not, and a
/// command that returned "found" would give the caller something to leak.
/// </para>
/// </summary>
public sealed class RequestPasswordResetCommand : IRequest
{
    public RequestPasswordResetCommand(string email, string resetPageUrl)
    {
        Email = email.Trim();
        ResetPageUrl = resetPageUrl;
    }

    public string Email { get; }

    /// <summary>
    /// Absolute address of the reset screen, without a query — the handler adds
    /// the token. Supplied by the caller because only the web layer knows which
    /// host this customer was reached on, and a link on the wrong subdomain lands
    /// on the wrong brand.
    /// </summary>
    public string ResetPageUrl { get; }
}
