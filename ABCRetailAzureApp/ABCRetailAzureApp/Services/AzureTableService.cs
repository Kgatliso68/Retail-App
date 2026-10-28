using Azure;
using Azure.Data.Tables;
using ABCRetailAzureApp.Models;

namespace ABCRetailAzureApp.Services
{
    public class AzureTableService<T> : IAzureTableService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public AzureTableService(IConfiguration configuration, string tableName)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<List<T>> GetAllEntitiesAsync()
        {
            var results = new List<T>();
            await foreach (var tableEntity in _tableClient.QueryAsync<TableEntity>())
            {
                results.Add(ConvertFromTableEntity(tableEntity));
            }
            return results;
        }

        public async Task<List<T>> GetAllEntitiesAsync(string partitionKey)
        {
            var results = new List<T>();
            await foreach (var tableEntity in _tableClient.QueryAsync<TableEntity>($"PartitionKey eq '{partitionKey}'"))
            {
                results.Add(ConvertFromTableEntity(tableEntity));
            }
            return results;
        }

        public async Task<T?> GetEntityAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                return ConvertFromTableEntity(response.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<T> AddEntityAsync(T entity)
        {
            var tableEntity = ConvertToTableEntity(entity);
            await _tableClient.AddEntityAsync(tableEntity);
            return entity;
        }

        public async Task<T> UpdateEntityAsync(T entity)
        {
            var tableEntity = ConvertToTableEntity(entity);
            await _tableClient.UpdateEntityAsync(tableEntity, ETag.All);
            return entity;
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<bool> EntityExistsAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        private T ConvertFromTableEntity(TableEntity tableEntity)
        {
            
            if (typeof(T) == typeof(Product))
            {
                var product = new Product
                {
                    PartitionKey = tableEntity.PartitionKey,
                    RowKey = tableEntity.RowKey,
                    Timestamp = tableEntity.Timestamp,
                    ETag = tableEntity.ETag,
                    ProductName = tableEntity.GetString("ProductName") ?? string.Empty,
                    Description = tableEntity.GetString("Description") ?? string.Empty,
                    Category = tableEntity.GetString("Category") ?? string.Empty,
                    Brand = tableEntity.GetString("Brand") ?? string.Empty,
                    ImageUrl = tableEntity.GetString("ImageUrl") ?? string.Empty,
                    StockQuantity = tableEntity.GetInt32("StockQuantity") ?? 0,
                    IsActive = tableEntity.GetBoolean("IsActive") ?? true,
                    CreatedDate = tableEntity.GetDateTime("CreatedDate") ?? DateTime.UtcNow,
                    LastUpdated = tableEntity.GetDateTime("LastUpdated") ?? DateTime.UtcNow
                };

                
                if (tableEntity.ContainsKey("Price"))
                {
                    var priceObj = tableEntity["Price"];
                    if (priceObj is double d) product.Price = (decimal)d;
                    else if (priceObj is decimal m) product.Price = m;
                    else if (priceObj is int i) product.Price = i;
                    else if (priceObj is string s && decimal.TryParse(s, out var parsed)) product.Price = parsed;
                    else product.Price = 0m;
                }
                else
                {
                    product.Price = 0m;
                }

                return (T)(object)product;
            }

            
            if (typeof(T) == typeof(Order))
            {
                var order = new Order
                {
                    PartitionKey = tableEntity.PartitionKey,
                    RowKey = tableEntity.RowKey,
                    Timestamp = tableEntity.Timestamp,
                    ETag = tableEntity.ETag,
                    OrderId = tableEntity.GetString("OrderId") ?? string.Empty,
                    CustomerId = tableEntity.GetString("CustomerId") ?? string.Empty,
                    ItemsJson = tableEntity.GetString("ItemsJson") ?? string.Empty,
                    ShippingAddress = tableEntity.GetString("ShippingAddress") ?? string.Empty,
                    PaymentMethod = tableEntity.GetString("PaymentMethod") ?? string.Empty,
                    Status = tableEntity.GetString("Status") ?? OrderStatus.Pending.ToString(),
                    CreatedDate = tableEntity.GetDateTime("CreatedDate") ?? DateTime.UtcNow,
                    UpdatedDate = tableEntity.GetDateTime("UpdatedDate")
                };

                
                if (tableEntity.ContainsKey("TotalAmount"))
                {
                    var totalObj = tableEntity["TotalAmount"];
                    if (totalObj is double d) order.TotalAmount = (decimal)d;
                    else if (totalObj is decimal m) order.TotalAmount = m;
                    else if (totalObj is int i) order.TotalAmount = i;
                    else if (totalObj is string s && decimal.TryParse(s, out var parsed)) order.TotalAmount = parsed;
                    else order.TotalAmount = 0m;
                }

                return (T)(object)order;
            }

            
            var entity = new T
            {
                PartitionKey = tableEntity.PartitionKey,
                RowKey = tableEntity.RowKey,
                Timestamp = tableEntity.Timestamp,
                ETag = tableEntity.ETag
            };

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.Name == nameof(ITableEntity.PartitionKey) ||
                    prop.Name == nameof(ITableEntity.RowKey) ||
                    prop.Name == nameof(ITableEntity.Timestamp) ||
                    prop.Name == nameof(ITableEntity.ETag))
                    continue;

                if (tableEntity.ContainsKey(prop.Name) && prop.CanWrite)
                {
                    var value = tableEntity[prop.Name];
                    if (value != null)
                    {
                        try
                        {
                            var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(entity, convertedValue);
                        }
                        catch
                        {
                            
                        }
                    }
                }
            }

            return entity;
        }

