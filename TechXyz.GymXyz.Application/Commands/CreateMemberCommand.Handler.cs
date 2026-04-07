using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateMemberCommand> _validator;

    public CreateMemberCommandHandler(IUnitOfWork unitOfWork, IValidator<CreateMemberCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<int> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _unitOfWork.GetDefaultGymAsync(cancellationToken);

        if (defaultGym is null)
        {
            throw new ValidationException("Default gym not found.");
        }

        var member = new Member(request.FirstName.Trim(), request.LastName.Trim())
        {
            Email = AddressHelper.NormalizeOptional(request.Email),
            Phone = AddressHelper.NormalizeOptional(request.Phone),
            Address = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country)
        };

        defaultGym.AddMember(member);
        await _unitOfWork.Save(cancellationToken);

        return member.Id;
    }
}
