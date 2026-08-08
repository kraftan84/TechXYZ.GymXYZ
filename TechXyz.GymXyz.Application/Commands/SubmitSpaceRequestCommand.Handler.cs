using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SubmitSpaceRequestCommandHandler
    : IRequestHandler<SubmitSpaceRequestCommand, SpaceRequestReceiptDto>
{
    /// <summary>Source recorded for anything arriving through this form.</summary>
    private const string OnlineForm = "Formulaire en ligne";

    private readonly IGymDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<SubmitSpaceRequestCommand> _validator;

    public SubmitSpaceRequestCommandHandler(
        IGymDbContext dbContext,
        IEmailSender emailSender,
        IValidator<SubmitSpaceRequestCommand> validator)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _validator = validator;
    }

    public async Task<SpaceRequestReceiptDto> Handle(
        SubmitSpaceRequestCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // The honeypot fired. Nothing is written, nothing is sent, and the caller
        // gets a confirmation shaped exactly like a real one — a bot told it was
        // detected is a bot whose author knows which field to leave alone next
        // time. The reference is plausible and belongs to nothing.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return new SpaceRequestReceiptDto(
                Reference(now.Year, 0),
                request.ContactFirstName,
                request.Name,
                request.ContactEmail,
                request.RequestedPlan,
                Subdomains.Normalise(request.RequestedSubdomain),
                now);
        }

        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var subdomain = Subdomains.Normalise(request.RequestedSubdomain);

        // Checked again here rather than trusted from step 5: the field was
        // verified while the applicant was still typing, and somebody else may
        // have asked for the same name in the minutes since.
        await RefuseIfTakenAsync(subdomain, cancellationToken);

        var isGym = request.Type == SpaceRequestType.Gym;

        var spaceRequest = new SpaceRequest(
            await AllocateReferenceAsync(now, cancellationToken),
            request.Type,
            request.Name.Trim())
        {
            Siret = Blank(request.Siret),
            SizeLabel = Blank(request.SizeLabel),
            Disciplines = Blank(request.Disciplines),

            // A gym gives an address, a coach gives an area. Keeping only the one
            // the profile asked for means the console never shows a half-address
            // somebody typed before changing their answer at step 1.
            Street = isGym ? Blank(request.Street) : null,
            ZipCode = isGym ? Blank(request.ZipCode) : null,
            City = isGym ? Blank(request.City) : null,
            AreaLabel = isGym ? null : Blank(request.AreaLabel),

            ContactFirstName = request.ContactFirstName.Trim(),
            ContactLastName = request.ContactLastName.Trim(),
            ContactRole = Blank(request.ContactRole),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = Blank(request.ContactPhone),

            RequestedPlan = request.RequestedPlan,
            AccentHex = Blank(request.AccentHex),
            AccentLabel = Blank(request.AccentLabel),
            RequestedSubdomain = subdomain,
            Message = Blank(request.Message),

            Status = SpaceRequestStatus.ToProcess,
            Source = OnlineForm,
            ReceivedOn = now,

            AcceptedTerms = request.AcceptedTerms,
            AcceptedDataProcessing = request.AcceptedDataProcessing,
            OptedIntoNewsletter = request.OptedIntoNewsletter,

            Activities =
            [
                new SpaceRequestActivity("Demande reçue")
                {
                    Detail = OnlineForm,
                    OccurredOn = now,
                    State = "done"
                }
            ]
        };

        _dbContext.SpaceRequests.Add(spaceRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Sent after the write, and its result deliberately not allowed to undo
        // it: the request is filed either way, and losing six steps of somebody's
        // answers because a mail server was down would be the worse failure. The
        // timeline records what actually happened.
        var delivery = await _emailSender.SendAsync(
            NotificationMessages.SpaceRequestAcknowledgement(
                spaceRequest.Reference,
                spaceRequest.ContactFirstName,
                spaceRequest.Name,
                spaceRequest.ContactEmail,
                spaceRequest.RequestedPlan,
                spaceRequest.RequestedSubdomain),
            cancellationToken);

        _dbContext.SpaceRequestActivities.Add(new SpaceRequestActivity(
            delivery.IsSent ? "Accusé de réception envoyé" : "Accusé de réception non envoyé")
        {
            SpaceRequestId = spaceRequest.Id,
            Detail = delivery.IsSent ? spaceRequest.ContactEmail : delivery.Failure,
            OccurredOn = DateTime.UtcNow,
            State = "done"
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SpaceRequestReceiptDto(
            spaceRequest.Reference,
            spaceRequest.ContactFirstName,
            spaceRequest.Name,
            spaceRequest.ContactEmail,
            spaceRequest.RequestedPlan,
            spaceRequest.RequestedSubdomain,
            spaceRequest.ReceivedOn);
    }

    private async Task RefuseIfTakenAsync(string subdomain, CancellationToken cancellationToken)
    {
        var takenByCustomer = await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(tenant => tenant.Slug == subdomain, cancellationToken);

        var alreadyAsked = await _dbContext.SpaceRequests
            .AsNoTracking()
            .AnyAsync(
                other => other.RequestedSubdomain == subdomain
                         && other.Status != SpaceRequestStatus.Refused,
                cancellationToken);

        if (takenByCustomer || alreadyAsked)
        {
            throw ValidationFailures.Refuse(
                nameof(SubmitSpaceRequestCommand.RequestedSubdomain),
                SpaceRequestRules.SubdomainTaken);
        }
    }

    /// <summary>
    /// DEM-2026-0149: the year, then that year's count.
    /// <para>
    /// The unique index is what actually guarantees it. Two forms submitted in
    /// the same second read the same count and pick the same number, so the loser
    /// takes the exception — which is a retry for the caller rather than a
    /// duplicate reference quoted in two different e-mails.
    /// </para>
    /// <para>
    /// <c>LIKE</c> rather than <c>StartsWith</c>, for the reason spelled out in
    /// <c>CheckSubdomainAvailabilityQueryHandler</c>: the MySQL provider cannot
    /// translate the latter over a parameter, and only a real database says so.
    /// </para>
    /// </summary>
    private async Task<string> AllocateReferenceAsync(DateTime now, CancellationToken cancellationToken)
    {
        var prefix = $"DEM-{now.Year}-";

        var used = await _dbContext.SpaceRequests
            .AsNoTracking()
            .Where(other => EF.Functions.Like(other.Reference, prefix + "%"))
            .Select(other => other.Reference)
            .ToListAsync(cancellationToken);

        var highest = used
            .Select(reference => int.TryParse(reference[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        return Reference(now.Year, highest + 1);
    }

    private static string Reference(int year, int sequence) => $"DEM-{year}-{sequence:D4}";

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
