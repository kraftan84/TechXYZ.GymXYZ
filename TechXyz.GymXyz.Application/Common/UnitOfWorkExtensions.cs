using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class UnitOfWorkExtensions
{
    public static Task<Gym?> GetDefaultGymAsync(this IGymDbContext dbContext, CancellationToken cancellationToken)
    {
        return dbContext.Gyms
            .OrderBy(gym => gym.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
