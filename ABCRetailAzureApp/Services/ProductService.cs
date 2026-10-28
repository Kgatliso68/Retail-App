using ABCRetailAzureApp.Models;
using Microsoft.AspNetCore.Http;

namespace ABCRetailAzureApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IAzureTableService<Product> _tableService;
        private readonly IBlobStorageService _blobService;
        private readonly IAzureQueueService _queueService;
        private readonly IAzureFileService _fileService;

        private const string ImageContainer = "productimages";
        private const string TableName = "Products";

        public ProductService(
            IAzureTableService<Product> tableService,
            IBlobStorageService blobService,
            IAzureQueueService queueService,
            IAzureFileService fileService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _tableService.GetAllEntitiesAsync("Product");
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync("Failed to fetch all products", ex.ToString());
                return new List<Product>();
            }
        }

        

        public async Task<Product?> GetProductAsync(string productId)
        {
            try
            {
                return await _tableService.GetEntityAsync("Product", productId);
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync($"Failed to fetch product {productId}", ex.ToString());
                return null;
            }
        }

        public async Task<Product> CreateProductAsync(Product product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    await _queueService.SendImageProcessingMessageAsync(imageFile.FileName, "UploadStarted", ImageContainer);

                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile, ImageContainer);

                    await _queueService.SendImageProcessingMessageAsync(imageFile.FileName, "UploadCompleted", ImageContainer);
                    await _fileService.LogImageProcessingAsync(imageFile.FileName, "Uploaded", $"Image stored at {product.ImageUrl}");
                }

                product.CreatedDate = DateTime.UtcNow;
                product.LastUpdated = DateTime.UtcNow;

                var created = await _tableService.AddEntityAsync(product);

                await _queueService.SendInventoryUpdateMessageAsync(product.RowKey, "ProductCreated", product.StockQuantity);
                await _fileService.LogInventoryUpdateAsync(product.RowKey, "ProductCreated", product.StockQuantity);

                return created;
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync($"Failed to create product {product?.ProductName}", ex.ToString());
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product, IFormFile? imageFile)
        {
            try
            {
                var existingProduct = await GetProductAsync(product.RowKey);

                if (imageFile != null && imageFile.Length > 0)
                {
                    await _queueService.SendImageProcessingMessageAsync(imageFile.FileName, "UpdateStarted", ImageContainer);

                    if (!string.IsNullOrEmpty(existingProduct?.ImageUrl))
                    {
                        await _blobService.DeleteImageAsync(existingProduct.ImageUrl, ImageContainer);
                        await _fileService.LogImageProcessingAsync(existingProduct.ImageUrl, "Deleted", "Replaced with new image");
                    }

                    product.ImageUrl = await _blobService.UploadImageAsync(imageFile, ImageContainer);

                    await _queueService.SendImageProcessingMessageAsync(imageFile.FileName, "UpdateCompleted", ImageContainer);
                    await _fileService.LogImageProcessingAsync(imageFile.FileName, "Updated", $"Replaced image at {product.ImageUrl}");
                }
                else if (existingProduct != null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                product.LastUpdated = DateTime.UtcNow;

                var updated = await _tableService.UpdateEntityAsync(product);

                if (existingProduct != null && existingProduct.StockQuantity != product.StockQuantity)
                {
                    var diff = product.StockQuantity - existingProduct.StockQuantity;
                    var action = diff > 0 ? "StockIncreased" : "StockDecreased";

                    await _queueService.SendInventoryUpdateMessageAsync(product.RowKey, action, Math.Abs(diff));
                    await _fileService.LogInventoryUpdateAsync(product.RowKey, action, Math.Abs(diff));
                }

                return updated;
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync($"Failed to update product {product?.RowKey}", ex.ToString());
                throw;
            }
        }

        public async Task DeleteProductAsync(string productId)
        {
            try
            {
                var product = await GetProductAsync(productId);

                if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imageName = Path.GetFileName(new Uri(product.ImageUrl).AbsolutePath);
                    await _blobService.DeleteImageAsync(product.ImageUrl, ImageContainer);

                    await _queueService.SendImageProcessingMessageAsync(imageName, "Deleted", ImageContainer);
                    await _fileService.LogImageProcessingAsync(imageName, "Deleted", $"Deleted due to product {product.ProductName} removal");
                }

                await _tableService.DeleteEntityAsync("Product", productId);

                await _queueService.SendInventoryUpdateMessageAsync(productId, "ProductDeleted", 0);
                await _fileService.LogInventoryUpdateAsync(productId, "ProductDeleted", 0);
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync($"Failed to delete product {productId}", ex.ToString());
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(string productId)
        {
            try
            {
                return await _tableService.EntityExistsAsync("Product", productId);
            }
            catch (Exception ex)
            {
                await _fileService.LogApplicationErrorAsync($"Failed to check product existence {productId}", ex.ToString());
                return false;
            }
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            var allProducts = await GetAllProductsAsync();
            return allProducts.Where(p => p.IsActive).ToList();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            var allProducts = await GetAllProductsAsync();
            return allProducts.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}


