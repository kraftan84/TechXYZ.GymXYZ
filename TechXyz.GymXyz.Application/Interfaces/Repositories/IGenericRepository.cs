using TechXyz.GymXyz.Domain.Common.Interfaces;

namespace TechXyz.GymXyz.Application.Interfaces.Repositories;

public interface IGenericRepository<T, TU> where T : class, IEntity<TU>
{
    IQueryable<T> Entities { get; }

    Task<T?> GetByIdAsync(TU id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
