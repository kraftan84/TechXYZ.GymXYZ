using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteSiteCommandValidator : AbstractValidator<DeleteSiteCommand>
{
    public DeleteSiteCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
