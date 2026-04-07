using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateMemberCommand> _validator;

    public UpdateMemberCommandHandler(IUnitOfWork unitOfWork, IValidator<UpdateMemberCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var repository = _unitOfWork.Repository<Member, int>();
        var member = await repository.Entities.FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);
        if (member is null)
        {
            return false;
        }

        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.Email = AddressHelper.NormalizeOptional(request.Email);
        member.Phone = AddressHelper.NormalizeOptional(request.Phone);

        var updatedAddress = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country);
        member.Address = AddressHelper.Apply(member.Address, updatedAddress);

        await repository.UpdateAsync(member);
        await _unitOfWork.Save(cancellationToken);

        return true;
    }
}
