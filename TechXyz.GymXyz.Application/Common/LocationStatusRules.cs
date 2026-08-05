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
    /// </summary>
    public static LocationStatus Resolve(LocationKind kind, bool isOpenAccess, bool isWeatherDependent)
    {
        if (kind == LocationKind.Home)
        {
            return LocationStatus.ByAppointment;
        }

        if (kind == LocationKind.Outdoor && isWeatherDependent)
        {
            return LocationStatus.WeatherDependent;
        }

        return isOpenAccess ? LocationStatus.OpenAccess : LocationStatus.Available;
    }
}
