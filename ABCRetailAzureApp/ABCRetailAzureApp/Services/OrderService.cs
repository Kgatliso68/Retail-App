using ABCRetailAzureApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailAzureApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAzureTableService<Order> _orderTableService;
        private readonly IAzureQueueService _queueService;
        private readonly IAzureFileService _fileService;

        public OrderService(
            IAzureTableService<Order> orderTableService,
            IAzureQueueService queueService,
            IAzureFileService fileService)
        {
            _orderTableService = orderTableService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public async Task<string> CreateOrderAsync(Order order)
        {
            try
            {
                
                if (string.IsNullOrEmpty(order.OrderId))
                {
                    order.OrderId = Guid.NewGuid().ToString();
                    order.RowKey = order.OrderId;
                }

                
                if (!string.IsNullOrEmpty(order.CustomerId))
                {
                    order.PartitionKey = order.CustomerId;
                }

                order.CreatedDate = DateTime.UtcNow;
                order.Status = OrderStatus.Pending.ToString();


                
                await _orderTableService.AddEntityAsync(order);

                
                await _queueService.SendOrderProcessingMessageAsync(
                    order.OrderId,
                    "OrderCreated",
                    $"Customer: {order.CustomerId}, Items: {order.Items?.Count ?? 0}"
                );

                
                await _fileService.LogOrderProcessingAsync(
                    order.OrderId,
                    "OrderCreated",
                    $"Customer: {order.CustomerId}, Total: {order.TotalAmount:C}"
                );

                return order.OrderId;
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to create order for customer {order.CustomerId}",
                    ex.StackTrace ?? ""
                );
                throw;
            }
        }

        public async Task<Order> GetOrderAsync(string orderId, string customerId)
        {
            try
            {
                string partitionKey = string.IsNullOrEmpty(customerId) ? "ORDER" : customerId;
                return await _orderTableService.GetEntityAsync(partitionKey, orderId);
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to retrieve order {orderId}",
                    ex.StackTrace ?? ""
                );
                return null;
            }
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId)
        {
            try
            {
                return await _orderTableService.GetAllEntitiesAsync(customerId);
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to retrieve orders for customer {customerId}",
                    ex.StackTrace ?? ""
                );
                return new List<Order>();
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _orderTableService.GetAllEntitiesAsync(); // no partition key needed
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    "Failed to retrieve all orders",
                    ex.StackTrace ?? ""
                );
                return new List<Order>();
            }
        }



        public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status, string customerId = "")
        {
            try
            {
                string partitionKey = string.IsNullOrEmpty(customerId) ? "ORDER" : customerId;
                var order = await _orderTableService.GetEntityAsync(partitionKey, orderId);

                if (order == null) return false;

                var oldStatus = order.Status;
                order.Status = status.ToString(); 
                order.UpdatedDate = DateTime.UtcNow;

                await _orderTableService.UpdateEntityAsync(order);

                
                await _queueService.SendOrderProcessingMessageAsync(
                    orderId,
                    "StatusUpdated",
                    $"From: {oldStatus} To: {status}"
                );

                
                await _fileService.LogOrderProcessingAsync(
                    orderId,
                    "StatusUpdated",
                    $"From: {oldStatus} To: {status}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to update order status for order {orderId}",
                    ex.StackTrace ?? ""
                );
                return false;
            }
        }


        public async Task<bool> CancelOrderAsync(string orderId, string customerId = "")
        {
            try
            {
                var result = await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, customerId);

                if (result)
                {
                    await _queueService.SendOrderProcessingMessageAsync(
                        orderId,
                        "OrderCancelled",
                        "Order cancelled by request"
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to cancel order {orderId}",
                    ex.StackTrace ?? ""
                );
                return false;
            }
        }

        public async Task ProcessOrderPaymentAsync(string orderId, string customerId = "")
        {
            try
            {
                await _queueService.SendOrderProcessingMessageAsync(
                    orderId,
                    "ProcessPayment",
                    "Payment processing initiated"
                );

                await _fileService.LogOrderProcessingAsync(
                    orderId,
                    "ProcessPayment",
                    "Payment processing initiated"
                );
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to process payment for order {orderId}",
                    ex.StackTrace ?? ""
                );
            }
        }

        public async Task FulfillOrderAsync(string orderId, string customerId = "")
        {
            try
            {
                await UpdateOrderStatusAsync(orderId, OrderStatus.Shipped, customerId);

                await _queueService.SendOrderProcessingMessageAsync(
                    orderId,
                    "OrderFulfilled",
                    "Order fulfilled and shipped"
                );
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync(
                    $"Failed to fulfill order {orderId}",
                    ex.StackTrace ?? ""
                );
            }
        }
    }
}

