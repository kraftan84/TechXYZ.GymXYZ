using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class InviteTeamMemberCommandHandler : IRequestHandler<InviteTeamMemberCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;
    private readonly IValidator<InviteTeamMemberCommand> _validator;

    public InviteTeamMemberCommandHandler(
        IGymDbContext dbContext,
        IUserDirectory userDirectory,
        IValidator<InviteTeamMemberCommand> validator)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
        _validator = validator;
    }

    public async Task<bool> Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _userDirectory.FindByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw ValidationFailures.Refuse(
                SettingsFieldNames.Invitation, SettingsRules.AccountAlreadyExists);
        }

        var pending = await _dbContext.Invitations
            .FirstOrDefaultAsync(
                invitation =>
                    invitation.IsActive &&
                    invitation.AcceptedOn == null &&
                    invitation.Email == request.Email,
                cancellationToken);

        if (pending is not null)
        {
            throw ValidationFailures.Refuse(
                SettingsFieldNames.Invitation, SettingsRules.InvitationAlreadySent);
        }

        if (request.MemberId is { } memberId)
        {
            var memberExists = await _dbContext.Members
                .AnyAsync(member => member.Id == memberId && member.IsActive, cancellationToken);

            if (!memberExists)
            {
                return false;
            }
        }

        _dbContext.Invitations.Add(new Invitation
        {
            Email = request.Email,
            RoleName = request.RoleName,
            MemberId = request.MemberId,
            SentOn = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
