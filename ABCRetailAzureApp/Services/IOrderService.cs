using ABCRetailAzureApp.Models;

namespace ABCRetailAzureApp.Services
{
    public interface IOrderService
    {
        Task<string> CreateOrderAsync(Order order);
        Task<Order> GetOrderAsync(string orderId, string customerId);
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId);
        Task<IEnumerable<Order>> GetAllOrdersAsync(); 
        Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status, string customerId = "");
        Task<bool> CancelOrderAsync(string orderId, string customerId = "");
        Task ProcessOrderPaymentAsync(string orderId, string customerId = "");
        Task FulfillOrderAsync(string orderId, string customerId = "");
    }


}
