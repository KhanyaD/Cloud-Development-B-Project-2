using System.Text.Json;
using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ITableStorageService<Order> _orderService;
        private readonly IQueueStorageService _queueService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITableStorageService<Order> orderService,
            IQueueStorageService queueService,
            ILogger<IndexModel> logger)
        {
            _orderService = orderService;
            _queueService = queueService;
            _logger = logger;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty]
        public Order Order { get; set; } = new();

        public List<string> QueueMessages { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadPageDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                return Page();
            }

            try
            {
                Order.RowKey = Guid.NewGuid().ToString();
                Order.PartitionKey = "Order";
                Order.OrderDate = DateTime.UtcNow;

                await _orderService.AddAsync(Order);

                var queueMessage = JsonSerializer.Serialize(new
                {
                    OrderId = Order.RowKey,
                    CustomerName = Order.CustomerName,
                    ProductName = Order.ProductName,
                    Quantity = Order.Quantity,
                    TotalAmount = Order.TotalAmount,
                    OrderDate = Order.OrderDate
                });

                await _queueService.SendMessageAsync(queueMessage);

                SuccessMessage =
                    $"Order for {Order.CustomerName} was created and queued successfully.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to create and queue order.");

                ErrorMessage = "Unable to send the order message to Azure Queue Storage.";
                await LoadPageDataAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            string partitionKey,
            string rowKey)
        {
            try
            {
                await _orderService.DeleteAsync(partitionKey, rowKey);

                SuccessMessage = "Order deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete order from Azure Table Storage.");

                ErrorMessage = "Unable to delete the order from Azure Storage.";
            }

            return RedirectToPage();
        }

        private async Task LoadPageDataAsync()
        {
            try
            {
                Orders = await _orderService.GetAllAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load orders from Azure Table Storage.");

                ErrorMessage = "Unable to load orders from Azure Storage.";
                Orders = new List<Order>();
            }

            try
            {
                QueueMessages = await _queueService.PeekMessagesAsync(10);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to read messages from Azure Queue Storage.");

                ErrorMessage = "Unable to access Azure Queue Storage.";
                QueueMessages = new List<string>();
            }
        }
    }
}