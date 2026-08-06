using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateTenantCommand> _validator;

    public CreateTenantCommandHandler(
        IGymDbContext dbContext,
        IValidator<CreateTenantCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var slug = request.Slug.Trim().ToLowerInvariant();

        // The column is unique, so this is a courtesy rather than the guarantee:
        // it turns a database error into the sentence the admin needs to read.
        var taken = await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(tenant => tenant.Slug == slug, cancellationToken);

        if (taken)
        {
            throw ValidationFailures.Refuse(TenantFieldNames.Slug, TenantRules.SlugTaken);
        }

        var name = request.Name.Trim();

        var tenant = new Tenant(name, slug, request.ThemeKey)
        {
            DisplayName = name,
            // No mark and no wordmark split: a new customer shows its name alone
            // until it supplies one. There is no default mark to fall back on —
            // GymXYZ's own would be a brand leak in a white-label product.
            WordmarkText = name,
            IsSolo = request.IsSolo
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
