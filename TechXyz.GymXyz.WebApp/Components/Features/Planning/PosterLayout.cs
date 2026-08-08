using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXyz.GymXyz.WebApp.Components.Features.Planning;

/// <summary>What a poster cell is, once the week has been laid out.</summary>
public enum PosterCellKind
{
    /// <summary>A class.</summary>
    Slot,

    /// <summary>
    /// A cell that holds its place and shows nothing. Not an absent cell: the
    /// grid keeps its rhythm and the columns stay aligned from one day to the
    /// next, which is the whole reason the seven rows read as one object.
    /// </summary>
    Filler,

    /// <summary>« +2 autres » — a day fuller than the poster can draw.</summary>
    Overflow,

    /// <summary>« Repos » — nothing publishable that day.</summary>
    Rest
}

/// <summary>One cell of the poster grid.</summary>
public sealed record PosterCell(
    PosterCellKind Kind,
    string? Time = null,
    string? Name = null,
    string? Meta = null,
    bool IsFull = false,
    int Rhythm = 0);

/// <summary>
/// How a published week is arranged and worded, per brand. Pure and static: it
/// takes a <see cref="PosterWeekDto"/> and the tenant's theme key and answers in
/// strings, so the whole of it is testable without rendering anything.
/// <para>
/// The wording lives here rather than in the Application layer on purpose. What
/// a class is called under its hour is brand voice — « Studio A · 6 places » for
/// one customer, « 60 min · L. Fontaine » for another — and the rest of the
/// product keeps brand voice on this side of the line.
/// </para>
/// </summary>
public static class PosterLayout
{
    public const int Width = 1080;
    public const int Height = 1350;

    /// <summary>
    /// Rendered at twice the poster's size, which is what makes it printable and
    /// keeps it crisp on a retina phone.
    /// </summary>
    public const int Scale = 2;

    private const string Leyssa = "leyssa";
    private const string TeamTrainers = "teamtrainers";

    /// <summary>
    /// Beyond this the cells are too narrow to read at the size the poster is
    /// actually seen — 1080 px wide shown at 400 on a phone turns a fifth column
    /// into 42 px. Measured against the prototype: a day of four fits one row at
    /// four columns, and a fourth cell at three columns starts a second row that
    /// overlaps the day underneath.
    /// </summary>
    public const int MaxColumns = 4;

    /// <summary>
    /// How many columns the brand draws when nothing forces more. Leyssa is two
    /// by design — fewer classes, more air, which is what a solo coach's week
    /// looks like.
    /// </summary>
    public static int BaseColumns(string? themeKey) =>
        themeKey == Leyssa ? 2 : 3;

    /// <summary>
    /// The column count for the whole poster, read from the busiest day so that
    /// every day is drawn the same way. Per-day counts would break the alignment
    /// the layout is built on.
    /// </summary>
    public static int Columns(string? themeKey, int busiestDay) =>
        Math.Clamp(busiestDay, BaseColumns(themeKey), MaxColumns);

    /// <summary>
    /// How many block variants the brand alternates between, to give the rows a
    /// rhythm. Team Trainer's plays white / black / grey; Leyssa alternates rose
    /// and sage; the default skin has one block and varies nothing.
    /// </summary>
    private static int RhythmVariants(string? themeKey) => themeKey switch
    {
        TeamTrainers => 3,
        Leyssa => 2,
        _ => 1
    };

    /// <summary>
    /// The cells of one day: its classes, then fillers up to the column count —
    /// or a single « Repos » when there is nothing to show.
    /// </summary>
    public static IReadOnlyList<PosterCell> Cells(
        IReadOnlyList<PosterSessionDto> day,
        int dayIndex,
        int columns,
        string? themeKey)
    {
        if (day.Count == 0)
        {
            return [new PosterCell(PosterCellKind.Rest, Name: "Repos")];
        }

        var variants = RhythmVariants(themeKey);
        var cells = new List<PosterCell>(columns);

        // A day fuller than the grid keeps its first classes and says how many
        // it could not draw. Dropping the rest silently would advertise a
        // quieter gym than the one that exists.
        var shown = day.Count > columns ? columns - 1 : day.Count;

        for (var index = 0; index < shown; index++)
        {
            var session = day[index];

            cells.Add(new PosterCell(
                PosterCellKind.Slot,
                Time: Hour(session),
                Name: session.CourseName,
                Meta: Meta(session, themeKey),
                IsFull: session.IsFull,
                // Deterministic, and varying along both axes so neighbours
                // differ: the same week must always produce the same image.
                Rhythm: (dayIndex + index) % variants));
        }

        if (day.Count > columns)
        {
            var hidden = day.Count - shown;

            cells.Add(new PosterCell(
                PosterCellKind.Overflow,
                Name: $"+{hidden} autre{(hidden > 1 ? "s" : string.Empty)}"));
        }

        while (cells.Count < columns)
        {
            cells.Add(new PosterCell(PosterCellKind.Filler));
        }

        return cells;
    }

