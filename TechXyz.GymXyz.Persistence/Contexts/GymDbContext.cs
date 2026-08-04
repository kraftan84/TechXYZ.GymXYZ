using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Domain.Common.Interfaces;
using TechXyz.GymXyz.Domain.Entities;
using TechXyz.GymXyz.Persistence.Converters;
using TechXyz.GymXyz.Persistence.Identity;

namespace TechXyz.GymXyz.Persistence.Contexts;

public class GymDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IGymDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;

    public GymDbContext(
        DbContextOptions<GymDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<Location> Locations =>  Set<Location>();
    public DbSet<Room> Rooms =>  Set<Room>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<PrivateLesson> PrivateLessons => Set<PrivateLesson>();
    public DbSet<CollectiveLesson> CollectiveLessons => Set<CollectiveLesson>();
    public DbSet<LessonTheme> LessonThemes => Set<LessonTheme>();
    public DbSet<Coach> Coaches =>  Set<Coach>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<CoachDiscipline> CoachDisciplines => Set<CoachDiscipline>();
    public DbSet<CoachCertification> CoachCertifications => Set<CoachCertification>();
    public DbSet<CourseTemplate> CourseTemplates => Set<CourseTemplate>();
    public DbSet<CourseTemplateCoach> CourseTemplateCoaches => Set<CourseTemplateCoach>();
    public DbSet<Member> Members =>  Set<Member>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
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
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<CoachDiscipline>(x =>
        {
            x.HasOne(cd => cd.Coach)
                .WithMany(coach => coach.Disciplines)
                .HasForeignKey(cd => cd.CoachId);

            x.HasOne(cd => cd.Discipline)
                .WithMany(discipline => discipline.Coaches)
                .HasForeignKey(cd => cd.DisciplineId);

            x.HasIndex(cd => new { cd.CoachId, cd.DisciplineId }).IsUnique();
        });

        modelBuilder.Entity<CourseTemplate>()
            .HasOne(template => template.DefaultRoom)
            .WithMany()
            .HasForeignKey(template => template.DefaultRoomId);

        modelBuilder.Entity<CourseTemplateCoach>(x =>
        {
            x.HasOne(link => link.CourseTemplate)
                .WithMany(template => template.Coaches)
                .HasForeignKey(link => link.CourseTemplateId);

            x.HasOne(link => link.Coach)
                .WithMany()
                .HasForeignKey(link => link.CoachId);

            x.HasIndex(link => new { link.CourseTemplateId, link.CoachId }).IsUnique();
        });

        modelBuilder.Entity<CoachCertification>()
            .HasOne(certification => certification.Coach)
            .WithMany(coach => coach.Certifications)
            .HasForeignKey(certification => certification.CoachId);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.TenantId);

        ApplyTenantFilters(modelBuilder);
    }

    /// <summary>
    /// Isolates customers from one another at the engine level: forgetting the
    /// filter in a query would leak another tenant's rows, so it is not left to
    /// each handler. Soft delete stays explicit per query, as the repository
    /// conventions require.
    /// </summary>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        var apply = typeof(GymDbContext).GetMethod(
            nameof(ApplyTenantFilter),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // A filter belongs on the root of an inheritance hierarchy only.
            if (entityType.BaseType is not null)
                continue;

            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            apply.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        // Reading CurrentTenantId through the context instance is deliberate:
        // EF Core rebinds it to the context executing the query, so the cached
        // model still resolves the ambient tenant of each request.
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    /// <summary>
    /// Read through a property rather than capturing the value: the model is
    /// built once and cached, the ambient tenant changes with every request.
    /// </summary>
    private int CurrentTenantId => _tenantContext.Current;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        // Stamp audit fields for tracked entities on save.
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: AuditableEntityBase, State: EntityState.Added or EntityState.Modified });

        var username = _currentUserService.UserName ?? "system";
        var now = DateTime.UtcNow;
        foreach (var entityEntry in entries)
        {
            ((AuditableEntityBase)entityEntry.Entity).UpdatedBy = username;
            ((AuditableEntityBase)entityEntry.Entity).UpdatedOn = now;

            if (entityEntry.State == EntityState.Added)
            {
                ((AuditableEntityBase)entityEntry.Entity).CreatedBy = username;
                ((AuditableEntityBase)entityEntry.Entity).CreatedOn = now;
            }
        }

        StampTenant();

        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Every write carries the ambient tenant. An explicit TenantId is left
    /// untouched so the initializer can seed several customers in one pass.
    /// </summary>
    private void StampTenant()
    {
        var tenantId = _tenantContext.Current;
        if (tenantId == 0)
            return;

        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == 0)
                entry.Entity.TenantId = tenantId;
        }
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }
}
