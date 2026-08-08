using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IGymDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;
    private readonly IEmailSender _emailSender;
    private readonly ITenantContext _tenantContext;

    public RequestPasswordResetCommandHandler(
        IGymDbContext dbContext,
        IUserDirectory userDirectory,
        IEmailSender emailSender,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
        _emailSender = emailSender;
        _tenantContext = tenantContext;
    }

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _userDirectory.BeginPasswordResetAsync(request.Email, cancellationToken);

        // Unknown address, another customer's, or a revoked access. The caller is
        // told nothing and shows the same screen: this silence is the feature.
        if (ticket is null)
        {
            return;
        }

        var spaceName = await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Id == _tenantContext.Current)
            .Select(tenant => tenant.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "GymXYZ";

        var link = $"{request.ResetPageUrl}" +
                   $"?email={Uri.EscapeDataString(ticket.Email)}" +
                   $"&token={Uri.EscapeDataString(ticket.Token)}";

        // The result is deliberately dropped. A send that fails must not turn into
        // a different screen, or the difference between "sent" and "not sent"
        // becomes the enumeration the silence above just prevented. In development
        // nothing leaves at all — LoggingEmailSender writes the link to the log,
        // which is where the reset is walked through.
        await _emailSender.SendAsync(
            NotificationMessages.PasswordReset(spaceName, ticket.Email, ticket.DisplayName, link),
            cancellationToken);
    }
}
