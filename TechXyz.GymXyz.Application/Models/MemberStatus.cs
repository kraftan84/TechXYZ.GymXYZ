namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Member standing as shown in the list and on the record. Always derived,
/// never stored: see <c>MemberStatusRules</c> for the single definition.
/// </summary>
public enum MemberStatus
{
    /// <summary>No subscription covering today.</summary>
    Inactive,

    /// <summary>Covered today, but the cover ends within the warning window.</summary>
    ExpiringSoon,

    /// <summary>Covered today and beyond the warning window.</summary>
    Active
}
