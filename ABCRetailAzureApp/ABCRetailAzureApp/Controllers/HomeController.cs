using Microsoft.AspNetCore.Mvc;
using ABCRetailAzureApp.Models;
using ABCRetailAzureApp.Models.ViewModels;
using ABCRetailAzureApp.Services;
using System.Diagnostics;

namespace ABCRetailAzureApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public HomeController(ICustomerService customerService, IProductService productService)
        {
            _customerService = customerService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var products = await _productService.GetAllProductsAsync();

                var viewModel = new HomeViewModel
                {
                    TotalCustomers = customers.Count,
                    TotalProducts = products.Count,
                    ActiveProducts = products.Count(p => p.IsActive),
                    RecentProducts = products.OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
                    RecentCustomers = customers.OrderByDescending(c => c.RegistrationDate).Take(5).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading dashboard data: " + ex.Message;
                return View(new HomeViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}