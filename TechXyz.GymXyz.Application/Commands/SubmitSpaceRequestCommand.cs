using MediatR;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

/// <summary>
/// Files a request for a space. Public, anonymous, and the only write in the
/// product a stranger can reach.
/// <para>
/// <see cref="IPlatformScoped"/>: it creates a row belonging to no customer,
/// because the customer is precisely what is being asked for. It runs with no
/// ambient tenant and must not reach for one.
/// </para>
/// </summary>
public sealed class SubmitSpaceRequestCommand : IRequest<SpaceRequestReceiptDto>, IPlatformScoped
{
    public required SpaceRequestType Type { get; init; }

    public required string Name { get; init; }

    public string? Siret { get; init; }

    public string? SizeLabel { get; init; }

    public string? Disciplines { get; init; }

    public string? Street { get; init; }

    public string? ZipCode { get; init; }

    public string? City { get; init; }

    public string? AreaLabel { get; init; }

    public required string ContactFirstName { get; init; }

    public required string ContactLastName { get; init; }

    public string? ContactRole { get; init; }

    public required string ContactEmail { get; init; }

    public string? ContactPhone { get; init; }

    public required string RequestedPlan { get; init; }

    public string? AccentHex { get; init; }

    public string? AccentLabel { get; init; }

    public required string RequestedSubdomain { get; init; }

    public string? Message { get; init; }

    public bool AcceptedTerms { get; init; }

    public bool AcceptedDataProcessing { get; init; }

    public bool OptedIntoNewsletter { get; init; }

    /// <summary>
    /// The honeypot. A field no human sees and no human fills; a bot walking the
    /// DOM fills everything it finds. Anything here and the request is dropped —
    /// <b>with a normal-looking confirmation</b>, because telling a bot it was
    /// detected is telling whoever wrote it which field to leave alone next time.
    /// </summary>
    public string? Website { get; init; }
}
