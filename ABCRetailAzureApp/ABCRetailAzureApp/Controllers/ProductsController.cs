using Microsoft.AspNetCore.Mvc;
using ABCRetailAzureApp.Models;
using ABCRetailAzureApp.Models.ViewModels;
using ABCRetailAzureApp.Services;

namespace ABCRetailAzureApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();

                
                foreach (var product in products)
                {
                    Console.WriteLine($"Product: {product.ProductName}, Price: {product.Price}");
                }

                return View(products.OrderBy(p => p.ProductName).ToList());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading products: " + ex.Message;
                return View(new List<Product>());
            }
        }

        
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _productService.GetProductAsync(id);
                if (product == null)
                    return NotFound();

                
                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading product details: " + ex.Message;
                return View();
            }
        }

 

        
        public IActionResult Create()
        {
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.CreateProductAsync(model.Product, model.ImageFile);
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error creating product: " + ex.Message;
                    return View(model);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> FixImageUrls()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var storageAccountName = "st10497868storage"; 
                var containerName = "productimages";

                foreach (var product in products)
                {
                    
                    if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.StartsWith("https://"))
                    {
                        
                        var fullUrl = $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{product.ImageUrl}";

                        
                        product.ImageUrl = fullUrl;
                        await _productService.UpdateProductAsync(product, null); 

                        Console.WriteLine($"Fixed URL for product {product.ProductName}: {fullUrl}");
                    }
                }

                TempData["SuccessMessage"] = "Image URLs have been fixed!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error fixing URLs: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _productService.GetProductAsync(id);
                if (product == null)
                    return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading product for deletion: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting product: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
