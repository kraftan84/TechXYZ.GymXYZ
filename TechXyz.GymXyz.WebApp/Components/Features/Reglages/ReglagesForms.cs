using TechXyz.GymXyz.Application.Commands;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.WebApp.Components.Features.Reglages;

/// <summary>
/// What the editable panels bind to. Separate from the DTOs the query returns
/// because a panel is edited before it is saved: binding straight onto the read
/// model would leave the screen showing changes that were refused, and « Annuler »
/// with nothing to go back to.
/// </summary>
public sealed class IdentityForm
{
    public string Name { get; set; } = string.Empty;
    public string? Baseline { get; set; }
    public int? Capacity { get; set; }
    public string? Siret { get; set; }
    public string? Street { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? AreaLabel { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public List<OpeningHoursForm> OpeningHours { get; set; } = [];

    /// <summary>
    /// The customer works on the move, so the panel offers a zone instead of a
    /// postal address.
    /// <para>
    /// Settable rather than read off <see cref="AreaLabel"/>: somebody switching
    /// to a zone has not typed one yet, and a flag derived from the text would
    /// close the field on the first character deleted.
    /// </para>
    /// </summary>
    public bool WorksFromAnArea { get; set; }

    public static IdentityForm From(GymIdentityDto identity, IReadOnlyList<OpeningHoursDto> hours) => new()
    {
        Name = identity.Name,
        Baseline = identity.Baseline,
        Capacity = identity.Capacity,
        Siret = identity.Siret,
        Street = identity.Street,
        ZipCode = identity.ZipCode,
        City = identity.City,
        AreaLabel = identity.AreaLabel,
        WorksFromAnArea = identity.WorksFromAnArea,
        Email = identity.Email,
        Phone = identity.Phone,
        OpeningHours = hours.Select(OpeningHoursForm.From).ToList()
    };

    public UpdateGymIdentityCommand ToCommand() => new(
        Name,
        Baseline,
        Capacity,
        Siret,
        Street,
        ZipCode,
        City,
        // Only sent when the switch is on: the handler reads a zone as "this
        // customer has no premises" and clears the address on the strength of it.
        WorksFromAnArea ? AreaLabel : null,
        Email,
        Phone,
        OpeningHours.Select(line => line.ToInput()).ToList());
}

public sealed class OpeningHoursForm
{
    public int Id { get; set; }
    public DayOfWeek DayFrom { get; set; } = DayOfWeek.Monday;
    public DayOfWeek DayTo { get; set; } = DayOfWeek.Friday;
    public TimeOnly OpensAt { get; set; } = new(9, 0);
    public TimeOnly ClosesAt { get; set; } = new(18, 0);

    /// <summary>
    /// <c>FluentTimePicker</c> binds a <see cref="DateTime"/>; the model keeps a
    /// <see cref="TimeOnly"/> because a day is not part of an opening hour. The
    /// date half is arbitrary and never read.
    /// </summary>
    public DateTime? OpensAtValue
    {
        get => DateTime.Today.Add(OpensAt.ToTimeSpan());
        set => OpensAt = value is { } moment ? TimeOnly.FromDateTime(moment) : OpensAt;
    }

    public DateTime? ClosesAtValue
    {
        get => DateTime.Today.Add(ClosesAt.ToTimeSpan());
        set => ClosesAt = value is { } moment ? TimeOnly.FromDateTime(moment) : ClosesAt;
    }

    public static OpeningHoursForm From(OpeningHoursDto hours) => new()
    {
        Id = hours.Id,
        DayFrom = hours.DayFrom,
        DayTo = hours.DayTo,
        OpensAt = hours.OpensAt,
        ClosesAt = hours.ClosesAt
    };

    public OpeningHoursInput ToInput() => new(Id, DayFrom, DayTo, OpensAt, ClosesAt);
}

public sealed class PaymentsForm
{
    public string Currency { get; set; } = GymSettings.DefaultCurrency;
    public string? VatMention { get; set; }
    public HashSet<PaymentMethod> AcceptedMethods { get; set; } = [];

    public static PaymentsForm From(PaymentSettingsDto payments) => new()
    {
        Currency = payments.Currency,
        VatMention = payments.VatMention,
        AcceptedMethods = payments.AcceptedMethods.ToHashSet()
    };

    public bool Accepts(PaymentMethod method) => AcceptedMethods.Contains(method);

    public void Toggle(PaymentMethod method, bool accepted)
    {
        if (accepted)
        {
            AcceptedMethods.Add(method);
        }
        else
        {
            AcceptedMethods.Remove(method);
        }
    }

    public UpdatePaymentMethodsCommand ToCommand() => new(
        Currency,
        VatMention,
        // Saved in display order rather than the order they were ticked, so two
        // gyms with the same methods store the same thing.
        SettingsLabels.PaymentMethods.Where(AcceptedMethods.Contains).ToList());
}

public sealed class NotificationForm
{
    public NotificationGroup Group { get; set; }
    public NotificationKey Key { get; set; }
    public bool IsEnabled { get; set; }
    public bool UsesEmail { get; set; }
    public bool UsesSms { get; set; }

    public static NotificationForm From(NotificationSettingDto setting) => new()
    {
        Group = setting.Group,
        Key = setting.Key,
        IsEnabled = setting.IsEnabled,
        UsesEmail = setting.Uses(NotificationChannels.Email),
        UsesSms = setting.Uses(NotificationChannels.Sms)
    };

    public NotificationChannels Channels =>
        (UsesEmail ? NotificationChannels.Email : NotificationChannels.None)
        | (UsesSms ? NotificationChannels.Sms : NotificationChannels.None);

    public NotificationSettingInput ToInput() => new(Key, IsEnabled, Channels);
}
