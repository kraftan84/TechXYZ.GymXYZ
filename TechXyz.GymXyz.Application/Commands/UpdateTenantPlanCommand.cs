using MediatR;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Saves what a customer pays TechXYZ: the formula, its price, when it renews
/// and how many members it covers.
/// <para>
/// A null <c>PlanMemberCap</c> is unlimited, which the panel reads as
/// « 112 / illimité » and draws with no gauge.
/// </para>
/// <para>
/// The payment method is <b>not</b> here. What the panel shows — a brand and
/// four digits — is what a payment provider hands back, not what somebody types,
/// and there is no provider. Editing it would mean inventing a card number the
/// product must never hold.
/// </para>
/// <para>
/// <see cref="IPlatformScoped"/>: what a customer pays TechXYZ is the
/// platform's own row, written by somebody who inhabits no customer.
/// </para>
/// </summary>
public sealed record UpdateTenantPlanCommand(
    int TenantId,
    string? GymPlan,
    string? PlanDescription,
    decimal? PlanPrice,
    DateOnly? PlanRenewalDate,
    int? PlanMemberCap) : IRequest<bool>, IPlatformScoped;
