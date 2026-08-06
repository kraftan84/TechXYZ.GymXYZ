using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Asks somebody to open an account — a collaborator on the gestion side, or a
/// member on their espace.
/// <para>
/// Records the invitation; it does <b>not</b> create the account. The account is
/// created when the invitation is taken up, so an address asked and never
/// answering leaves no nameless login behind. The e-mail carrying it goes out
/// with the messaging channel in the next PR; until then the row is the record
/// and the panel shows it as « en attente ».
/// </para>
/// </summary>
public sealed class InviteTeamMemberCommand : IRequest<bool>
{
    public InviteTeamMemberCommand(string email, string roleName, int? memberId = null)
    {
        Email = email.Trim();
        RoleName = roleName;
        MemberId = memberId;
    }

    public string Email { get; }

    /// <summary>A value of <c>GymRoleNames</c>, and one a customer may assign.</summary>
    public string RoleName { get; }

    /// <summary>Set when inviting a member to their espace; null for the team.</summary>
    public int? MemberId { get; }
}
