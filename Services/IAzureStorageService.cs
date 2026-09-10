using Cloud_Development_B__project_1.Models;

namespace Cloud_Development_B__project_1.Services
{
    public interface IAzureStorageService
    {
        // Table Storage - Customers
        Task<bool> AddCustomerAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomersAsync();

        // Table Storage - Products
        Task<bool> AddProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();

        // Blob Storage - Images
        Task<string> UploadImageAsync(IFormFile file, string containerName = "product-images");
        Task<List<string>> GetAllImageUrlsAsync(string containerName = "product-images");

        // Queue Storage - Orders
        Task<bool> SendOrderMessageAsync(string message);
        Task<string> ReceiveOrderMessageAsync();

        // File Storage - Logs
        Task<bool> UploadLogFileAsync(string fileName, string content);
        Task<List<string>> GetAllLogFilesAsync();
    }
}