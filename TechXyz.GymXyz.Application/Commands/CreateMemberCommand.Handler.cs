using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateMemberCommand> _validator;

    public CreateMemberCommandHandler(IGymDbContext dbContext, IValidator<CreateMemberCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _dbContext.GetRequiredDefaultGymAsync(cancellationToken);

        var member = new Member(request.FirstName.Trim(), request.LastName.Trim())
        {
            Email = AddressHelper.NormalizeOptional(request.Email),
            Phone = AddressHelper.NormalizeOptional(request.Phone),
            Address = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country)
        };

        defaultGym.AddMember(member);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return member.Id;
    }
}
