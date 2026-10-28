using ABCRetailAzureApp.Models;

namespace ABCRetailAzureApp.Services
{
    public interface IAzureTableService<T> where T : class, Azure.Data.Tables.ITableEntity, new()
    {
        Task<T?> GetEntityAsync(string partitionKey, string rowKey);
        Task<List<T>> GetAllEntitiesAsync(string partitionKey);
        Task<List<T>> GetAllEntitiesAsync();
        Task<T> AddEntityAsync(T entity);
        Task<T> UpdateEntityAsync(T entity);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
        Task<bool> EntityExistsAsync(string partitionKey, string rowKey);
    }

}
