using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The credit packs behind one attendance sheet, and the one place an entry is
/// spent or given back.
/// <para>
/// A pack is debited by the pointing and never by the sign-up — a no-show would
/// otherwise eat a session nobody attended. It is debited <b>once</b>: the stamp
/// lives on the registration, so pointing the same seat twice finds it already
/// set and changes nothing. That is the whole of invariant 6, and it is written
/// here rather than in the two commands that point because both of them can run
/// over the same seat and neither can see what the other did.
/// </para>
/// <para>
/// Correcting a seat back to absent gives the entry back. The refund goes to the
/// subscription the stamp names, not to whichever pack is current: by the time
/// somebody fixes a sheet, the pack that paid for it may well have lapsed, and
/// crediting a different one would move money between two things the member
/// bought separately.
/// </para>
/// </summary>
public sealed class CreditLedger
{
    private readonly Dictionary<int, List<Subscription>> _spendableByMember;
    private readonly Dictionary<int, Subscription> _byId;

    private CreditLedger(Dictionary<int, List<Subscription>> spendableByMember, Dictionary<int, Subscription> byId)
    {
        _spendableByMember = spendableByMember;
        _byId = byId;
    }

    public static CreditLedger Empty { get; } = new([], []);

    /// <summary>
    /// Loads what the given seats could touch: the packs covering the day of the
    /// session, plus whichever subscriptions those seats already took an entry
    /// from — a refund has to reach its own pack even after it has lapsed.
    /// <para>
    /// Tracked on purpose. These rows are written.
    /// </para>
    /// </summary>
    public static async Task<CreditLedger> LoadAsync(
        IGymDbContext dbContext,
        IReadOnlyCollection<Registration> seats,
        DateOnly on,
        CancellationToken cancellationToken)
    {
        if (seats.Count == 0)
        {
            return Empty;
        }

        var memberIds = seats.Select(seat => seat.MemberId).Distinct().ToList();
        var debitedIds = seats
            .Where(seat => seat.CreditDebitedFromSubscriptionId is not null)
            .Select(seat => seat.CreditDebitedFromSubscriptionId!.Value)
            .Distinct()
            .ToList();

        var subscriptions = await dbContext.Subscriptions
            .Where(subscription =>
                (subscription.IsActive &&
                 subscription.CreditsRemaining != null &&
                 memberIds.Contains(subscription.MemberId) &&
                 subscription.StartedOn <= on &&
                 subscription.EndsOn >= on)
                || debitedIds.Contains(subscription.Id))
            .ToListAsync(cancellationToken);

        var spendable = subscriptions
            .Where(subscription =>
                subscription.IsActive &&
                subscription.CreditsRemaining is not null &&
                subscription.StartedOn <= on &&
                subscription.EndsOn >= on)
            .GroupBy(subscription => subscription.MemberId)
            // Soonest to lapse first: an entry is worth more spent on the pack
            // about to run out than on the one bought last week.
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(subscription => subscription.EndsOn)
                    .ThenBy(subscription => subscription.Id)
                    .ToList());

        return new CreditLedger(spendable, subscriptions.ToDictionary(subscription => subscription.Id));
    }

    /// <summary>
    /// Brings one seat's credit into line with the verdict just recorded on it.
    /// Idempotent by construction: the stamp, not the tap, is what says whether
    /// this seat has already been paid for.
    /// </summary>
    public void Settle(Registration registration, AttendanceStatus status)
    {
        if (AttendanceRules.CountsAsAttended(status))
        {
            Debit(registration);
        }
        else
        {
            Refund(registration);
        }
    }

    private void Debit(Registration registration)
    {
        if (registration.CreditDebitedFromSubscriptionId is not null)
        {
            return;
        }

        if (!_spendableByMember.TryGetValue(registration.MemberId, out var packs))
        {
            return;
        }

        var pack = packs.FirstOrDefault(candidate => candidate.CreditsRemaining > 0);
        if (pack is null)
        {
            // Nothing left to take. The seat is still pointed — attendance is a
            // fact, and refusing to record it because a pack ran dry would hide
            // the very member who needs to renew.
            return;
        }

        pack.CreditsRemaining -= 1;
        registration.CreditDebitedFromSubscriptionId = pack.Id;
    }

    private void Refund(Registration registration)
    {
        if (registration.CreditDebitedFromSubscriptionId is not { } subscriptionId)
        {
            return;
        }

        if (_byId.TryGetValue(subscriptionId, out var pack) && pack.CreditsRemaining is { } remaining)
        {
            pack.CreditsRemaining = pack.CreditsTotal is { } total
                ? Math.Min(total, remaining + 1)
                : remaining + 1;
        }

        registration.CreditDebitedFromSubscriptionId = null;
    }
}
