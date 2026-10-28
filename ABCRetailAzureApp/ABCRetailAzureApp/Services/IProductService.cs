using ABCRetailAzureApp.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailAzureApp.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(string productId);
        Task<Product> CreateProductAsync(Product product, IFormFile? imageFile);
        Task<Product> UpdateProductAsync(Product product, IFormFile? imageFile);
        Task DeleteProductAsync(string productId);
        Task<bool> ProductExistsAsync(string productId);
        Task<List<Product>> GetActiveProductsAsync();
        Task<List<Product>> GetProductsByCategoryAsync(string category);
    }
}
