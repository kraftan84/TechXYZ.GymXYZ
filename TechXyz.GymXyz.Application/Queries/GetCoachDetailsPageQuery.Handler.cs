using MediatR;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Common;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.Application.Queries;

public sealed class GetCoachDetailsPageQueryHandler
    : IRequestHandler<GetCoachDetailsPageQuery, CoachDetailsPageDto?>
{
    private readonly IGymDbContext _dbContext;

    public GetCoachDetailsPageQueryHandler(IGymDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CoachDetailsPageDto?> Handle(GetCoachDetailsPageQuery request, CancellationToken cancellationToken)
    {
        // Projected into an anonymous shape first: the record carries pieces the
        // database knows nothing about (the empty week, the derived standing),
        // and composing them in the projection is what fails to translate.
        var coach = await _dbContext.Coaches
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.CoachId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.RoleLabel,
                candidate.Bio,
                candidate.JoinedOn,
                candidate.AwayUntil,
                HasAccount = candidate.UserId != null,
                Address = candidate.Address == null
                    ? null
                    : new AddressDto(
                        candidate.Address.Street,
                        candidate.Address.ZipCode,
                        candidate.Address.City,
                        candidate.Address.Country),
                Availability = new WeeklyAvailabilityDto(
                    candidate.AvailableOnMonday,
                    candidate.AvailableOnTuesday,
                    candidate.AvailableOnWednesday,
                    candidate.AvailableOnThursday,
                    candidate.AvailableOnFriday,
                    candidate.AvailableOnSaturday,
                    candidate.AvailableOnSunday),
                Disciplines = candidate.Disciplines!
                    .Where(link => link.IsActive && link.Discipline!.IsActive)
                    .OrderBy(link => link.Rank)
                    .Select(link => new DisciplineDto(
                        link.Discipline!.Id,
                        link.Discipline.Name,
                        link.Discipline.IconKey,
                        link.Discipline.Tone))
                    .ToList(),
                Certifications = candidate.Certifications!
                    .Where(certification => certification.IsActive)
                    .OrderBy(certification => certification.Rank)
                    .Select(certification => certification.Label)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (coach is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        return new CoachDetailsPageDto(
            coach.Id,
            coach.FirstName,
            coach.LastName,
            coach.Email,
            coach.Phone,
            coach.RoleLabel,
            coach.Bio,
            coach.JoinedOn,
            coach.AwayUntil,
            coach.Address,
            coach.Availability,
            coach.Disciplines,
            coach.Certifications,
            // Sessions of the week arrive with the planning (lot 5).
            [],
            CoachStatsDto.Empty)
        {
            Status = CoachStatusRules.Resolve(coach.AwayUntil, today),
            HasAccount = coach.HasAccount
        };
    }
}
