using Azure;
using Azure.Data.Tables;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABCRetailAzureApp.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ItemsJson { get; set; } = string.Empty; 
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = OrderStatus.Pending.ToString();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        
        private List<OrderItem>? _items;

        [JsonIgnore] 
        public List<OrderItem> Items
        {
            get
            {
                if (_items == null)
                {
                    if (!string.IsNullOrEmpty(ItemsJson))
                    {
                        try
                        {
                            _items = JsonSerializer.Deserialize<List<OrderItem>>(ItemsJson) ?? new List<OrderItem>();
                        }
                        catch (JsonException)
                        {
                            _items = new List<OrderItem>();
                        }
                    }
                    else
                    {
                        _items = new List<OrderItem>();
                    }
                }
                return _items;
            }
            set
            {
                _items = value;
                ItemsJson = value != null ? JsonSerializer.Serialize(value) : string.Empty;
            }
        }

        
        public Order()
        {
            PartitionKey = "ORDER";
            RowKey = string.Empty;
            CreatedDate = DateTime.UtcNow;
        }

        public Order(string orderId) : this()
        {
            OrderId = orderId;
            RowKey = orderId;
        }

        public Order(string customerId, string orderId) : this()
        {
            CustomerId = customerId;
            OrderId = orderId;
            PartitionKey = customerId; 
            RowKey = orderId;
        }
    }
}