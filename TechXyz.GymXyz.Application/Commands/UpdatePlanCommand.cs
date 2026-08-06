using MediatR;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Edits a formule on sale.
/// <para>
/// The <c>Kind</c> is not among the fields: turning a monthly plan into a pack
/// would leave every subscription already sold on it consuming entries it never
/// had, and turning a pack into a plan would strand their gauges. A different
/// nature is a different offer.
/// </para>
/// <para>
/// What is edited here changes what the <b>next</b> member pays. The ones
/// already on the plan keep the price and the entry count snapshot on their own
/// subscription — including the monthly figure the MRR is a sum of.
/// </para>
/// </summary>
public sealed class UpdatePlanCommand : IRequest<bool>
{
    public UpdatePlanCommand(
        int id,
        string name,
        string? shortName,
        decimal price,
        int validityMonths,
        int? creditCount,
        string? description,
        bool hasCommitment,
        bool isFeatured)
    {
        Id = id;
        Name = name.Trim();
        ShortName = string.IsNullOrWhiteSpace(shortName) ? Name : shortName.Trim();
        Price = price;
        ValidityMonths = validityMonths;
        CreditCount = creditCount;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        HasCommitment = hasCommitment;
        IsFeatured = isFeatured;
    }

    public int Id { get; }
    public string Name { get; }
    public string ShortName { get; }
    public decimal Price { get; }
    public int ValidityMonths { get; }
    public int? CreditCount { get; }
    public string? Description { get; }
    public bool HasCommitment { get; }
    public bool IsFeatured { get; }
}
