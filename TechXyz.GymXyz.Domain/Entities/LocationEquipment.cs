using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One chip of the "Équipement" list — "Tapis ×20", "Sono immersive". Free text
/// and ordered, exactly like <see cref="CoachCertification"/>: what a room holds
/// has no closed list worth maintaining.
/// </summary>
public class LocationEquipment : EntityBase<int>, ITenantScoped
{
    public LocationEquipment(string label)
    {
        Label = label;
    }

    public int TenantId { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public string Label { get; set; }

    public int Rank { get; set; }
}
