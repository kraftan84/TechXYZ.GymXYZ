using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Reads the public holidays and the school holidays from the two open data
/// sources of the French administration, server-side, and caches them.
/// <para>
/// This is the application's first outbound call, so it is deliberately meek: a
/// short timeout, a long cache, and every failure swallowed into an unavailable
/// calendar. The planning renders without its banner if the sources are down —
/// never an exception at render time.
/// </para>
/// </summary>
public sealed class SchoolCalendarService : ISchoolCalendarService
{
    public const string HttpClientName = "school-calendar";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SchoolCalendarService> _logger;
    private readonly ExternalApiOptions _options;

    public SchoolCalendarService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<SchoolCalendarService> logger,
        IOptions<ExternalApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    private TimeSpan CacheDuration => TimeSpan.FromHours(_options.CacheHours);

    public async Task<SchoolCalendarDto> GetAsync(
        string? postcode,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var zone = SchoolZones.ForPostcode(postcode);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            // A span can straddle new year, so both years are asked for.
            var years = Enumerable.Range(from.Year, Math.Max(1, to.Year - from.Year + 1)).ToList();

            var holidays = new List<PublicHolidayDto>();
            foreach (var year in years)
            {
                holidays.AddRange(await GetHolidaysAsync(year, timeout.Token));
            }

            var vacations = await GetVacationsAsync(zone, years.Min(), years.Max(), timeout.Token);

            return new SchoolCalendarDto(zone, holidays, vacations);
        }
        catch (Exception exception) when (exception is not OperationCanceledException
                                          || !cancellationToken.IsCancellationRequested)
        {
            // Including the timeout: the calendar is decoration on a screen that
            // has to render regardless.
            _logger.LogWarning(exception, "School calendar unavailable for zone {Zone}.", zone);

            return SchoolCalendarDto.Unavailable(zone);
        }
    }

    private async Task<IReadOnlyList<PublicHolidayDto>> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync($"school-calendar:holidays:{year}", async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var client = _httpClientFactory.CreateClient(HttpClientName);
            var payload = await client.GetFromJsonAsync<Dictionary<string, string>>(
                _options.PublicHolidaysUrl.Replace("{year}", year.ToString(CultureInfo.InvariantCulture)),
                JsonOptions,
                cancellationToken);

            return (payload ?? [])
                .Where(day => DateOnly.TryParse(day.Key, CultureInfo.InvariantCulture, out _))
                .Select(day => new PublicHolidayDto(
                    DateOnly.Parse(day.Key, CultureInfo.InvariantCulture),
                    day.Value))
                .OrderBy(holiday => holiday.Date)
                .ToList() as IReadOnlyList<PublicHolidayDto>;
        }) ?? [];
    }

    private async Task<IReadOnlyList<SchoolVacationDto>> GetVacationsAsync(
        string zone,
        int firstYear,
        int lastYear,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"school-calendar:vacations:{zone}:{firstYear}:{lastYear}";

        return await _cache.GetOrCreateAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;

            // The school year straddles the calendar one, so the window reaches
            // into the following February to catch the winter holidays.
            var where = Uri.EscapeDataString(
                $"zones=\"Zone {zone}\" and end_date>=\"{firstYear}-01-01\" and start_date<=\"{lastYear + 1}-02-28\"");

            var client = _httpClientFactory.CreateClient(HttpClientName);
            var payload = await client.GetFromJsonAsync<VacationResponse>(
                $"{_options.SchoolVacationsUrl}?where={where}&order_by=start_date&limit=100",
                JsonOptions,
                cancellationToken);

            return (payload?.Results ?? [])
                // The teachers' own dates are published alongside the pupils'
                // and would double every span.
                .Where(record => !string.Equals(record.Population, "Enseignants", StringComparison.OrdinalIgnoreCase))
                .Select(ToVacation)
                .OfType<SchoolVacationDto>()
                .DistinctBy(vacation => (vacation.Label, vacation.Start))
                .OrderBy(vacation => vacation.Start)
                .ToList() as IReadOnlyList<SchoolVacationDto>;
        }) ?? [];
    }

    private static SchoolVacationDto? ToVacation(VacationRecord record)
    {
        if (record.Description is null ||
            !DateTimeOffset.TryParse(record.StartDate, CultureInfo.InvariantCulture, out var start) ||
            !DateTimeOffset.TryParse(record.EndDate, CultureInfo.InvariantCulture, out var end))
        {
            return null;
        }

        // The source publishes the day term restarts; the last day off is the
        // one before. Read in Paris time, because that is the calendar the dates
        // belong to whatever the server's own clock is set to.
        return new SchoolVacationDto(
            record.Description,
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(start, ParisTimeZone).DateTime),
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(end, ParisTimeZone).DateTime).AddDays(-1));
    }

    private static readonly TimeZoneInfo ParisTimeZone = ResolveParisTimeZone();

    private static TimeZoneInfo ResolveParisTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private sealed record VacationResponse(
        [property: JsonPropertyName("results")] List<VacationRecord>? Results);

    private sealed record VacationRecord(
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("start_date")] string? StartDate,
        [property: JsonPropertyName("end_date")] string? EndDate,
        [property: JsonPropertyName("population")] string? Population);
}
