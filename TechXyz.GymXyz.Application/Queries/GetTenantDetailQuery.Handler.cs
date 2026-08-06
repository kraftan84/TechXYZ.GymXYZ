using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetTenantDetailQueryHandler
    : IRequestHandler<GetTenantDetailQuery, TenantDetailDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetTenantDetailQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantDetailDto?> Handle(
        GetTenantDetailQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.TenantId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Slug,
                candidate.DisplayName,
                candidate.ThemeKey,
                candidate.Baseline,
                candidate.LogoPath,
                candidate.LogoDarkPath,
                candidate.CircleLogo,
                candidate.WordmarkText,
                candidate.WordmarkPrefix,
                candidate.WordmarkAccent,
                candidate.IsSolo,
                candidate.City,
                candidate.AreaLabel,
                candidate.GymPlan,
                candidate.PlanDescription,
                candidate.PlanPrice,
                candidate.PlanRenewalDate,
                candidate.PlanMemberCap,
                candidate.PaymentBrand,
                candidate.PaymentLast4,
                candidate.PaymentExpiry
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        // Newest first: the panel lists the history the way it is read, and the
        // most recent invoice is the one anybody is looking for.
        var invoices = await _dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.TenantId == request.TenantId && invoice.IsActive)
            .OrderByDescending(invoice => invoice.Date)
            .ThenByDescending(invoice => invoice.Id)
            .Select(invoice => new InvoiceDto(
                invoice.Id,
                invoice.Reference,
                invoice.Date,
                invoice.Amount,
                invoice.Status))
            .ToListAsync(cancellationToken);

        var counts = await _dbContext.CountActiveByTenantAsync(cancellationToken);

        return new TenantDetailDto(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.ThemeKey,
            tenant.Baseline,
            tenant.LogoPath,
            tenant.LogoDarkPath,
            tenant.CircleLogo,
            tenant.WordmarkText,
            tenant.WordmarkPrefix,
            tenant.WordmarkAccent,
            tenant.IsSolo,
            tenant.City,
            tenant.AreaLabel,
            tenant.GymPlan,
            tenant.PlanDescription,
            tenant.PlanPrice,
            tenant.PlanRenewalDate,
            tenant.PlanMemberCap,
            tenant.PaymentBrand,
            tenant.PaymentLast4,
            tenant.PaymentExpiry,
            counts.CountFor(tenant.Id),
            invoices);
    }
}
