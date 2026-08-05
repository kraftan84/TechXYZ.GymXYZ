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
    public DbSet<Site> Sites =>  Set<Site>();
    public DbSet<Location> Locations =>  Set<Location>();
    public DbSet<LocationEquipment> LocationEquipment => Set<LocationEquipment>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Registration> Registrations => Set<Registration>();
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

        modelBuilder.Entity<Session>(x =>
        {
            x.HasOne(session => session.CourseTemplate)
                .WithMany()
                .HasForeignKey(session => session.CourseTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: an open-access slot runs without anybody animating it.
            x.HasOne(session => session.Coach)
                .WithMany(coach => coach.Sessions)
                .HasForeignKey(session => session.CoachId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            x.HasOne(session => session.Location)
                .WithMany()
                .HasForeignKey(session => session.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // The planning always reads a week of one venue, of one coach, or of
            // a whole series at once — those three are the indexes.
            x.HasIndex(session => session.StartsAt);
            x.HasIndex(session => new { session.LocationId, session.StartsAt });
            x.HasIndex(session => new { session.CoachId, session.StartsAt });
            x.HasIndex(session => session.SeriesId);

            // "Combien de feuilles à pointer" asks for the sessions whose sheet
            // is still open, on every load of the Présences screen.
            x.HasIndex(session => session.AttendanceClosedAt);
        });

        modelBuilder.Entity<Registration>(x =>
        {
            x.HasOne(registration => registration.Session)
                .WithMany(session => session.Registrations)
                .HasForeignKey(registration => registration.SessionId);

            x.HasOne(registration => registration.Member)
                .WithMany(member => member.Registrations)
                .HasForeignKey(registration => registration.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // One seat per member per session, waiting list included: signing up
            // twice is the same seat, not two.
            x.HasIndex(registration => new { registration.SessionId, registration.MemberId })
                .IsUnique();

            // Tallying a sheet — how many present, late, absent — is the one
            // question the roster asks, and it asks it per session.
            x.HasIndex(registration => new { registration.SessionId, registration.Status });
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
            .HasOne(template => template.DefaultLocation)
            .WithMany()
            .HasForeignKey(template => template.DefaultLocationId);

        modelBuilder.Entity<Location>(x =>
        {
            // Optional on purpose: the park and the member's home sit in no
            // building of the gym's.
            x.HasOne(location => location.Site)
                .WithMany(site => site.Locations)
                .HasForeignKey(location => location.SiteId)
                .IsRequired(false);

            // A venue points at the indoor one it falls back to. Restrict rather
            // than cascade: losing the fallback must not take the outdoor venue
            // with it.
            x.HasOne(location => location.FallbackLocation)
                .WithMany()
                .HasForeignKey(location => location.FallbackLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Spelled out because the engine defaults would round coordinates
            // into uselessness.
            x.Property(location => location.Latitude).HasPrecision(9, 6);
            x.Property(location => location.Longitude).HasPrecision(9, 6);
            x.Property(location => location.AreaSqm).HasPrecision(7, 2);
        });

        modelBuilder.Entity<LocationEquipment>()
            .HasOne(equipment => equipment.Location)
            .WithMany(location => location.Equipment)
            .HasForeignKey(equipment => equipment.LocationId);

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
