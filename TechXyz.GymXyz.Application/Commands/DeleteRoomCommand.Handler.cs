using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteRoomCommand> _validator;

    public DeleteRoomCommandHandler(IUnitOfWork unitOfWork, IValidator<DeleteRoomCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var roomRepository = _unitOfWork.Repository<Room, int>();
        var room = await roomRepository.GetByIdAsync(request.Id);
        if (room is null)
        {
            return false;
        }

        await roomRepository.DeleteAsync(room);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
