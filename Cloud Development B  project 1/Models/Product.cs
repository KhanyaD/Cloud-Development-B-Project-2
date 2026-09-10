using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace Cloud_Development_B__project_1.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";

        public string RowKey { get; set; } =
            Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000000,
            ErrorMessage = "Price must be greater than R0.00.")]
        [Display(Name = "Price")]
        public double Price { get; set; }

        [Range(0, 1000000,
            ErrorMessage = "Quantity cannot be negative.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Url(ErrorMessage = "Please enter a valid image URL.")]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
    }
}