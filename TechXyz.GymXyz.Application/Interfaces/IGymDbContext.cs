using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Interfaces;

public interface IGymDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<Site> Sites { get; }
    DbSet<Location> Locations { get; }
    DbSet<LocationEquipment> LocationEquipment { get; }
    DbSet<Session> Sessions { get; }
    DbSet<Registration> Registrations { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<Discipline> Disciplines { get; }
    DbSet<CoachDiscipline> CoachDisciplines { get; }
    DbSet<CoachCertification> CoachCertifications { get; }
    DbSet<CourseTemplate> CourseTemplates { get; }
    DbSet<CourseTemplateCoach> CourseTemplateCoaches { get; }
    DbSet<Member> Members { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<GymSettings> GymSettings { get; }
    DbSet<OpeningHours> OpeningHours { get; }
    DbSet<NotificationSetting> NotificationSettings { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<TenantImpersonation> TenantImpersonations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
