using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Saves the money half of « Formules &amp; tarifs »: the currency, the tax
/// mention and which methods the gym takes.
/// <para>
/// The formules themselves are not edited here — the panel links across to
/// Abonnements for that, and <c>UpdatePlanCommand</c> owns it. What a formule
/// costs and what a gym can be paid in are two decisions with two audiences.
/// </para>
/// </summary>
public sealed class UpdatePaymentMethodsCommand : IRequest<bool>, IManagerOnly
{
    public UpdatePaymentMethodsCommand(
        string currency,
        string? vatMention,
        IReadOnlyList<PaymentMethod> acceptedMethods)
    {
        Currency = currency.Trim().ToUpperInvariant();
        VatMention = string.IsNullOrWhiteSpace(vatMention) ? null : vatMention.Trim();
        AcceptedMethods = acceptedMethods.Distinct().ToList();
    }

    public string Currency { get; }

    public string? VatMention { get; }

    public IReadOnlyList<PaymentMethod> AcceptedMethods { get; }
}
