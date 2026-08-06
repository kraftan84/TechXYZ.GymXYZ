using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTenantPlanCommandHandler
    : IRequestHandler<UpdateTenantPlanCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateTenantPlanCommand> _validator;

    public UpdateTenantPlanCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdateTenantPlanCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(
        UpdateTenantPlanCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.TenantId && candidate.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            return false;
        }

        tenant.GymPlan = Trimmed(request.GymPlan);
        tenant.PlanDescription = Trimmed(request.PlanDescription);
        tenant.PlanPrice = request.PlanPrice;
        tenant.PlanRenewalDate = request.PlanRenewalDate;
        tenant.PlanMemberCap = request.PlanMemberCap;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
