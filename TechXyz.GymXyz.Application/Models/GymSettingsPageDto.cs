using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// Everything the four Réglages panels draw, in one read.
/// <para>
/// One query rather than four because the sections are tabs of a single screen:
/// switching from Identité to Notifications must not go back to the server, and
/// a panel that loaded on its own would show figures counted at a different
/// moment from the ones beside it.
/// </para>
/// <para>
/// The « calendrier scolaire » card is <b>not</b> here. Its data comes from
/// <c>ISchoolCalendarService</c> over the network, and folding a call that can
/// be slow or down into the query every panel waits on would make the whole
/// screen hostage to an open data source. The page fetches it separately, as the
/// planning does.
/// </para>
/// </summary>
public sealed record GymSettingsPageDto(
    GymIdentityDto Identity,
    IReadOnlyList<OpeningHoursDto> OpeningHours,
    TeamAccessDto Team,
    PaymentSettingsDto Payments,
    IReadOnlyList<NotificationSettingDto> Notifications)
{
    public static GymSettingsPageDto Empty { get; } = new(
        GymIdentityDto.Empty,
        [],
        TeamAccessDto.Empty,
        PaymentSettingsDto.Empty,
        []);

    public IEnumerable<NotificationSettingDto> InGroup(NotificationGroup group) =>
        Notifications.Where(setting => setting.Group == group);
}

/// <summary>
/// The gym as it presents itself. Mostly <see cref="Tenant"/> fields: the
/// identity panel edits the customer record, not a settings row.
/// </summary>
public sealed record GymIdentityDto(
    string Name,
    string? Baseline,
    int? Capacity,
    string? Siret,
    string? Street,
    string? ZipCode,
    string? City,
    string? AreaLabel,
    string? Email,
    string? Phone,
    bool ShowSchoolVacations)
{
    public static GymIdentityDto Empty { get; } =
        new(string.Empty, null, null, null, null, null, null, null, null, null, true);

    /// <summary>
    /// True for a customer who works on the move — Leyssa Coaching, around
    /// Thonon. The panel then shows the zone <b>instead of</b> the postal
    /// address, because there is no address to show and an empty one would read
    /// as missing data rather than as a deliberate way of working.
    /// </summary>
    public bool WorksFromAnArea => !string.IsNullOrWhiteSpace(AreaLabel);
}

public sealed record OpeningHoursDto(
    int Id,
    DayOfWeek DayFrom,
    DayOfWeek DayTo,
    TimeOnly OpensAt,
    TimeOnly ClosesAt);

/// <summary>
/// Both halves of « Équipe & accès »: who works here and signs in, and how far
/// the members have got with their own espace.
/// </summary>
public sealed record TeamAccessDto(
    IReadOnlyList<TeamMemberDto> Team,
    IReadOnlyList<PendingInvitationDto> Invitations,
    MemberAccessKpisDto MemberAccess,
    IReadOnlyList<MemberAccountDto> MemberAccounts)
{
    public static TeamAccessDto Empty { get; } = new([], [], MemberAccessKpisDto.Empty, []);
}

/// <summary>
/// One row of « Équipe de gestion ». <paramref name="AccessScope"/> is derived
/// from the role by <c>TeamAccessScopes</c>, never stored.
/// </summary>
public sealed record TeamMemberDto(
    string UserId,
    string DisplayName,
    string Email,
    string? RoleLabel,
    string Role,
    string AccessScope,
    DateTime? LastSeenAt,
    bool IsRevoked,
    bool IsCurrentUser);

public sealed record PendingInvitationDto(
    int Id,
    string Email,
    string RoleName,
    DateTime SentOn,
    int? MemberId);

/// <summary>
/// The three tiles above « Accès à l'espace membre ». Counted off the members
/// themselves so the tiles and the list below cannot disagree.
/// </summary>
public sealed record MemberAccessKpisDto(int Total, int WithAccount, int Invited)
{
    public static MemberAccessKpisDto Empty { get; } = new(0, 0, 0);

    /// <summary>Never negative, whatever the data says.</summary>
    public int WithoutAccess => Math.Max(0, Total - WithAccount - Invited);
}

public sealed record MemberAccountDto(
    int MemberId,
    string FirstName,
    string LastName,
    string? Email,
    string? PlanName,
    MemberAccessState State,
    DateTime? LastSeenAt)
{
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>Where a member has got to with their espace.</summary>
public enum MemberAccessState
{
    /// <summary>Nobody has asked them.</summary>
    None,

    /// <summary>Asked, not yet taken up.</summary>
    Invited,

    /// <summary>Signs in.</summary>
    Active,

    /// <summary>Had an account, and it has been closed.</summary>
    Revoked
}

/// <summary>
/// Money and tax. The formules the panel lists above them are not here: they
/// come from <c>GetPlansQuery</c>, which was written parameterless so this panel
/// and the Abonnements screen could ask for the same list and get the same
/// answer.
/// </summary>
public sealed record PaymentSettingsDto(
    string Currency,
    string? VatMention,
    IReadOnlyList<PaymentMethod> AcceptedMethods)
{
    public static PaymentSettingsDto Empty { get; } = new(GymSettings.DefaultCurrency, null, []);

    public bool Accepts(PaymentMethod method) => AcceptedMethods.Contains(method);
}

public sealed record NotificationSettingDto(
    int Id,
    NotificationGroup Group,
    NotificationKey Key,
    bool IsEnabled,
    NotificationChannels Channels)
{
    public bool Uses(NotificationChannels channel) =>
        channel != NotificationChannels.None && Channels.HasFlag(channel);
}
