using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<DeleteRoomCommand> _validator;

    public DeleteRoomCommandHandler(IGymDbContext dbContext, IValidator<DeleteRoomCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var room = await _dbContext.Rooms.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (room is null)
        {
            return false;
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
