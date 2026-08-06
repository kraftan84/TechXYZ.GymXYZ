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

    /// <summary>
    /// Identity account of the member's espace, once they have opened one. Kept
    /// as an id rather than a navigation for the same reason as
    /// <see cref="Coach.UserId"/>: the account lives in Persistence and the
    /// Domain must not see it.
    /// <para>
    /// The link is stored rather than resolved from <see cref="Person.Email"/>,
    /// which is nullable and not unique across customers — matching on it would
    /// quietly hand one person's account to another.
    /// </para>
    /// </summary>
    public string? UserId { get; set; }

    public ICollection<Subscription>? Subscriptions { get; set; }
    public ICollection<Registration>? Registrations { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}
