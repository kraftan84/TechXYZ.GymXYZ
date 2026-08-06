namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// Where a GymXYZ invoice stands. Deliberately short: nothing in the product
/// collects this money, so a row only ever records what TechXYZ observed.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Issued, not settled yet.</summary>
    Pending,

    Paid,

    /// <summary>Past its due date and still unsettled.</summary>
    Late
}
