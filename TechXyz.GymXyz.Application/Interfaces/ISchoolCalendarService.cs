using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Interfaces;

/// <summary>
/// Public holidays and school holidays, read server-side from the two open data
/// sources of the French administration and cached.
/// <para>
/// The contract is that it never throws. A source that is down, slow or
/// malformed comes back as <see cref="SchoolCalendarDto.IsAvailable"/> false, so
/// no caller has to wrap it and no screen can fail to render because of it.
/// </para>
/// </summary>
public interface ISchoolCalendarService
{
    /// <summary>
    /// The calendar covering <paramref name="from"/> to <paramref name="to"/>,
    /// for the zone the postcode falls in.
    /// </summary>
    Task<SchoolCalendarDto> GetAsync(
        string? postcode,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
