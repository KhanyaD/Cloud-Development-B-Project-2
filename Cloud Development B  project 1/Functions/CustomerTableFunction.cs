using Cloud_Development_B__project_1.Models;
using Cloud_Development_B__project_1.Services;

namespace Cloud_Development_B__project_1.Functions
{
    public class CustomerTableFunction
    {
        private readonly ITableStorageService<Customer> _customerService;

        public CustomerTableFunction(
            ITableStorageService<Customer> customerService)
        {
            _customerService = customerService;
        }

        public async Task StoreCustomerAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.PartitionKey))
            {
                customer.PartitionKey = "Customers";
            }

            if (string.IsNullOrWhiteSpace(customer.RowKey))
            {
                customer.RowKey = Guid.NewGuid().ToString();
            }

            await _customerService.AddAsync(customer);
        }
    }
}