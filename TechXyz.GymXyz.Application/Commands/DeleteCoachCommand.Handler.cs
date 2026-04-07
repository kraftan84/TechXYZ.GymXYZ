using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteCoachCommand> _validator;

    public DeleteCoachCommandHandler(IGymDbContext dbContext, IValidator<DeleteCoachCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var coach = await _dbContext.Coaches.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (coach is null)
        {
            return false;
        }

        _dbContext.Coaches.Remove(coach);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
