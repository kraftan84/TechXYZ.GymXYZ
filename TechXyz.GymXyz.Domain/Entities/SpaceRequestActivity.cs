using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One line of a request's history — filed, acknowledged, taken up, refused.
/// <para>
/// Written from the moment the form is submitted rather than left to the console,
/// because the two lines that matter most happen now: the request arriving and
/// the acknowledgement leaving. Reconstructing them later from timestamps would
/// be guesswork.
/// </para>
/// </summary>
public class SpaceRequestActivity : EntityBase<int>
{
    public SpaceRequestActivity(string title)
    {
        Title = title;
    }

    public int SpaceRequestId { get; set; }

    public SpaceRequest? Request { get; set; }

    public string Title { get; set; }

    public string? Detail { get; set; }

    public DateTime OccurredOn { get; set; }

    /// <summary>"done" or "now" — the timeline's own two states.</summary>
    public string State { get; set; } = "done";
}
