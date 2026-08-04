using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// One line of the "Certifications" list — "BPJEPS AF — Cours collectifs".
/// Free text: the diplomas of the trade have no closed list worth maintaining.
/// </summary>
public class CoachCertification : EntityBase<int>, ITenantScoped
{
    public CoachCertification(string label)
    {
        Label = label;
    }

    public int TenantId { get; set; }

    public int CoachId { get; set; }
    public Coach? Coach { get; set; }

    public string Label { get; set; }

    public int Rank { get; set; }
}
