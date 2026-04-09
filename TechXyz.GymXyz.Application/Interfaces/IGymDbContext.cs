using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Interfaces;

public interface IGymDbContext
{
    DbSet<Gym> Gyms { get; }
    DbSet<Location> Locations { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<PrivateLesson> PrivateLessons { get; }
    DbSet<CollectiveLesson> CollectiveLessons { get; }
    DbSet<LessonTheme> LessonThemes { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<Member> Members { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
