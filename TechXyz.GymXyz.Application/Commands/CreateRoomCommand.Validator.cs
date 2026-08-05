using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.SiteId).GreaterThan(0);
    }
}
