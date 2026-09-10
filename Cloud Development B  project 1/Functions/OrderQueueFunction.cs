using Cloud_Development_B__project_1.Services;

namespace Cloud_Development_B__project_1.Functions
{
    public class OrderQueueFunction
    {
        private readonly IQueueStorageService _queueStorageService;

        public OrderQueueFunction(
            IQueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        public async Task QueueOrderAsync(
            string orderId,
            string customerName,
            decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(orderId));
            }

            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException(
                    "Customer name is required.",
                    nameof(customerName));
            }

            var message =
                $"OrderId: {orderId} | " +
                $"Customer: {customerName} | " +
                $"Total: R{totalAmount:F2} | " +
                $"QueuedAt: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            await _queueStorageService.SendMessageAsync(message);
        }

        public async Task<List<string>> GetQueuedOrdersAsync(
            int maxMessages = 10)
        {
            return await _queueStorageService
                .PeekMessagesAsync(maxMessages);
        }
    }
}