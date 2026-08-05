namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// Which school-holiday zone a postcode falls in. France splits métropole into
/// three zones whose holidays are staggered, and the zone follows the
/// département — the first two digits of the postcode.
/// <para>
/// Ported as it stands from the prototype's <c>gxZoneForZip</c>. Zone A is the
/// fallback for anything unrecognised, including the overseas départements: the
/// banner then says something slightly wrong rather than nothing at all, which
/// is what the prototype does.
/// </para>
/// </summary>
public static class SchoolZones
{
    public const string DefaultZone = "A";

    private static readonly Dictionary<string, string> ZoneByDepartment = Build();

    /// <summary>The zone of a postcode: "A", "B" or "C".</summary>
    public static string ForPostcode(string? postcode)
    {
        var department = (postcode ?? string.Empty).Trim();

        return department.Length >= 2 && ZoneByDepartment.TryGetValue(department[..2], out var zone)
            ? zone
            : DefaultZone;
    }

    private static Dictionary<string, string> Build()
    {
        var zones = new Dictionary<string, string>
        {
            ["A"] = "01 03 07 15 16 17 19 21 23 24 25 26 33 38 39 40 42 43 47 58 63 64 69 70 71 73 74 79 86 87 89 90",
            ["B"] = "02 04 05 06 08 10 13 14 18 22 27 28 29 35 36 37 41 44 45 49 50 51 52 53 54 55 56 57 59 60 61 62 "
                    + "67 68 72 76 80 83 84 85 88",
            ["C"] = "09 11 12 30 31 32 34 46 48 65 66 75 77 78 81 82 91 92 93 94 95"
        };

        return zones
            .SelectMany(zone => zone.Value.Split(' '), (zone, department) => (department, zone.Key))
            .ToDictionary(pair => pair.department, pair => pair.Key);
    }
}
