using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TechXyz.GymXyz.Persistence.Converters;

public class Converters() : ValueConverter<DateOnly, DateTime>(
    dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
    dateTime => DateOnly.FromDateTime(dateTime));

public class TimeOnlyConverter() : ValueConverter<TimeOnly, TimeSpan>(
    timeOnly => timeOnly.ToTimeSpan(),
    timeOnly => TimeOnly.FromTimeSpan(timeOnly));