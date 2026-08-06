using MediatR;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Puts a new formule on sale. Nothing is sold by creating one — a plan is an
/// offer, and it stays an offer until <c>AssignSubscriptionCommand</c> gives a
/// member their copy of it.
/// </summary>
public sealed class CreatePlanCommand : IRequest<int>
{
    public CreatePlanCommand(
        string name,
        string? shortName,
        decimal price,
        PlanKind kind,
        int validityMonths,
        int? creditCount,
        string? description,
        bool hasCommitment)
    {
        Name = name.Trim();
        // The narrow columns want a short form; falling back to the full name is
        // better than an empty cell on every attendance sheet.
        ShortName = string.IsNullOrWhiteSpace(shortName) ? Name : shortName.Trim();
        Price = price;
        Kind = kind;
        ValidityMonths = validityMonths;
        CreditCount = kind == PlanKind.CreditPack ? creditCount : null;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        HasCommitment = hasCommitment;
    }

    public string Name { get; }
    public string ShortName { get; }
    public decimal Price { get; }
    public PlanKind Kind { get; }
    public int ValidityMonths { get; }
    public int? CreditCount { get; }
    public string? Description { get; }

    /// <summary>
    /// Drives the wording on the card — "Engagement 12 mois" against "Sans
    /// engagement", or "Paiement unique" for a pack, which is neither.
    /// </summary>
    public bool HasCommitment { get; }
}
