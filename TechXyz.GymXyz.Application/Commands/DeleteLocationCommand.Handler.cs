using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteLocationCommand> _validator;

    public DeleteLocationCommandHandler(IUnitOfWork unitOfWork, IValidator<DeleteLocationCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var locationRepository = _unitOfWork.Repository<Location, int>();
        var roomRepository = _unitOfWork.Repository<Room, int>();

        var location = await locationRepository.Entities
            .Include(candidate => candidate.Rooms)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

        if (location is null)
        {
            return false;
        }

        foreach (var room in location.Rooms?.ToList() ?? [])
        {
            await roomRepository.DeleteAsync(room);
        }

        await locationRepository.DeleteAsync(location);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
