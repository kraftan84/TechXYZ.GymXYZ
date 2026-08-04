using FluentValidation;
using MediatR;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Commands;

public sealed class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, int>
{
    private readonly IGymDbContext _dbContext;
    private readonly IValidator<CreateCoachCommand> _validator;

    public CreateCoachCommandHandler(IGymDbContext dbContext, IValidator<CreateCoachCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Handle(CreateCoachCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var defaultGym = await _dbContext.GetRequiredDefaultGymAsync(cancellationToken);

        var coach = new Coach(request.FirstName.Trim(), request.LastName.Trim())
        {
            Email = AddressHelper.NormalizeOptional(request.Email),
            Phone = AddressHelper.NormalizeOptional(request.Phone),
            RoleLabel = AddressHelper.NormalizeOptional(request.RoleLabel),
            Bio = AddressHelper.NormalizeOptional(request.Bio),
            JoinedOn = request.JoinedOn ?? DateOnly.FromDateTime(DateTime.Today),
            AwayUntil = request.AwayUntil,
            Address = AddressHelper.BuildOptionalAddress(request.Street, request.ZipCode, request.City, request.Country)
        };

        CoachCompositionHelper.ApplyAvailability(coach, request.Availability);

        // Written through the navigations rather than the sync helpers: the
        // coach has no key yet, and EF fixes the foreign keys up on insert.
        if (request.DisciplineIds is { Count: > 0 } disciplineIds)
        {
            var disciplines = await CoachCompositionHelper.LoadOrderedDisciplinesAsync(
                _dbContext, disciplineIds, cancellationToken);

            for (var rank = 0; rank < disciplines.Count; rank++)
            {
                coach.AddDiscipline(disciplines[rank], rank);
            }
        }

        if (request.Certifications is { Count: > 0 } certifications)
        {
            var rank = 0;
            foreach (var label in certifications
                         .Select(AddressHelper.NormalizeOptional)
                         .Where(label => label is not null)
                         .Distinct())
            {
                coach.AddCertification(label!, rank++);
            }
        }

        defaultGym.AddCoach(coach);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return coach.Id;
    }
}
