using System.Collections;
using TechXyz.GymXyz.Application.Interfaces.Repositories;
using TechXyz.GymXyz.Domain.Common;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXyz.GymXyz.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymDbContext _dbContext;
    private readonly Hashtable _repositories;
    private bool disposed;

    public UnitOfWork(GymDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repositories = new Hashtable();
    }

    public IGenericRepository<T, TU> Repository<T, TU>() where T : EntityBase<TU>
    {
        var type = typeof(T).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<T, TU>);

            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<T, TU>)_repositories[type];
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
        if (disposed)
            if (disposing)
                //dispose managed resources
                _dbContext.Dispose();

        //dispose unmanaged resources
        disposed = true;
    }
}