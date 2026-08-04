using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Interfaces;

public interface IGymDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<Location> Locations { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<PrivateLesson> PrivateLessons { get; }
    DbSet<CollectiveLesson> CollectiveLessons { get; }
    DbSet<LessonTheme> LessonThemes { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<Discipline> Disciplines { get; }
    DbSet<CoachDiscipline> CoachDisciplines { get; }
    DbSet<CoachCertification> CoachCertifications { get; }
    DbSet<CourseTemplate> CourseTemplates { get; }
    DbSet<CourseTemplateCoach> CourseTemplateCoaches { get; }
    DbSet<Member> Members { get; }
    DbSet<Subscription> Subscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
