using FluentValidation;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class DeleteMemberCommandValidator : AbstractValidator<DeleteMemberCommand>
{
    public DeleteMemberCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);
    }
}
