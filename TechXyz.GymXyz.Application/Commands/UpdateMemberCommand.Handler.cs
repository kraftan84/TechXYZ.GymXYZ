using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateMemberCommand> _validator;

    public UpdateMemberCommandHandler(IGymDbContext dbContext, IValidator<UpdateMemberCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var member = await _dbContext.Members
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id && candidate.IsActive, cancellationToken);
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
