using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages
{
    public class CustomersModel : PageModel
    {
        private readonly ITableStorageService _tableStorageService;

        public CustomersModel(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        [BindProperty]
        public CustomerProfile NewCustomer { get; set; } = new CustomerProfile();

        public List<CustomerProfile> Customers { get; set; } = new List<CustomerProfile>();

        public async Task OnGetAsync()
        {
            Customers = await _tableStorageService.GetAllCustomerProfilesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _tableStorageService.AddCustomerProfileAsync(NewCustomer);
            return RedirectToPage();
        }
    }
}