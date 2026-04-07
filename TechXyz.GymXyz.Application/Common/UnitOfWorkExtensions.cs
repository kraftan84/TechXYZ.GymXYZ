using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Entities;

namespace TechXyz.GymXyz.Application.Common;

public static class UnitOfWorkExtensions
{
    public static Task<Gym?> GetDefaultGymAsync(this IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        return unitOfWork
            .Repository<Gym, int>()
            .Entities
            .OrderBy(gym => gym.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
