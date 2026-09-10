using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace Cloud_Development_B__project_1.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "Order";

        public string RowKey { get; set; } =
            Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        [Required(ErrorMessage = "Please select a customer.")]
        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a product.")]
        [Display(Name = "Product")]
        public string ProductName { get; set; } = string.Empty;

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Display(Name = "Total Amount")]
        public double TotalAmount { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } =
            DateTime.UtcNow;
    }
}