        private TableEntity ConvertToTableEntity(T entity)
        {
            var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey)
            {
                ETag = entity.ETag
            };

            
            if (typeof(T) == typeof(Product) && entity is Product product)
            {
                tableEntity[nameof(Product.ProductName)] = product.ProductName;
                tableEntity[nameof(Product.Description)] = product.Description;
                tableEntity[nameof(Product.Category)] = product.Category;
                tableEntity[nameof(Product.Brand)] = product.Brand;
                tableEntity[nameof(Product.ImageUrl)] = product.ImageUrl;
                tableEntity[nameof(Product.Price)] = (double)product.Price; 
                tableEntity[nameof(Product.StockQuantity)] = product.StockQuantity;
                tableEntity[nameof(Product.IsActive)] = product.IsActive;
                tableEntity[nameof(Product.CreatedDate)] = product.CreatedDate;
                tableEntity[nameof(Product.LastUpdated)] = product.LastUpdated;
            }
            
            else if (typeof(T) == typeof(Order) && entity is Order order)
            {
                tableEntity[nameof(Order.OrderId)] = order.OrderId;
                tableEntity[nameof(Order.CustomerId)] = order.CustomerId;
                tableEntity[nameof(Order.ItemsJson)] = order.ItemsJson; 
                tableEntity[nameof(Order.TotalAmount)] = (double)order.TotalAmount; 
                tableEntity[nameof(Order.Status)] = order.Status;
                tableEntity[nameof(Order.CreatedDate)] = order.CreatedDate;
                tableEntity[nameof(Order.UpdatedDate)] = order.UpdatedDate;
                tableEntity[nameof(Order.ShippingAddress)] = order.ShippingAddress;
                tableEntity[nameof(Order.PaymentMethod)] = order.PaymentMethod;

                
            }
            else
            {
                
                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.Name == nameof(ITableEntity.PartitionKey) ||
                        prop.Name == nameof(ITableEntity.RowKey) ||
                        prop.Name == nameof(ITableEntity.Timestamp) ||
                        prop.Name == nameof(ITableEntity.ETag))
                        continue;

                    
                    if (typeof(T) == typeof(Order) && prop.Name == "Items")
                        continue;

                    var value = prop.GetValue(entity);
                    if (value != null)
                    {
                        tableEntity[prop.Name] = value;
                    }
                }
            }

            return tableEntity;
        }
    }
}