    /// <summary>« 08h00 », as the poster writes an hour.</summary>
    public static string Hour(PosterSessionDto session) =>
        session.StartsAt.ToString("HH'h'mm", GxFormats.Culture);

    /// <summary>
    /// The line under the class name. Three brands, three things worth saying:
    /// a gym sells its rooms and its remaining seats, a studio sells its coach
    /// and the length of the effort, a solo coach sells where and how much room
    /// is left.
    /// <para>
    /// Never the enrolled headcount, and never a capacity beside the remainder —
    /// the mockup writes « 6 places · 2 restantes » for Leyssa, and the two
    /// together say that four people signed up. Seats <b>left</b> only.
    /// </para>
    /// </summary>
    public static string Meta(PosterSessionDto session, string? themeKey) => themeKey switch
    {
        TeamTrainers => session.IsFull
            ? $"{session.DurationMinutes} min · complet"
            : $"{session.DurationMinutes} min · {session.CoachShortName ?? session.LocationName}",

        Leyssa => session.IsFull
            ? $"{session.LocationName} · complet"
            : $"{session.LocationName} · {Seats(session.RemainingSeats)} restante{(session.RemainingSeats > 1 ? "s" : string.Empty)}",

        _ => session.IsFull
            ? "Complet · liste d'attente"
            : $"{session.LocationName} · {Seats(session.RemainingSeats)}"
    };

    private static string Seats(int remaining) =>
        remaining > 1 ? $"{remaining} places" : $"{remaining} place";

    /// <summary>« LUN » for most, « Lundi » for Leyssa, whose day names are the ornament.</summary>
    public static string DayName(DateOnly day, string? themeKey)
    {
        var index = ((int)day.DayOfWeek + 6) % 7;

        return themeKey == Leyssa
            ? PlanningFilters.LongDayLabels[index]
            : PlanningFilters.DayLabels[index];
    }

    /// <summary>« 08/06 », under the day name.</summary>
    public static string DayNumber(DateOnly day) => day.ToString("dd/MM", GxFormats.Culture);

    /// <summary>
    /// The headline date range, in each brand's register: « 8 → 14 juin » for the
    /// default skin, « 03 → 09 AOÛT » for Team Trainer's, « Du 8 au 14 juin » for
    /// Leyssa. The month is written once when the week does not straddle two.
    /// </summary>
    public static string Range(DateOnly weekStart, string? themeKey)
    {
        var weekEnd = weekStart.AddDays(6);
        var sameMonth = weekStart.Month == weekEnd.Month;

        if (themeKey == TeamTrainers)
        {
            var from = weekStart.ToString("dd", GxFormats.Culture);
            var to = weekEnd.ToString("dd MMMM", GxFormats.Culture);

            return sameMonth ? $"{from} → {to}" : $"{weekStart.ToString("dd MMMM", GxFormats.Culture)} → {to}";
        }

        var start = sameMonth
            ? weekStart.Day.ToString(GxFormats.Culture)
            : GxFormats.DayAndMonth(weekStart);
        var end = GxFormats.DayAndMonth(weekEnd);

        return themeKey == Leyssa ? $"Du {start} au {end}" : $"{start} → {end}";
    }

    /// <summary>« 12 cours cette semaine ».</summary>
    public static string CourseCount(int count, bool withSuffix) =>
        withSuffix
            ? $"{GxFormats.Plural(count, "cours", "cours")} cette semaine"
            : GxFormats.Plural(count, "cours", "cours");

    /// <summary>
    /// Where the gym is, for the foot of the poster and — for Leyssa — for the
    /// headline. The area label wins over the town: a coach who works on the
    /// move has a zone, not an address, and no postal address of hers appears
    /// anywhere in the product.
    /// </summary>
    public static string Place(TenantBrandDto? brand) =>
        brand?.Where is { Length: > 0 } where ? where : brand?.DisplayName ?? string.Empty;

    /// <summary>
    /// The line across the foot. Brand copy rather than a token, and the one
    /// place a new customer needs more than a block of CSS: « Révélez-vous » is
    /// Leyssa's, and the seeded Baseline (« Coach indépendante ») describes her
    /// job rather than calling anybody to anything.
    /// </summary>
    public static string Call(string? themeKey) =>
        themeKey == Leyssa ? "Révélez-vous" : "Réservations ouvertes";

    /// <summary>
    /// The brand's display face — the one thing the poster cannot be published
    /// without. Handed to gx-poster.js, which refuses to produce an image if it
    /// could not embed this family: an affiche in the system sans carries the
    /// customer's name in somebody else's voice, and it looks fine enough on a
    /// thumbnail to be published by mistake.
    /// <para>
    /// The names match the @font-face families in techxyz/tokens/fonts.css.
    /// </para>
    /// </summary>
    public static string DisplayFont(string? themeKey) => themeKey switch
    {
        TeamTrainers => "Anton",
        Leyssa => "Dancing Script",
        _ => "Orbitron"
    };
}
