using Azure.Data.Tables;

namespace Cloud_Development_B__project_1.Services
{
    public interface ITableStorageService<T>
        where T : class, ITableEntity, new()
    {
        Task<List<T>> GetAllAsync();

        Task<T?> GetAsync(string partitionKey, string rowKey);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(string partitionKey, string rowKey);
    }
}