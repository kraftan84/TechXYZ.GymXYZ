using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteCoachCommand> _validator;

    public DeleteCoachCommandHandler(IUnitOfWork unitOfWork, IValidator<DeleteCoachCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Coach, int>();
        var coach = await repository.GetByIdAsync(request.Id);
        if (coach is null)
        {
            return false;
        }

        await repository.DeleteAsync(coach);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
