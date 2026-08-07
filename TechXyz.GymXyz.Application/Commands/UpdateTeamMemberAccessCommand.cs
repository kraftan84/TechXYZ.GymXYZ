using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Changes what a collaborator can open.
/// <para>
/// The scope the panel prints — « Planning, cours &amp; présences » — is read off
/// the role, so changing the scope <em>is</em> changing the role. There is no
/// second setting to keep in step with it.
/// </para>
/// </summary>
public sealed class UpdateTeamMemberAccessCommand : IRequest<bool>, IManagerOnly
{
    public UpdateTeamMemberAccessCommand(string userId, string roleName)
    {
        UserId = userId;
        RoleName = roleName;
    }

    public string UserId { get; }

    public string RoleName { get; }
}
