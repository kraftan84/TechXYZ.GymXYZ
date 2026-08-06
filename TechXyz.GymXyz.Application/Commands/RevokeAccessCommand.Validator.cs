using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class RevokeAccessCommandValidator : AbstractValidator<RevokeAccessCommand>
{
    public RevokeAccessCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
