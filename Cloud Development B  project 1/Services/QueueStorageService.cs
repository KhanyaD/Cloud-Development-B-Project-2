using Azure.Storage.Queues;
using System.Linq;

namespace Cloud_Development_B__project_1.Services
{
    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            _queueClient = new QueueClient(
                connectionString,
                "orders"
            );
        }

        public async Task SendMessageAsync(string message)
        {
            await _queueClient.CreateIfNotExistsAsync();

            await _queueClient.SendMessageAsync(message);
        }

        public async Task<List<string>> PeekMessagesAsync(int maxMessages = 10)
        {
            await _queueClient.CreateIfNotExistsAsync();

            var response =
                await _queueClient.PeekMessagesAsync(maxMessages);

            return response.Value
                .Select(message => message.MessageText)
                .ToList();
        }
    }
}