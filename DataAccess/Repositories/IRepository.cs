namespace DataAccess.Repositories;

public interface IRepository<T>
{
    public Task<List<T>> GetAsync();
    public Task<T?> GetAsync(string id);
    public Task<T> CreateAsync(T obj);
    public Task UpdateAsync(string id, T updatedObj);
    public Task RemoveAsync(string id);
}