using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One visit of a platform admin inside a customer's data: who entered, whose
/// data, when in and when out.
/// <para>
/// This is a trail, not a session store — nothing reads it to decide what a
/// request is allowed to see. Impersonation is carried by the authentication
/// cookie; this only records that it happened. Which is why an unclosed row
/// (<see cref="EndedAt"/> still null) means the admin left without using the
/// exit, not that the visit is still live.
/// </para>
/// <para>
/// Like <see cref="Tenant"/> and <see cref="Invoice"/>, it carries a
/// <see cref="TenantId"/> without being <c>ITenantScoped</c>: an audit row about
/// a customer must stay readable from outside that customer, and must not be
/// erasable from inside it.
/// </para>
/// </summary>
public class TenantImpersonation : EntityBase<int>
{
    /// <summary>Account of the platform admin who entered.</summary>
    public string AdminUserId { get; set; } = string.Empty;

    /// <summary>
    /// Address of that account, copied at the time. Kept as a copy on purpose:
    /// the trail has to stay readable after the account is renamed or removed.
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>Customer whose data was entered.</summary>
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public DateTime StartedAt { get; set; }

    /// <summary>Null until the admin uses the exit.</summary>
    public DateTime? EndedAt { get; set; }
}
