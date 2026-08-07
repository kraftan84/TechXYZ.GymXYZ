using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetGymSettingsQueryHandler : IRequestHandler<GetGymSettingsQuery, GymSettingsPageDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IUserDirectory _userDirectory;
    private readonly ICurrentUserService _currentUser;

    public GetGymSettingsQueryHandler(
        IGymDbContext dbContext,
        ITenantContext tenantContext,
        IUserDirectory userDirectory,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _userDirectory = userDirectory;
        _currentUser = currentUser;
    }

    public async Task<GymSettingsPageDto> Handle(
        GetGymSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == _tenantContext.Current && candidate.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            return GymSettingsPageDto.Empty;
        }

        var settings = await _dbContext.GymSettings
            .AsNoTracking()
            .Include(candidate => candidate.OpeningHours!.Where(hours => hours.IsActive))
            .FirstOrDefaultAsync(candidate => candidate.IsActive, cancellationToken);

        return new GymSettingsPageDto(
            Identity(tenant),
            OpeningHours(settings),
            await TeamAsync(cancellationToken),
            Payments(settings),
            await NotificationsAsync(cancellationToken));
    }

    private static GymIdentityDto Identity(Tenant tenant) => new(
        tenant.DisplayName,
        tenant.Baseline,
        tenant.Capacity,
        tenant.Siret,
        tenant.Street,
        tenant.ZipCode,
        tenant.City,
        tenant.AreaLabel,
        tenant.Email,
        tenant.Phone,
        tenant.ShowSchoolVacations);

    private static IReadOnlyList<OpeningHoursDto> OpeningHours(GymSettings? settings) =>
        settings?.OpeningHours?
            .OrderBy(hours => hours.Rank)
            .ThenBy(hours => hours.Id)
            .Select(hours => new OpeningHoursDto(
                hours.Id,
                hours.DayFrom,
                hours.DayTo,
                hours.OpensAt,
                hours.ClosesAt))
            .ToList()
        ?? [];

    /// <summary>
    /// A customer with no settings row yet reads as the defaults rather than as
    /// nothing. The row is written the first time the gym saves — a query does
    /// not write.
    /// </summary>
    private static PaymentSettingsDto Payments(GymSettings? settings) =>
        settings is null
            ? PaymentSettingsDto.Empty
            : new PaymentSettingsDto(
                settings.Currency,
                settings.VatMention,
                settings.AcceptedPaymentMethods);

    private async Task<TeamAccessDto> TeamAsync(CancellationToken cancellationToken)
    {
        var accounts = await _userDirectory.GetTenantUsersAsync(cancellationToken);

        var team = accounts
            .Where(account => account.Role != GymRoleNames.Member)
            .OrderByDescending(account => account.Role == GymRoleNames.GymManager)
            .ThenBy(account => account.DisplayName ?? account.Email)
            .Select(account => new TeamMemberDto(
                account.UserId,
                account.DisplayName ?? account.Email,
                account.Email,
                account.RoleLabel,
                account.Role,
                TeamAccessScopes.Label(account.Role),
                account.LastSeenAt,
                account.IsRevoked,
                IsCurrentUser(account.Email)))
            .ToList();

        var invitations = await _dbContext.Invitations
            .AsNoTracking()
            .Where(invitation => invitation.IsActive && invitation.AcceptedOn == null)
            .OrderByDescending(invitation => invitation.SentOn)
            .Select(invitation => new PendingInvitationDto(
                invitation.Id,
                invitation.Email,
                invitation.RoleName,
                invitation.SentOn,
                invitation.MemberId))
            .ToListAsync(cancellationToken);

        var (kpis, memberAccounts) = await MemberAccessAsync(accounts, invitations, cancellationToken);

        return new TeamAccessDto(
            team,
            // The gestion segment lists the invitations that are not a member's.
            invitations.Where(invitation => invitation.MemberId is null).ToList(),
            kpis,
            memberAccounts);
    }

    private async Task<(MemberAccessKpisDto Kpis, IReadOnlyList<MemberAccountDto> Accounts)> MemberAccessAsync(
        IReadOnlyList<DirectoryUserDto> accounts,
        IReadOnlyList<PendingInvitationDto> invitations,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var members = await _dbContext.Members
            .AsNoTracking()
            .Where(member => member.IsActive)
            .OrderBy(member => member.LastName)
            .ThenBy(member => member.FirstName)
            .Select(member => new
            {
                member.Id,
                member.FirstName,
                member.LastName,
                member.Email,
                member.UserId,
                // What they are covered by today, for the "Illimité mensuel"
                // beside their address. The newest cover wins when two overlap.
                PlanName = member.Subscriptions!
                    .Where(subscription =>
                        subscription.IsActive &&
                        subscription.StartedOn <= today &&
                        subscription.EndsOn >= today)
                    .OrderByDescending(subscription => subscription.StartedOn)
                    .Select(subscription => subscription.Plan!.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var accountsById = accounts.ToDictionary(account => account.UserId);
        var invitedMemberIds = invitations
            .Where(invitation => invitation.MemberId is not null)
            .Select(invitation => invitation.MemberId!.Value)
            .ToHashSet();

        var rows = members
            .Select(member =>
            {
                var account = member.UserId is not null && accountsById.TryGetValue(member.UserId, out var found)
                    ? found
                    : null;

                var state = account switch
                {
                    { IsRevoked: true } => MemberAccessState.Revoked,
                    not null => MemberAccessState.Active,
                    _ when invitedMemberIds.Contains(member.Id) => MemberAccessState.Invited,
                    _ => MemberAccessState.None
                };

                return new MemberAccountDto(
                    member.Id,
                    member.FirstName,
                    member.LastName,
                    member.Email,
                    member.PlanName,
                    state,
                    account?.LastSeenAt);
            })
            .ToList();

        var kpis = new MemberAccessKpisDto(
            rows.Count,
            rows.Count(row => row.State == MemberAccessState.Active),
            rows.Count(row => row.State == MemberAccessState.Invited));

        return (kpis, rows);
    }

    /// <summary>
    /// The stored switches, topped up with the defaults for any message this
    /// customer has no row for. A missing row is not a decision — reading it as
    /// "off" would quietly stop a message the gym never turned off.
    /// </summary>
    private async Task<IReadOnlyList<NotificationSettingDto>> NotificationsAsync(CancellationToken cancellationToken)
    {
        var stored = await _dbContext.NotificationSettings
            .AsNoTracking()
            .Where(setting => setting.IsActive)
            .ToListAsync(cancellationToken);

        var settings = stored
            .Select(setting => new NotificationSettingDto(
                setting.Id,
                setting.Group,
                setting.Key,
                setting.IsEnabled,
                setting.Channels))
            .Concat(NotificationDefaults
                .Missing(stored.Select(setting => setting.Key))
                .Select(key =>
                {
                    var fallback = NotificationDefaults.Create(key);
                    return new NotificationSettingDto(
                        0,
                        fallback.Group,
                        fallback.Key,
                        fallback.IsEnabled,
                        fallback.Channels);
                }));

        return settings
            .OrderBy(setting => NotificationDefaults.RankOf(setting.Key))
            .ToList();
    }

    private bool IsCurrentUser(string email) =>
        string.Equals(email, _currentUser.UserName, StringComparison.OrdinalIgnoreCase);
}
