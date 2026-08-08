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
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<GymSettings> GymSettings => Set<GymSettings>();
    public DbSet<OpeningHours> OpeningHours => Set<OpeningHours>();
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();
    public DbSet<Address> Addresses =>  Set<Address>();

    // Sits above the tenant filter, alongside Tenant: it is read by the TechXYZ
    // console about a customer it does not inhabit.
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // Above it for a different reason: these are filled in before any customer
    // exists. A space request is a stranger asking for one, so there is no tenant to
    // scope it to — not an escape from the partitioning, a place before it. The
    // requests that touch them carry IPlatformScoped and say so.
    public DbSet<SpaceRequest> SpaceRequests => Set<SpaceRequest>();
    public DbSet<SpaceRequestActivity> SpaceRequestActivities => Set<SpaceRequestActivity>();
    public DbSet<SpaceRequestNote> SpaceRequestNotes => Set<SpaceRequestNote>();

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

        modelBuilder.Entity<Plan>(x =>
        {
            // Money, spelled out: the engine defaults would round a price into
            // something the gym never charged.
            x.Property(plan => plan.Price).HasPrecision(9, 2);

            // The formules grid reads every active plan in display order, on
            // every load of the Abonnements screen and of the settings panel.
            x.HasIndex(plan => plan.Rank);
        });

        modelBuilder.Entity<Subscription>(x =>
        {
            x.Property(subscription => subscription.Price).HasPrecision(9, 2);
            x.Property(subscription => subscription.MonthlyPrice).HasPrecision(9, 2);

            x.HasOne(subscription => subscription.Member)
                .WithMany(member => member.Subscriptions)
                .HasForeignKey(subscription => subscription.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict rather than cascade: a plan is retired, and retiring it
            // must not take the history of everybody who bought it.
            x.HasOne(subscription => subscription.Plan)
                .WithMany(plan => plan.Subscriptions)
                .HasForeignKey(subscription => subscription.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // "Which cover is running today", asked once per member row of the
            // members table and once per seat of every attendance sheet.
            x.HasIndex(subscription => new { subscription.MemberId, subscription.EndsOn });

            // The suivi table sorts on the due date across every member at once.
            x.HasIndex(subscription => subscription.EndsOn);
        });

        modelBuilder.Entity<Payment>(x =>
        {
            x.Property(payment => payment.Amount).HasPrecision(9, 2);

            x.HasOne(payment => payment.Member)
                .WithMany(member => member.Payments)
                .HasForeignKey(payment => payment.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            x.HasOne(payment => payment.Subscription)
                .WithMany(subscription => subscription.Payments)
                .HasForeignKey(payment => payment.SubscriptionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // "Encaissements récents" reads the last seven days across everybody;
            // the member's record reads one member's, newest first.
            x.HasIndex(payment => payment.Date);
            x.HasIndex(payment => new { payment.MemberId, payment.Date });

            // Telling an ended subscription from a late one is a lookup for a
            // row that is not Collected, and it runs per subscription.
            x.HasIndex(payment => new { payment.SubscriptionId, payment.Status });
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

        modelBuilder.Entity<Invitation>(x =>
        {
            // Optional: inviting a collaborator names an address the gym holds
            // no member record for.
            x.HasOne(invitation => invitation.Member)
                .WithMany()
                .HasForeignKey(invitation => invitation.MemberId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // "Has this address already been asked" runs before every invite,
            // and the pending list reads the same column.
            x.HasIndex(invitation => invitation.Email);

            // The comptes membres segment asks the question the other way round:
            // which of these members is waiting on an invitation.
            x.HasIndex(invitation => invitation.MemberId);
        });

        modelBuilder.Entity<GymSettings>(x =>
        {
            x.Property(settings => settings.AcceptedPaymentMethods)
                .HasConversion(new PaymentMethodListConverter())
                .Metadata.SetValueComparer(new PaymentMethodListComparer());

            x.Property(settings => settings.Currency).HasMaxLength(3);

            x.HasMany(settings => settings.OpeningHours)
                .WithOne(hours => hours.Settings)
                .HasForeignKey(hours => hours.GymSettingsId);

            // One row per customer. The screen reads it by tenant and nothing
            // else ever looks it up.
            x.HasIndex(settings => settings.TenantId).IsUnique();
        });

        modelBuilder.Entity<NotificationSetting>(x =>
        {
            // A message is configured once per customer: two rows for the same
            // key would make "is this allowed" a question with two answers.
            x.HasIndex(setting => new { setting.TenantId, setting.Key }).IsUnique();
        });

        modelBuilder.Entity<Member>()
            .HasIndex(member => member.UserId);

        // Read on the way in rather than on the way out: resolving the signed-in
        // account to its coach is what decides whose sessions a coach may see, so
        // it happens on screens that are opened all day.
        modelBuilder.Entity<Coach>()
            .HasIndex(coach => coach.UserId);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<Invoice>(x =>
        {
            x.Property(invoice => invoice.Amount).HasPrecision(9, 2);

            x.HasOne(invoice => invoice.Tenant)
                .WithMany(tenant => tenant.Invoices)
                .HasForeignKey(invoice => invoice.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // The billing panel reads one customer's invoices, newest first.
            x.HasIndex(invoice => new { invoice.TenantId, invoice.Date });

            // A reference identifies a document, and issuing it twice would make
            // "which invoice is GX-2026-001" a question with two answers.
            x.HasIndex(invoice => invoice.Reference).IsUnique();
        });

        modelBuilder.Entity<SpaceRequest>(x =>
        {
            // The reference is what the applicant quotes and what the console
            // searches on, so it is unique in the store and not merely in the
            // code that allocates it.
            x.HasIndex(request => request.Reference).IsUnique();

            // The console's default view is "à traiter, newest first".
            x.HasIndex(request => new { request.Status, request.ReceivedOn });

            // What the purge sweeps on: refusals old enough to delete.
            x.HasIndex(request => request.RefusedOn);

            // Cascade, and deliberately so: the purge deletes a refused request
            // three months on because the consent text promised it would, and a
            // promise kept only for the parent row is not kept.
            x.HasMany(request => request.Activities)
                .WithOne(activity => activity.Request!)
                .HasForeignKey(activity => activity.SpaceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(request => request.Notes)
                .WithOne(note => note.Request!)
                .HasForeignKey(note => note.SpaceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Computed from the other two on the way out; nothing to store.
            x.Ignore(request => request.Where);
        });

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
