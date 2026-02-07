using TechXyz.GymXyz.Domain.Common;

namespace TechXyz.GymXyz.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T, TU> Repository<T, TU>() where T : EntityBase<TU>;

    Task<int> Save(CancellationToken cancellationToken);

    Task<int> SaveAndRemoveCache(CancellationToken cancellationToken, params string[] cacheKeys);

    Task Rollback();
}