using ABCRetailAzureApp.Models;

namespace ABCRetailAzureApp.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerProfile>> GetAllCustomersAsync();
        Task<CustomerProfile?> GetCustomerAsync(string customerId);
        Task<CustomerProfile> CreateCustomerAsync(CustomerProfile customer);
        Task<CustomerProfile> UpdateCustomerAsync(CustomerProfile customer);
        Task DeleteCustomerAsync(string customerId);
        Task<bool> CustomerExistsAsync(string customerId);
    }
}
