namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What became of one payment. This is the only stored money state in the
/// application — the standing of a subscription is derived from the cover and
/// from these rows, never written down.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Announced but not in yet — a direct debit on its way.</summary>
    Pending,

    Collected,

    /// <summary>Came back: an unpaid direct debit, a bounced cheque.</summary>
    Rejected
}
