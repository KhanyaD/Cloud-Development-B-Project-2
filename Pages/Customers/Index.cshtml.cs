using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ITableStorageService<Customer> _customerService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITableStorageService<Customer> customerService,
            ILogger<IndexModel> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        public List<Customer> Customers { get; set; } = new();

        [BindProperty]
        public Customer Customer { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCustomersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomersAsync();
                return Page();
            }

            try
            {
                Customer.RowKey = Guid.NewGuid().ToString();
                Customer.PartitionKey = "Customer";

                await _customerService.AddAsync(Customer);

                SuccessMessage =
                    $"Customer {Customer.Name} was added successfully.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to add customer to Azure Table Storage.");

                ErrorMessage = "Unable to save the customer to Azure Storage.";
                await LoadCustomersAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            string partitionKey,
            string rowKey)
        {
            try
            {
                await _customerService.DeleteAsync(partitionKey, rowKey);

                SuccessMessage = "Customer deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete customer from Azure Table Storage.");

                ErrorMessage = "Unable to delete the customer from Azure Storage.";
            }

            return RedirectToPage();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                Customers = await _customerService.GetAllAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load customers from Azure Table Storage.");

                ErrorMessage = "Unable to load customers from Azure Storage.";
                Customers = new List<Customer>();
            }
        }
    }
}