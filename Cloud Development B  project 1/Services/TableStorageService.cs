using Azure;
using Azure.Data.Tables;

namespace Cloud_Development_B__project_1.Services
{
    public class TableStorageService<T> : ITableStorageService<T>
        where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _tableClient =
                new TableClient(
                    connectionString,
                    GetTableName());

            _tableClient.CreateIfNotExists();
        }

        public async Task<List<T>> GetAllAsync()
        {
            var results = new List<T>();

            await foreach (
                var entity in _tableClient.QueryAsync<T>())
            {
                results.Add(entity);
            }

            return results;
        }

        public async Task<T?> GetAsync(
            string partitionKey,
            string rowKey)
        {
            try
            {
                var response =
                    await _tableClient.GetEntityAsync<T>(
                        partitionKey,
                        rowKey);

                return response.Value;
            }
            catch (RequestFailedException ex)
                when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task AddAsync(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await _tableClient.UpsertEntityAsync(
                entity,
                TableUpdateMode.Replace);
        }

        public async Task DeleteAsync(
            string partitionKey,
            string rowKey)
        {
            await _tableClient.DeleteEntityAsync(
                partitionKey,
                rowKey);
        }

        private static string GetTableName()
        {
            return typeof(T).Name switch
            {
                "Customer" => "Customers",
                "Product" => "Products",
                "Order" => "Orders",
                _ => $"{typeof(T).Name}s"
            };
        }
    }
}