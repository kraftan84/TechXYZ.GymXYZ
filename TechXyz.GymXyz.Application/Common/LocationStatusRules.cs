using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// The venue standing rule. Unlike the member and coach ones it needs no date
/// and no second expression over the entity: the catalogue has no status filter
/// to translate to SQL, so one function serves both the list and the record.
/// </summary>
public static class LocationStatusRules
{
    /// <summary>
    /// Order matters. A venue at the member's home is answered by its kind
    /// before anything else, and an outdoor spot that does not care about the
    /// weather is simply available.
    /// <para>
    /// Occupancy is asked last, after the three chips that describe what a venue
    /// *is*: a plateau in open access stays "Accès libre" however busy it gets,
    /// because that is the useful thing to say about it. Only an ordinary studio
    /// turns to "Forte demande".
    /// </para>
    /// </summary>
    public static LocationStatus Resolve(
        LocationKind kind,
        bool isOpenAccess,
        bool isWeatherDependent,
        int? occupancyRate = null)
    {
        if (kind == LocationKind.Home)
        {
            return LocationStatus.ByAppointment;
        }

        if (kind == LocationKind.Outdoor && isWeatherDependent)
        {
            return LocationStatus.WeatherDependent;
        }

        if (isOpenAccess)
        {
            return LocationStatus.OpenAccess;
        }

        return occupancyRate >= PlanningRules.HighDemandThreshold
            ? LocationStatus.HighDemand
            : LocationStatus.Available;
    }
}
