using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class UpdateNotificationSettingsCommandHandler
    : IRequestHandler<UpdateNotificationSettingsCommand, bool>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<UpdateNotificationSettingsCommand> _validator;

    public UpdateNotificationSettingsCommandHandler(
        IGymDbContext dbContext,
        IValidator<UpdateNotificationSettingsCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> Handle(
        UpdateNotificationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var stored = await _dbContext.NotificationSettings.ToListAsync(cancellationToken);

        foreach (var input in request.Settings)
        {
            var setting = stored.FirstOrDefault(candidate => candidate.Key == input.Key);

            if (setting is null)
            {
                setting = NotificationDefaults.Create(input.Key);
                _dbContext.NotificationSettings.Add(setting);
            }

            setting.IsActive = true;
            setting.IsEnabled = input.IsEnabled;
            setting.Channels = input.Channels;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
