using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Interfaces;

public interface IGymDbContext
{
    DbSet<Gym> Gyms { get; }
    DbSet<Location> Locations { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<Member> Members { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
