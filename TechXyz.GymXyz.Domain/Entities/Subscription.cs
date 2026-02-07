using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Domain.Entities;

public class Subscription : EntityBase<int>
{
    public Member Member { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public Coach? CashedBy { get; set; }
    public int NumberOfLessons { get; set; }
}