using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Opens a new GymXYZ customer. Answers its id.
/// <para>
/// Takes only what a customer cannot exist without: a name, the host prefix that
/// resolves it, and a theme. Everything else — address, logo, plan — is filled
/// afterwards from the panels, because a customer signed up on the phone has a
/// name before it has a wordmark.
/// </para>
/// <para>
/// The row is the whole job. As <c>Tenant</c> puts it, adding a customer is a row
/// here plus a block of tokens in themes.css, never a screen change.
/// </para>
/// </summary>
public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string ThemeKey,
    bool IsSolo) : IRequest<int>;
