namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// The two natures of offer the gym sells, and the only thing that decides how a
/// subscription is consumed.
/// <para>
/// A <see cref="Recurring"/> plan is bought by the month or the year and runs
/// until its cover ends; a <see cref="CreditPack"/> is bought once and is spent
/// one entry at a time by the attendance sheet. The distinction is not
/// cosmetic — it is what keeps a pack out of the recurring revenue and what
/// makes the credit gauge mean something.
/// </para>
/// </summary>
public enum PlanKind
{
    Recurring,
    CreditPack
}
