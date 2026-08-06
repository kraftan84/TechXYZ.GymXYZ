using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, NotificationOutcomeDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<CancelSessionCommand> _validator;

    public CancelSessionCommandHandler(
        IGymDbContext dbContext,
        IEmailSender emailSender,
        ITenantContext tenantContext,
        IValidator<CancelSessionCommand> validator)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tenantContext = tenantContext;
        _validator = validator;
    }

    public async Task<NotificationOutcomeDto> Handle(
        CancelSessionCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.Id && candidate.IsActive,
                cancellationToken);

        if (session is null)
        {
            return NotificationOutcomeDto.NotFound;
        }

        var reason = AddressHelper.NormalizeOptional(request.Reason);
        var affected = await LoadScopeAsync(session, request.Scope, cancellationToken);

        foreach (var candidate in affected)
        {
            candidate.Status = SessionStatus.Cancelled;
            candidate.CancellationReason = reason;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await WarnRegistrantsAsync(affected, reason, cancellationToken);
    }

    /// <summary>
    /// Tells everybody who held a seat. Runs after the write is committed, so a
    /// mail server that is down costs a warning and never the cancellation.
    /// </summary>
    private async Task<NotificationOutcomeDto> WarnRegistrantsAsync(
        List<Session> cancelled,
        string? reason,
        CancellationToken cancellationToken)
    {
        if (!await NotificationPolicy.AllowsEmailAsync(
                _dbContext, NotificationKey.CourseCancelled, cancellationToken))
        {
            return NotificationOutcomeDto.Suppressed;
        }

        var sessionIds = cancelled.Select(session => session.Id).ToList();

        var seats = await _dbContext.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.IsActive &&
                sessionIds.Contains(registration.SessionId) &&
                registration.Member!.IsActive &&
                registration.Member.Email != null)
            .Select(registration => new
            {
                registration.Member!.FirstName,
                registration.Member.LastName,
                registration.Member.Email,
                // The course name lives on the template, and the message says
                // which course was called off — read here rather than through a
                // second load of the sessions.
                CourseName = registration.Session!.CourseTemplate!.Name,
                registration.Session.StartsAt
            })
            .ToListAsync(cancellationToken);

        if (seats.Count == 0)
        {
            return NotificationOutcomeDto.SavedOnly;
        }

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == _tenantContext.Current, cancellationToken);

        var gymName = tenant?.DisplayName ?? string.Empty;

        var sent = 0;
        var failed = 0;

        foreach (var seat in seats)
        {
            var message = NotificationMessages.CourseCancelled(
                gymName,
                seat.FirstName,
                seat.Email!,
                $"{seat.FirstName} {seat.LastName}",
                seat.CourseName,
                seat.StartsAt,
                reason) with
            {
                FromName = tenant?.DisplayName,
                ReplyToAddress = tenant?.Email,
                ReplyToName = tenant?.DisplayName
            };

            var delivery = await _emailSender.SendAsync(message, cancellationToken);

            if (delivery.IsSent)
            {
                sent++;
            }
            else
            {
                failed++;
            }
        }

        return NotificationOutcomeDto.Delivered(sent, failed);
    }

    private async Task<List<Session>> LoadScopeAsync(
        Session session,
        SessionEditScope scope,
        CancellationToken cancellationToken)
    {
        if (scope == SessionEditScope.ThisOne || session.SeriesId is not { } seriesId)
        {
            return [session];
        }

        // Only forward: cancelling a series must not reach back and rewrite the
        // occurrences that already ran.
        return await _dbContext.Sessions
            .Where(candidate =>
                candidate.IsActive &&
                candidate.SeriesId == seriesId &&
                candidate.StartsAt >= session.StartsAt)
            .ToListAsync(cancellationToken);
    }
}
