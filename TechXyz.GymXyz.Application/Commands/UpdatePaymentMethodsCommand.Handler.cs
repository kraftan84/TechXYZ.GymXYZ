using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdatePaymentMethodsCommandHandler : IRequestHandler<UpdatePaymentMethodsCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdatePaymentMethodsCommand> _validator;

    public UpdatePaymentMethodsCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdatePaymentMethodsCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdatePaymentMethodsCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var settings = await _dbContext.GymSettings
            .FirstOrDefaultAsync(candidate => candidate.IsActive, cancellationToken);

        if (settings is null)
        {
            // First save for a customer that has never had a settings row. The
            // query reads defaults for one, so this is where it becomes real.
            settings = new GymSettings();
            _dbContext.GymSettings.Add(settings);
        }

        settings.Currency = request.Currency;
        settings.VatMention = request.VatMention;
        settings.AcceptedPaymentMethods = request.AcceptedMethods.ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
