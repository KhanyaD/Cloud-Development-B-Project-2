using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ITableStorageService<Product> _productService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITableStorageService<Product> productService,
            ILogger<IndexModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty]
        public Product Product { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadProductsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return Page();
            }

            try
            {
                Product.RowKey = Guid.NewGuid().ToString();
                Product.PartitionKey = "Product";

                await _productService.AddAsync(Product);

                SuccessMessage =
                    $"Product '{Product.Name}' was added successfully.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to add product to Azure Table Storage.");

                ErrorMessage = "Unable to save the product to Azure Storage.";
                await LoadProductsAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            string partitionKey,
            string rowKey)
        {
            try
            {
                await _productService.DeleteAsync(partitionKey, rowKey);

                SuccessMessage = "Product deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete product from Azure Table Storage.");

                ErrorMessage = "Unable to delete the product from Azure Storage.";
            }

            return RedirectToPage();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                Products = await _productService.GetAllAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load products from Azure Table Storage.");

                ErrorMessage = "Unable to load products from Azure Storage.";
                Products = new List<Product>();
            }
        }
    }
}