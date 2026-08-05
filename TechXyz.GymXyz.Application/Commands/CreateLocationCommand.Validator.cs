using FluentValidation;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150)
            .WithName(LocationFieldNames.Name);

        RuleFor(command => command.Capacity)
            .InclusiveBetween(LocationRules.MinimumCapacity, LocationRules.MaximumCapacity)
            .WithMessage(LocationRules.CapacityMessage);

        RuleFor(command => command.Capacity)
            .Equal(LocationRules.HomeCapacity)
            .When(command => command.Kind == LocationKind.Home)
            .WithMessage(LocationRules.HomeCapacityMessage);

        RuleFor(command => command.AreaSqm)
            .InclusiveBetween(LocationRules.MinimumAreaSqm, LocationRules.MaximumAreaSqm)
            .When(command => command.AreaSqm.HasValue)
            .WithMessage(LocationRules.AreaMessage);

        RuleFor(command => command.IsWeatherDependent)
            .Equal(false)
            .When(command => command.Kind != LocationKind.Outdoor)
            .WithMessage(LocationRules.WeatherKindMessage);

        RuleFor(command => command.Street)
            .Empty()
            .When(command => command.Kind == LocationKind.Home)
            .WithMessage(LocationRules.HomeAddressMessage);

        RuleFor(command => command.TypeLabel)
            .MaximumLength(120)
            .WithName(LocationFieldNames.TypeLabel);

        RuleFor(command => command.Floor)
            .MaximumLength(60)
            .WithName(LocationFieldNames.Floor);

        RuleFor(command => command.Note)
            .MaximumLength(2000)
            .WithName(LocationFieldNames.Note);

        RuleFor(command => command.IconKey)
            .MaximumLength(50)
            .WithName(LocationFieldNames.IconKey);

        RuleFor(command => command.Tone)
            .MaximumLength(30)
            .WithName(LocationFieldNames.Tone);

        RuleForEach(command => command.Equipment)
            .MaximumLength(120)
            .WithName(LocationFieldNames.Equipment);
    }
}
