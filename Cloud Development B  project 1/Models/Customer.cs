using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace Cloud_Development_B__project_1.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";

        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(254)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;
    }
}