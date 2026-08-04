namespace TechXyz.GymXyz.WebApp.Components.Features.Members;

/// <summary>
/// What the create / edit drawer binds to. Dates are <c>DateTime?</c> because
/// that is what the Fluent date picker speaks; the commands take
/// <see cref="DateOnly"/>.
/// </summary>
public sealed class MemberEditModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? JoinedOn { get; set; }
    public string? Notes { get; set; }

    public string? Street { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public DateOnly? BirthDateOnly => BirthDate is { } value ? DateOnly.FromDateTime(value) : null;

    public DateOnly? JoinedOnOnly => JoinedOn is { } value ? DateOnly.FromDateTime(value) : null;
}
