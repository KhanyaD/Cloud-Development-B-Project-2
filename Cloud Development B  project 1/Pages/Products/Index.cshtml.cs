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

        public bool IsEditing { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadProductsAsync();
        }

        public async Task<IActionResult> OnGetEditAsync(
            string partitionKey,
            string rowKey)
        {
            await LoadProductsAsync();

            try
            {
                var product = await _productService.GetAsync(
                    partitionKey,
                    rowKey);

                if (product == null)
                {
                    ErrorMessage = "The selected product could not be found.";
                    return RedirectToPage();
                }

                Product = product;
                IsEditing = true;

                return Page();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load product for editing.");

                ErrorMessage = "Unable to load the selected product.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
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

                SuccessMessage = "Product added successfully.";
                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to add product to Azure Table Storage.");

                ErrorMessage = "Unable to save the product.";

                await LoadProductsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                IsEditing = true;
                await LoadProductsAsync();

                return Page();
            }

            try
            {
                var existingProduct = await _productService.GetAsync(
                    Product.PartitionKey,
                    Product.RowKey);

                if (existingProduct == null)
                {
                    ErrorMessage = "The selected product could not be found.";
                    return RedirectToPage();
                }

                existingProduct.Name = Product.Name;
                existingProduct.Description = Product.Description;
                existingProduct.Price = Product.Price;
                existingProduct.Quantity = Product.Quantity;
                existingProduct.ImageUrl = Product.ImageUrl;

                await _productService.UpdateAsync(existingProduct);

                SuccessMessage = "Product updated successfully.";
                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to update product in Azure Table Storage.");

                ErrorMessage = "Unable to update the product.";

                IsEditing = true;
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
                await _productService.DeleteAsync(
                    partitionKey,
                    rowKey);

                SuccessMessage = "Product deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete product from Azure Table Storage.");

                ErrorMessage = "Unable to delete the product.";
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

                ErrorMessage =
                    "Unable to load products from Azure Table Storage.";

                Products = new List<Product>();
            }
        }
    }
}