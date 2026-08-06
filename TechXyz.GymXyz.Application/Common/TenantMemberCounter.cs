using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Counts members per customer for the TechXYZ console — the client list shows
/// one count per row, and the billing panel fills its gauge with the same number.
///
/// <para>
/// <b>This is the one place in the product that disarms the tenant filter, and
/// it must stay the only one.</b> Every business entity is <c>ITenantScoped</c>
/// and filtered globally, so a plain <c>Members.Count()</c> only ever counts the
/// tenant the request is being served as. Counting across customers therefore
/// needs either one scoped query per customer, or one query with the filter
/// lifted. This takes the second: a console listing every customer would
/// otherwise fire a round trip per row.
/// </para>
///
/// <para>
/// What makes lifting the filter safe here, and unsafe almost anywhere else:
/// nothing but a count leaves this method. No member row, no name, no id — a
/// grouped integer per tenant, which is exactly what the caller is allowed to
/// know. Do not widen the projection, and do not copy this pattern into a
/// handler: the filter is what stops one customer reading another's data, and
/// the reason it is disarmed here is written above.
/// </para>
/// </summary>
public static class TenantMemberCounter
{
    /// <summary>
    /// Active members of every customer, keyed by tenant. A customer with no
    /// member at all is absent from the dictionary rather than present at zero —
    /// callers read it through <see cref="CountFor"/>, which answers zero.
    /// </summary>
    public static async Task<IReadOnlyDictionary<int, int>> CountActiveByTenantAsync(
        this IGymDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var counts = await dbContext.Members
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(member => member.IsActive)
            .GroupBy(member => member.TenantId)
            .Select(group => new { TenantId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(row => row.TenantId, row => row.Count);
    }

    /// <summary>Reads one customer's count out of the grouped result.</summary>
    public static int CountFor(this IReadOnlyDictionary<int, int> counts, int tenantId)
    {
        return counts.TryGetValue(tenantId, out var count) ? count : 0;
    }
}
