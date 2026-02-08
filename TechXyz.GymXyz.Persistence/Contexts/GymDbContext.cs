using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Converters;

namespace TechXyz.GymXyz.Persistence.Contexts;

public class GymDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    
    public GymDbContext(DbContextOptions<GymDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<Location> Locations =>  Set<Location>();
    public DbSet<Room> Rooms =>  Set<Room>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<PrivateLesson> PrivateLessons => Set<PrivateLesson>();
    public DbSet<CollectiveLesson> CollectiveLessons => Set<CollectiveLesson>();
    public DbSet<Coach> Coaches =>  Set<Coach>();
    public DbSet<Member> Members =>  Set<Member>();
    public DbSet<Address> Addresses =>  Set<Address>();
    
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<Enum>()
            .HaveConversion<string>();

        builder.Properties<DateOnly>()
            .HaveConversion<Converters.Converters>();

        builder.Properties<TimeOnly>()
            .HaveConversion<TimeOnlyConverter>();

        base.ConfigureConventions(builder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Lesson>(x =>
            {
                x.UseTpcMappingStrategy();

                x.HasOne(l => l.Coach)
                    .WithMany();
            })
            .Entity<PrivateLesson>(x =>
            {
                x.HasOne(pl => pl.Coach)
                    .WithMany(c => c.PrivateLessons);
                x.HasOne(pl => pl.Room)
                    .WithMany();

                x.HasOne(pl => pl.Member)
                    .WithMany(m => m.PrivateLessons);
            })
            .Entity<CollectiveLesson>(x =>
            {
                x.HasOne(pl => pl.Coach)
                    .WithMany(c => c.CollectiveLessons);
                x.HasMany(cl => cl.Rooms)
                    .WithMany();

                x.HasMany(cl => cl.Participants)
                    .WithMany(m => m.CollectiveLessons);
            });

        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        // Stamp audit fields for tracked entities on save.
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: AuditableEntityBase, State: EntityState.Added or EntityState.Modified });

        var username = _currentUserService.UserName;
        foreach (var entityEntry in entries)
        {
            ((AuditableEntityBase)entityEntry.Entity).UpdatedBy = username;
            ((AuditableEntityBase)entityEntry.Entity).UpdatedOn = DateTime.Now;

            if (entityEntry.State == EntityState.Added)
            {
                ((AuditableEntityBase)entityEntry.Entity).CreatedBy = username;
                ((AuditableEntityBase)entityEntry.Entity).CreatedOn = DateTime.Now;
            }
        }
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }
}
