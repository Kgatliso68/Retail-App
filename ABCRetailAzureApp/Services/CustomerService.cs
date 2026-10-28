using ABCRetailAzureApp.Models;

namespace ABCRetailAzureApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IAzureTableService<CustomerProfile> _tableService;

        public CustomerService(IAzureTableService<CustomerProfile> tableService)
        {
            _tableService = tableService;
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            return await _tableService.GetAllEntitiesAsync("Customer");
        }

        public async Task<CustomerProfile?> GetCustomerAsync(string customerId)
        {
            return await _tableService.GetEntityAsync("Customer", customerId);
        }

        public async Task<CustomerProfile> CreateCustomerAsync(CustomerProfile customer)
        {
            customer.RegistrationDate = DateTime.UtcNow;
            return await _tableService.AddEntityAsync(customer);
        }

        public async Task<CustomerProfile> UpdateCustomerAsync(CustomerProfile customer)
        {
            return await _tableService.UpdateEntityAsync(customer);
        }

        public async Task DeleteCustomerAsync(string customerId)
        {
            await _tableService.DeleteEntityAsync("Customer", customerId);
        }

        public async Task<bool> CustomerExistsAsync(string customerId)
        {
            return await _tableService.EntityExistsAsync("Customer", customerId);
        }
    }
}
