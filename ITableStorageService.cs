using Azure.Data.Tables;

namespace Cloud_Development_B__project_1.Services
{
    public interface ITableStorageService
    {
        Task AddCustomerProfileAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomerProfilesAsync();
        Task AddProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();
    }

    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "CustomerProfile";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}