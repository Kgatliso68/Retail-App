using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABCRetailAzureApp.Services
{
    public class AzureQueueService : IAzureQueueService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, QueueClient> _queueClients;

        public AzureQueueService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") ??
                throw new InvalidOperationException("Azure Storage connection string not found");
            _queueClients = new Dictionary<string, QueueClient>();
        }

        private QueueClient GetQueueClient(string queueName)
        {
            if (!_queueClients.ContainsKey(queueName))
            {
                var queueClient = new QueueClient(_connectionString, queueName.ToLower());
                _queueClients[queueName] = queueClient;
            }
            return _queueClients[queueName];
        }

        public async Task CreateQueueIfNotExistsAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        public async Task<QueueMessage?> ReceiveMessageAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.ReceiveMessageAsync();
            return response.Value;
        }

        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.DeleteMessageAsync(messageId, popReceipt);
        }

        public async Task<int> GetQueueLengthAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var properties = await queueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }

        
        public async Task SendOrderProcessingMessageAsync(string orderId, string action, string details = "")
        {
            var message = System.Text.Json.JsonSerializer.Serialize(new
            {
                MessageType = "OrderProcessing",
                OrderId = orderId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            });

            await SendMessageAsync("order-processing", message);
        }

        public async Task SendInventoryUpdateMessageAsync(string productId, string action, int quantity = 0)
        {
            var message = System.Text.Json.JsonSerializer.Serialize(new
            {
                MessageType = "InventoryUpdate",
                ProductId = productId,
                Action = action,
                Quantity = quantity,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            });

            await SendMessageAsync("inventory-updates", message);
        }

        public async Task SendImageProcessingMessageAsync(string imageName, string action, string containerName = "")
        {
            var message = System.Text.Json.JsonSerializer.Serialize(new
            {
                MessageType = "ImageProcessing",
                ImageName = imageName,
                Action = action,
                ContainerName = containerName,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            });

            await SendMessageAsync("image-processing", message);
        }
    }
}
