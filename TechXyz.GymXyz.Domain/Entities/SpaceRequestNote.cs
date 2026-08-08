using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// What GymXYZ writes to itself about a request. Never shown to the applicant.
/// <para>
/// Modelled now and written by nothing yet: the console owns the screen that adds
/// them. It is here so the purge deletes them with their request rather than
/// leaving orphaned notes about somebody who was refused three months ago.
/// </para>
/// </summary>
public class SpaceRequestNote : EntityBase<int>
{
    public SpaceRequestNote(string text)
    {
        Text = text;
    }

    public int SpaceRequestId { get; set; }

    public SpaceRequest? Request { get; set; }

    public string Author { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; }

    public string Text { get; set; }
}
