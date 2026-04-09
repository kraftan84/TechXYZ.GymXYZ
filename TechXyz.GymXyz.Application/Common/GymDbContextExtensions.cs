using Microsoft.EntityFrameworkCore;
using FluentValidation;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class GymDbContextExtensions
{
    public static Task<Gym?> GetDefaultGymAsync(this IGymDbContext dbContext, CancellationToken cancellationToken)
    {
        return dbContext.Gyms
            .Where(gym => gym.IsActive)
            .OrderBy(gym => gym.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<Gym> GetRequiredDefaultGymAsync(this IGymDbContext dbContext, CancellationToken cancellationToken)
    {
        return await dbContext.GetDefaultGymAsync(cancellationToken)
               ?? throw new ValidationException("Default gym not found.");
    }
}
