using Microsoft.EntityFrameworkCore;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The "ordered list of free text" pattern, written once. A coach's
/// certifications and a venue's equipment are the same thing twice: lines the
/// user types, kept in the order they were typed, replaced wholesale on save.
/// <para>
/// Replaced rather than reconciled, unlike the link tables: these rows have no
/// identity of their own — nothing points at "certification 12" — so matching
/// them up by label would buy nothing and would mis-handle a rename.
/// </para>
/// </summary>
public static class OrderedLabelHelper
{
    /// <summary>
    /// Trimmed, blanks dropped, duplicates dropped, order preserved. Blank lines
    /// are what an empty row of the drawer sends, and storing them would show
    /// the user an empty chip.
    /// </summary>
    public static List<string> Normalize(IEnumerable<string>? labels)
    {
        if (labels is null)
        {
            return [];
        }

        return labels
            .Select(AddressHelper.NormalizeOptional)
            .Where(label => label is not null)
            .Select(label => label!)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Drops every existing row and writes the new ones back in order.
    /// <paramref name="create"/> receives the label and its rank and fills in
    /// whatever the entity needs beyond them — its parent key and its tenant.
    /// </summary>
    public static void Replace<TEntity>(
        DbSet<TEntity> set,
        IReadOnlyCollection<TEntity> existing,
        IReadOnlyList<string> labels,
        Func<string, int, TEntity> create)
        where TEntity : class
    {
        if (existing.Count > 0)
        {
            set.RemoveRange(existing);
        }

        for (var rank = 0; rank < labels.Count; rank++)
        {
            set.Add(create(labels[rank], rank));
        }
    }
}
