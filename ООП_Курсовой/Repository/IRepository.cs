namespace ООП_Курсовой.Repository;

public interface IRepository<T> : IDisposable where T : class
{
    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);

    Task<T?> GetByIdAsync(long id);

    Task<List<T>> GetAllAsync();

    Task SaveChangesAsync();
}