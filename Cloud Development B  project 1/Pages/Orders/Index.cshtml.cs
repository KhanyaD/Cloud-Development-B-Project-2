using Cloud_Development_B__project_1.Functions;
using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ITableStorageService<Order> _orderService;
        private readonly ITableStorageService<Customer> _customerService;
        private readonly ITableStorageService<Product> _productService;
        private readonly OrderQueueFunction _orderQueueFunction;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITableStorageService<Order> orderService,
            ITableStorageService<Customer> customerService,
            ITableStorageService<Product> productService,
            OrderQueueFunction orderQueueFunction,
            ILogger<IndexModel> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
            _orderQueueFunction = orderQueueFunction;
            _logger = logger;
        }

        public List<Order> Orders { get; set; } = new();

        public List<Customer> Customers { get; set; } = new();

        public List<Product> Products { get; set; } = new();

        public List<string> QueueMessages { get; set; } = new();

        [BindProperty]
        public Order Order { get; set; } = new();

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
            await LoadReferenceDataAsync();

            var selectedCustomer =
                Customers.FirstOrDefault(
                    customer =>
                        customer.Name == Order.CustomerName);

            var selectedProduct =
                Products.FirstOrDefault(
                    product =>
                        product.Name == Order.ProductName);

            if (selectedCustomer == null)
            {
                ModelState.AddModelError(
                    "Order.CustomerName",
                    "Please select a valid customer.");
            }

            if (selectedProduct == null)
            {
                ModelState.AddModelError(
                    "Order.ProductName",
                    "Please select a valid product.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOrdersAndQueueAsync();

                return Page();
            }

            try
            {
                Order.PartitionKey = "Order";

                Order.RowKey =
                    Guid.NewGuid().ToString();

                Order.OrderDate =
                    DateTime.UtcNow;

                // Calculate total using product price
                // retrieved from Azure Table Storage.
                Order.TotalAmount =
                    selectedProduct!.Price *
                    Order.Quantity;

                // Save order to Azure Table Storage.
                await _orderService.AddAsync(Order);

                // PROJECT 2:
                // Send transaction information
                // through the OrderQueueFunction
                // to Azure Queue Storage.
                await _orderQueueFunction.QueueOrderAsync(
                    Order.RowKey,
                    Order.CustomerName,
                    (decimal)Order.TotalAmount);

                SuccessMessage =
                    $"Order for {Order.CustomerName} " +
                    $"created successfully and sent " +
                    $"to Azure Queue Storage.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to create order or send Azure Queue message.");

                ErrorMessage =
                    "Unable to create the order or " +
                    "send the queue message.";

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
                await _orderService.DeleteAsync(
                    partitionKey,
                    rowKey);

                SuccessMessage =
                    "Order deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete order.");

                ErrorMessage =
                    "Unable to delete the order.";
            }

            return RedirectToPage();
        }

        private async Task LoadPageDataAsync()
        {
            await LoadReferenceDataAsync();
            await LoadOrdersAndQueueAsync();
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                Customers =
                    await _customerService.GetAllAsync();

                Products =
                    await _productService.GetAllAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load customers or products.");

                ErrorMessage =
                    "Unable to load customers or products " +
                    "from Azure Table Storage.";

                Customers =
                    new List<Customer>();

                Products =
                    new List<Product>();
            }
        }

        private async Task LoadOrdersAndQueueAsync()
        {
            try
            {
                Orders =
                    await _orderService.GetAllAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load orders.");

                ErrorMessage =
                    "Unable to load orders from " +
                    "Azure Table Storage.";

                Orders =
                    new List<Order>();
            }

            try
            {
                // PROJECT 2:
                // Read transaction messages
                // through the OrderQueueFunction
                // from Azure Queue Storage.
                QueueMessages =
                    await _orderQueueFunction
                        .GetQueuedOrdersAsync(10);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to read Azure Queue messages.");

                QueueMessages =
                    new List<string>();
            }
        }
    }
}