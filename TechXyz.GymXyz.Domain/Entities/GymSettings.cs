using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// The customer's own configuration: one row per tenant.
/// <para>
/// Deliberately small. <see cref="Tenant"/> already carries the name, the
/// baseline, the capacity, the SIRET, the address and the
/// <see cref="Tenant.AreaLabel"/>, so the Identité panel edits mostly the tenant
/// and this holds what has nowhere else to live — money, tax, and the opening
/// hours.
/// </para>
/// </summary>
public class GymSettings : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }

    /// <summary>
    /// ISO code of what the gym charges in — EUR, CHF, CAD. The code rather than
    /// the printed label so a screen can format an amount without parsing
    /// « Franc suisse (CHF) ».
    /// </summary>
    public string Currency { get; set; } = DefaultCurrency;

    /// <summary>
    /// The line printed at the foot of an invoice — « TVA non applicable,
    /// art. 293 B du CGI ». Free text: the gym's accountant writes it, and the
    /// wordings vary by régime in ways no enum would keep up with.
    /// </summary>
    public string? VatMention { get; set; }

    /// <summary>
    /// What the gym takes payment in. A list rather than a parallel flags enum so
    /// there is one definition of what a payment method is — the same
    /// <see cref="PaymentMethod"/> an encaissement is recorded with.
    /// </summary>
    public List<PaymentMethod> AcceptedPaymentMethods { get; set; } = [];

    /// <summary>
    /// School-holiday zone, cached from the postcode. Derived rather than
    /// chosen — <c>SchoolZones.ForPostcode</c> owns the rule — but stored so the
    /// planning banner does not recompute it on every read, and refreshed
    /// whenever the identity panel saves a new postcode.
    /// </summary>
    public string? SchoolZone { get; set; }

    public ICollection<OpeningHours>? OpeningHours { get; set; }

    public const string DefaultCurrency = "EUR";

    public bool Accepts(PaymentMethod method) => AcceptedPaymentMethods.Contains(method);

    public void AddOpeningHours(OpeningHours hours)
    {
        OpeningHours ??= new List<OpeningHours>();
        hours.Rank = OpeningHours.Count;
        OpeningHours.Add(hours);
    }
}
