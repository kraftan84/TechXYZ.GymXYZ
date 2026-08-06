using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Models;

/// <summary>
/// One line of the customer's GymXYZ invoice history. Nothing generates a
/// document yet, so a row is what TechXYZ recorded and nothing more.
/// </summary>
public sealed record InvoiceDto(
    int Id,
    string Reference,
    DateOnly Date,
    decimal Amount,
    InvoiceStatus Status);
