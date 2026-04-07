using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteMemberCommand> _validator;

    public DeleteMemberCommandHandler(IUnitOfWork unitOfWork, IValidator<DeleteMemberCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Member, int>();
        var member = await repository.GetByIdAsync(request.Id);
        if (member is null)
        {
            return false;
        }

        await repository.DeleteAsync(member);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
