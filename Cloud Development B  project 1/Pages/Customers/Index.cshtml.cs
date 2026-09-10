using Cloud_Development_B__project_1.Functions;
using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ITableStorageService<Customer> _customerService;
        private readonly CustomerTableFunction _customerTableFunction;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITableStorageService<Customer> customerService,
            CustomerTableFunction customerTableFunction,
            ILogger<IndexModel> logger)
        {
            _customerService = customerService;
            _customerTableFunction = customerTableFunction;
            _logger = logger;
        }

        public List<Customer> Customers { get; set; } = new();

        [BindProperty]
        public Customer Customer { get; set; } = new();

        public bool IsEditing { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCustomersAsync();
        }

        public async Task<IActionResult> OnGetEditAsync(
            string partitionKey,
            string rowKey)
        {
            var customer = await _customerService.GetAsync(
                partitionKey,
                rowKey);

            if (customer == null)
            {
                ErrorMessage = "Customer could not be found.";
                return RedirectToPage();
            }

            Customer = customer;
            IsEditing = true;

            await LoadCustomersAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomersAsync();
                return Page();
            }

            try
            {
                Customer.PartitionKey = "Customer";
                Customer.RowKey = Guid.NewGuid().ToString();

                await _customerTableFunction.StoreCustomerAsync(Customer);

                SuccessMessage = "Customer added successfully.";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while adding the customer.");

                ErrorMessage =
                    "An error occurred while adding the customer.";

                await LoadCustomersAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                IsEditing = true;
                await LoadCustomersAsync();

                return Page();
            }

            try
            {
                await _customerService.UpdateAsync(Customer);

                SuccessMessage = "Customer updated successfully.";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while updating the customer.");

                ErrorMessage =
                    "An error occurred while updating the customer.";

                IsEditing = true;

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
                await _customerService.DeleteAsync(
                    partitionKey,
                    rowKey);

                SuccessMessage = "Customer deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while deleting the customer.");

                ErrorMessage =
                    "An error occurred while deleting the customer.";
            }

            return RedirectToPage();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                Customers =
                    await _customerService.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while loading customers.");

                Customers = new List<Customer>();

                ErrorMessage =
                    "An error occurred while loading customers.";
            }
        }
    }
}