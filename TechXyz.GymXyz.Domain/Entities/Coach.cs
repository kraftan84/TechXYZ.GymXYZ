namespace TechXyz.GymXyz.Domain.Entities;

public class Coach : Person
{
    public Coach(string firstName, string lastName)
        : base(firstName, lastName)
    {
    }

    /// <summary>Role as the card writes it — "Coach senior · co-fondatrice".</summary>
    public string? RoleLabel { get; set; }

    public string? Bio { get; set; }

    /// <summary>Day the coach joined the team — "coach depuis janv. 2022".</summary>
    public DateOnly JoinedOn { get; set; }

    /// <summary>
    /// Last day of the current leave. Null means the coach is around; a date in
    /// the past is left alone and simply stops meaning anything.
    /// </summary>
    public DateOnly? AwayUntil { get; set; }

    /// <summary>
    /// Identity account key, when the coach has one. Optional by design: a coach
    /// exists in the roster without ever signing in. No navigation — Domain does
    /// not know about Identity.
    /// </summary>
    public string? UserId { get; set; }

    // Weekly availability, Monday to Sunday. Seven columns rather than a flags
    // enum: the context converts every enum to a string, which no engine can
    // filter or index on.
    public bool AvailableOnMonday { get; set; }
    public bool AvailableOnTuesday { get; set; }
    public bool AvailableOnWednesday { get; set; }
    public bool AvailableOnThursday { get; set; }
    public bool AvailableOnFriday { get; set; }
    public bool AvailableOnSaturday { get; set; }
    public bool AvailableOnSunday { get; set; }

    public ICollection<CoachDiscipline>? Disciplines { get; set; }
    public ICollection<CoachCertification>? Certifications { get; set; }
    public ICollection<PrivateLesson>? PrivateLessons { get; set; }
    public ICollection<CollectiveLesson>? CollectiveLessons { get; set; }

    public void AddDiscipline(Discipline discipline, int rank)
    {
        Disciplines ??= new List<CoachDiscipline>();
        Disciplines.Add(new CoachDiscipline { Discipline = discipline, Rank = rank });
    }

    public void AddCertification(string label, int rank)
    {
        Certifications ??= new List<CoachCertification>();
        Certifications.Add(new CoachCertification(label) { Rank = rank });
    }
}
