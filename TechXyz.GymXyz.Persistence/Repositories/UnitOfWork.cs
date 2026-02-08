using System.Collections.Generic;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXyz.GymXyz.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymDbContext _dbContext;
    private readonly Dictionary<string, object> _repositories;
    private bool disposed;

    public UnitOfWork(GymDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repositories = new Dictionary<string, object>();
    }

    public IGenericRepository<T, TU> Repository<T, TU>() where T : EntityBase<TU>
    {
        // Cache repositories by entity type to avoid recreating instances per call.
        var type = typeof(T).Name;
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new GenericRepository<T, TU>(_dbContext);
            _repositories[type] = repository;
        }

        return (IGenericRepository<T, TU>)repository;
    }

    public Task Rollback()
    {
        _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
        return Task.CompletedTask;
    }

    public async Task<int> Save(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveAndRemoveCache(CancellationToken cancellationToken, params string[] cacheKeys)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources once.
                _dbContext.Dispose();
            }

            //dispose unmanaged resources
            disposed = true;
        }
    }
}
