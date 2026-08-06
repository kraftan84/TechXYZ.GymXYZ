using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What a customer owes TechXYZ for its GymXYZ subscription.
/// <para>
/// Not to be confused with <see cref="Payment"/>, which is a member paying the
/// gym. This is the other direction, one level up: the gym paying us.
/// </para>
/// <para>
/// Carries a <see cref="TenantId"/> but is deliberately <b>not</b>
/// <c>ITenantScoped</c>: like <see cref="Tenant"/> it sits above the global
/// filter, because the only screen that reads it is the TechXYZ console, where
/// a platform admin reads the invoices of a customer he does not inhabit.
/// </para>
/// </summary>
public class Invoice : EntityBase<int>
{
    /// <summary>Customer the invoice is addressed to.</summary>
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    /// <summary>Human reference printed on the document ("GX-2026-001").</summary>
    public string Reference { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public decimal Amount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;
}
