using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// An access invitation waiting to be taken up — a collaborator asked to join
/// the gestion side, or a member asked to open their espace.
/// <para>
/// The account itself is only created when the invitation is accepted, so this
/// is the sole record of somebody who has been asked and has not yet answered.
/// Identity has nowhere to say <em>when</em> the ask went out, and « Renvoyer »
/// and « Invité il y a 2 j » both need that date.
/// </para>
/// </summary>
public class Invitation : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    /// <summary>Address the invitation was sent to. What identifies it.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Role the account will hold once accepted — a value of
    /// <c>GymRoleNames</c>. Stored rather than inferred because inviting a coach
    /// and inviting a member are the same gesture on two different screens.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// The member whose espace this opens. Null for an invitation to the
    /// gestion team — which is also what tells the two apart, so there is no
    /// separate kind to keep in step with it.
    /// </summary>
    public int? MemberId { get; set; }

    public Member? Member { get; set; }

    /// <summary>
    /// When the invitation last went out. A resend moves it, which is why this
    /// is a timestamp and not a date: two sends in one morning have to be
    /// tellable apart even though the row only ever prints « il y a 2 j ».
    /// </summary>
    public DateTime SentOn { get; set; }

    /// <summary>Set when the account is created. A row with a date here is history.</summary>
    public DateTime? AcceptedOn { get; set; }

    /// <summary>Still waiting: neither taken up nor withdrawn.</summary>
    public bool IsPending => IsActive && AcceptedOn is null;
}
