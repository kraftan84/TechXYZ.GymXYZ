namespace TechXyz.GymXyz.Domain.Entities;

public class Member : Person
{
    public Member(string firstName, string lastName)
        : base(firstName, lastName)
    {
    }

    /// <summary>Day the person joined the gym — "membre depuis mars 2024".</summary>
    public DateOnly JoinedOn { get; set; }

    public DateOnly? BirthDate { get; set; }

    /// <summary>Free-form note kept by the staff, shown in the edit drawer.</summary>
    public string? Notes { get; set; }

    public ICollection<Subscription>? Subscriptions { get; set; }
    public ICollection<Registration>? Registrations { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}
