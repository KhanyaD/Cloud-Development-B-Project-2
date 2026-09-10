using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Cloud_Development_B__project_1.Models;

namespace Cloud_Development_B__project_1.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly string _connectionString;
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly QueueClient _queueClient;
        private readonly ShareClient _shareClient;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"] 
                ?? throw new ArgumentNullException("Azure Storage connection string is not configured");

            // Initialize Table Storage
            var tableServiceClient = new TableServiceClient(_connectionString);
            _customerTableClient = tableServiceClient.GetTableClient("Customers");
            _customerTableClient.CreateIfNotExists();

            _productTableClient = tableServiceClient.GetTableClient("Products");
            _productTableClient.CreateIfNotExists();

            // Initialize Blob Storage
            _blobServiceClient = new BlobServiceClient(_connectionString);

            // Initialize Queue Storage
            _queueClient = new QueueClient(_connectionString, "order-processing");
            _queueClient.CreateIfNotExists();

            // Initialize File Storage
            var shareServiceClient = new ShareServiceClient(_connectionString);
            _shareClient = shareServiceClient.GetShareClient("logs");
            _shareClient.CreateIfNotExists();
        }

        #region Table Storage - Customers

        public async Task<bool> AddCustomerAsync(CustomerProfile customer)
        {
            try
            {
                await _customerTableClient.AddEntityAsync(customer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            await foreach (var customer in _customerTableClient.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        #endregion

        #region Table Storage - Products

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                await _productTableClient.AddEntityAsync(product);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        #endregion

        #region Blob Storage - Images

        public async Task<string> UploadImageAsync(IFormFile file, string containerName = "product-images")
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<List<string>> GetAllImageUrlsAsync(string containerName = "product-images")
        {
            var imageUrls = new List<string>();
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                if (await containerClient.ExistsAsync())
                {
                    await foreach (var blobItem in containerClient.GetBlobsAsync())
                    {
                        var blobClient = containerClient.GetBlobClient(blobItem.Name);
                        imageUrls.Add(blobClient.Uri.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }
            return imageUrls;
        }

        #endregion

        #region Queue Storage - Orders

        public async Task<bool> SendOrderMessageAsync(string message)
        {
            try
            {
                await _queueClient.SendMessageAsync(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> ReceiveOrderMessageAsync()
        {
            try
            {
                var response = await _queueClient.ReceiveMessageAsync();
                if (response.Value != null)
                {
                    var message = response.Value.MessageText;
                    await _queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                    return message;
                }
                return "No messages in queue";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        #endregion

        #region File Storage - Logs

        public async Task<bool> UploadLogFileAsync(string fileName, string content)
        {
            try
            {
                var directoryClient = _shareClient.GetDirectoryClient("application-logs");
                await directoryClient.CreateIfNotExistsAsync();

                var fileClient = directoryClient.GetFileClient(fileName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadAsync(stream);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> GetAllLogFilesAsync()
        {
            var logFiles = new List<string>();
            try
            {
                var directoryClient = _shareClient.GetDirectoryClient("application-logs");
                if (await directoryClient.ExistsAsync())
                {
                    await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
                    {
                        if (!item.IsDirectory)
                        {
                            logFiles.Add(item.Name);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }
            return logFiles;
        }

        #endregion
    }
}