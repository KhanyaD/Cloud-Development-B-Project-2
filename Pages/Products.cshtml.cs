using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IAzureStorageService _storageService;

        [BindProperty]
        public Product NewProduct { get; set; } = new Product();

        [BindProperty]
        public IFormFile? ProductImage { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public ProductsModel(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task OnGetAsync()
        {
            Products = await _storageService.GetAllProductsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Products = await _storageService.GetAllProductsAsync();
                return Page();
            }

            if (ProductImage != null)
            {
                var imageUrl = await _storageService.UploadImageAsync(ProductImage);
                NewProduct.ImageUrl = imageUrl;
            }

            var result = await _storageService.AddProductAsync(NewProduct);

            if (result)
            {
                SuccessMessage = $"Product '{NewProduct.ProductName}' added successfully!";

                await _storageService.SendOrderMessageAsync(
                    $"New product added: {NewProduct.ProductName}, Stock: {NewProduct.StockQuantity}");

                await _storageService.UploadLogFileAsync(
                    $"product-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt",
                    $"Product added: {NewProduct.ProductName}, Price: ${NewProduct.Price}, Stock: {NewProduct.StockQuantity} at {DateTime.UtcNow}");
            }
            else
            {
                ErrorMessage = "Failed to add product. Please try again.";
            }

            return RedirectToPage();
        }
    }
}