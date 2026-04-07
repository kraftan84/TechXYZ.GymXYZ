using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteRoomCommandValidator : AbstractValidator<DeleteRoomCommand>
{
    public DeleteRoomCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
