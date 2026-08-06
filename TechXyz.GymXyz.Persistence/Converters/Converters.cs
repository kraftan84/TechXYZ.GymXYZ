using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Persistence.Converters;

public class Converters() : ValueConverter<DateOnly, DateTime>(
    dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
    dateTime => DateOnly.FromDateTime(dateTime));

public class TimeOnlyConverter() : ValueConverter<TimeOnly, TimeSpan>(
    timeOnly => timeOnly.ToTimeSpan(),
    timeOnly => TimeOnly.FromTimeSpan(timeOnly));

/// <summary>
/// The accepted payment methods, as one column. A handful of values read and
/// written together and never queried apart, so a child table would buy nothing
/// and cost a join on every settings read.
/// <para>
/// Names rather than ordinals, matching the string conversion every other enum
/// in this model gets: renumbering <see cref="PaymentMethod"/> must not silently
/// turn cash into cheques. Unknown names are dropped on read so a value removed
/// from the enum degrades to "not accepted" instead of throwing on load.
/// </para>
/// </summary>
public class PaymentMethodListConverter() : ValueConverter<List<PaymentMethod>, string>(
    methods => string.Join(Separator, methods.Select(method => method.ToString())),
    stored => Parse(stored))
{
    private const char Separator = ',';

    private static List<PaymentMethod> Parse(string stored) =>
        stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => Enum.TryParse<PaymentMethod>(name, out var method) ? method : (PaymentMethod?)null)
            .Where(method => method.HasValue)
            .Select(method => method!.Value)
            .Distinct()
            .ToList();
}

/// <summary>
/// Tells EF when the list actually changed. Without it the change tracker
/// compares the two lists by reference, and a method toggled in place would
/// never be saved.
/// </summary>
public class PaymentMethodListComparer() : ValueComparer<List<PaymentMethod>>(
    (left, right) => left!.SequenceEqual(right!),
    methods => methods.Aggregate(0, (hash, method) => HashCode.Combine(hash, method.GetHashCode())),
    methods => methods.ToList());