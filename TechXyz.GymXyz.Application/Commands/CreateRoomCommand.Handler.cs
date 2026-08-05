using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateRoomCommand> _validator;

    public CreateRoomCommandHandler(IGymDbContext dbContext, IValidator<CreateRoomCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _dbContext.Sites
            .FirstOrDefaultAsync(candidate => candidate.Id == request.SiteId && candidate.IsActive, cancellationToken);

        if (site is null)
        {
            throw new ValidationException("Site not found.");
        }

        var room = new Room(request.Name.Trim());
        site.AddRoom(room);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return room.Id;
    }
}
