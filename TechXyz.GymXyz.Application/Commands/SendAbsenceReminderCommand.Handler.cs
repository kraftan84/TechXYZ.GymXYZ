using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class SendAbsenceReminderCommandHandler
    : IRequestHandler<SendAbsenceReminderCommand, NotificationOutcomeDto>
{
    private readonly IGymDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<SendAbsenceReminderCommand> _validator;

    public SendAbsenceReminderCommandHandler(
        IGymDbContext dbContext,
        IEmailSender emailSender,
        ITenantContext tenantContext,
        IValidator<SendAbsenceReminderCommand> validator)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tenantContext = tenantContext;
        _validator = validator;
    }

    public async Task<NotificationOutcomeDto> Handle(
        SendAbsenceReminderCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var members = await _dbContext.Members
            .AsNoTracking()
            .Where(member =>
                member.IsActive &&
                request.MemberIds.Contains(member.Id) &&
                member.Email != null)
            .Select(member => new
            {
                member.Id,
                member.FirstName,
                member.LastName,
                member.Email,
                // The last séance they actually turned up to. What makes the
                // message say something rather than ask a stranger to come back.
                LastSeenOn = member.Registrations!
                    .Where(registration =>
                        registration.IsActive &&
                        registration.Status == AttendanceStatus.Present)
                    .OrderByDescending(registration => registration.Session!.StartsAt)
                    .Select(registration => (DateTime?)registration.Session!.StartsAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
        {
            // Every id was unknown, inactive, or belongs to somebody with no
            // address. Nothing was written, so nothing is claimed.
            return NotificationOutcomeDto.NotFound;
        }

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == _tenantContext.Current, cancellationToken);

        var gymName = tenant?.DisplayName ?? string.Empty;

        var sent = 0;
        var failed = 0;

        foreach (var member in members)
        {
            var message = NotificationMessages.AbsenceChase(
                gymName,
                member.FirstName,
                member.Email!,
                $"{member.FirstName} {member.LastName}",
                member.LastSeenOn is { } seen ? DateOnly.FromDateTime(seen) : null) with
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
}
