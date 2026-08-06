namespace TechXyz.GymXyz.Domain.Entities;

/// <summary>
/// How the money actually arrived. Every one of these happens away from the
/// application — no payment is taken online in this lot, they are recorded after
/// the fact.
/// </summary>
public enum PaymentMethod
{
    Card,
    SepaDirectDebit,
    Cash,
    Cheque,

    /// <summary>
    /// A link sent to the member and paid by them. Nothing issues one yet: it is
    /// here because the settings screen lists the accepted methods and dropping
    /// it would make that list disagree with the model.
    /// </summary>
    PaymentLink
}
