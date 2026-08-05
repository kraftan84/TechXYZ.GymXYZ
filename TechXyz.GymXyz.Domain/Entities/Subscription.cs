using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Domain.Entities;

public class Subscription : EntityBase<int>, ITenantScoped
{
    public int TenantId { get; set; }
    public Member Member { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public Coach? CashedBy { get; set; }
    public int NumberOfSessions { get; set; }
}