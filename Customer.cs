using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace Cloud_Development_B__project_1.Models
{
    public class Order : ITableEntity
    {
        [Required]
        public string PartitionKey { get; set; } = "ORDER";

        [Required]
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public double TotalPrice { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}

using Azure;
using Azure.Data.Tables;

namespace Cloud_Development_B__project_1.Services
{
    public class TableStorageService<T> : ITableStorageService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration, string tableName)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var tableServiceClient = new TableServiceClient(connectionString);
            _tableClient = tableServiceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<T?> GetEntityAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task<List<T>> GetAllEntitiesAsync()
        {
            var entities = new List<T>();

            await foreach (var entity in _tableClient.QueryAsync<T>())
            {
                entities.Add(entity);
            }

            return entities;
        }

        public async Task AddEntityAsync(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        public async Task UpdateEntityAsync(T entity)
        {
            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }

    public interface IQueueStorageService
    {
        Task SendMessageAsync(string message);
        Task<string?> ReceiveMessageAsync();
        Task<List<string>> PeekMessagesAsync(int maxMessages = 10);
        Task DeleteMessageAsync(string messageId, string popReceipt);
    }

    using Azure.Storage.Queues;
    using Azure.Storage.Queues.Models;

    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _queueClient = new QueueClient(connectionString, "orders");
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        public async Task<string?> ReceiveMessageAsync()
        {
            var response = await _queueClient.ReceiveMessageAsync();
            if (response.Value != null)
            {
                await _queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                return response.Value.MessageText;
            }
            return null;
        }

        public async Task<List<string>> PeekMessagesAsync(int maxMessages = 10)
        {
            var messages = new List<string>();
            var response = await _queueClient.PeekMessagesAsync(maxMessages);

            foreach (var message in response.Value)
            {
                messages.Add(message.MessageText);
            }

            return messages;
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }

    public interface IFileStorageService
    {
        Task UploadFileAsync(string fileName, string content);
        Task<string> DownloadFileAsync(string fileName);
        Task<List<string>> GetAllFilesAsync();
        Task DeleteFileAsync(string fileName);
    }
}