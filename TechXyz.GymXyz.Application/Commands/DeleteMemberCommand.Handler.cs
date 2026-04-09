using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteMemberCommand> _validator;

    public DeleteMemberCommandHandler(IGymDbContext dbContext, IValidator<DeleteMemberCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var member = await _dbContext.Members
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
        if (member is null)
        {
            return false;
        }

        member.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
