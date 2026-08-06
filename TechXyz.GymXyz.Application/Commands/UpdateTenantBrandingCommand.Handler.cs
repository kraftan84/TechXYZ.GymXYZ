using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateTenantBrandingCommandHandler
    : IRequestHandler<UpdateTenantBrandingCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateTenantBrandingCommand> _validator;

    public UpdateTenantBrandingCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdateTenantBrandingCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(
        UpdateTenantBrandingCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // Loaded and mutated rather than ExecuteUpdateAsync: the latter is not
        // supported by the InMemory provider the tests run on.
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(
                candidate => candidate.Id == request.TenantId && candidate.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            return false;
        }

        tenant.ThemeKey = request.ThemeKey;
        tenant.DisplayName = request.DisplayName.Trim();
        tenant.Baseline = Trimmed(request.Baseline);

        // A wordmark is either whole or split, never both: leaving the other
        // shape behind would let the lockup render yesterday's name.
        var prefix = Trimmed(request.WordmarkPrefix);
        var accent = Trimmed(request.WordmarkAccent);

        if (prefix is not null || accent is not null)
        {
            tenant.WordmarkPrefix = prefix;
            tenant.WordmarkAccent = accent;
            tenant.WordmarkText = null;
        }
        else
        {
            tenant.WordmarkText = Trimmed(request.WordmarkText);
            tenant.WordmarkPrefix = null;
            tenant.WordmarkAccent = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
