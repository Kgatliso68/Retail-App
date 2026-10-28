using ABCRetailAzureApp.Models;
using ABCRetailAzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using ABCRetailAzureApp.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ABCRetailAzureApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public OrdersController(IOrderService orderService, ICustomerService customerService, IProductService productService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
        }

        
        public async Task<IActionResult> Index(string customerId = "")
        {
            IEnumerable<Order> orders;

            if (string.IsNullOrEmpty(customerId))
            {
                
                orders = await _orderService.GetAllOrdersAsync();
            }
            else
            {
                
                orders = await _orderService.GetCustomerOrdersAsync(customerId);
            }

            ViewBag.CustomerId = customerId;
            return View(orders);
        }

        public async Task<IActionResult> All()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders); 
        }



        
        public async Task<IActionResult> Details(string orderId, string customerId)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(customerId))
                return BadRequest();

            var order = await _orderService.GetOrderAsync(orderId, customerId);
            if (order == null)
                return NotFound();

            return View(order);
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string orderId, string customerId, OrderStatus status)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(customerId))
                return BadRequest();

            var success = await _orderService.UpdateOrderStatusAsync(orderId, status, customerId);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Details), new { orderId, customerId });
        }

        
        [HttpPost]
        public async Task<IActionResult> Cancel(string orderId, string customerId)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(customerId))
                return BadRequest();

            var success = await _orderService.CancelOrderAsync(orderId, customerId);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index), new { customerId });
        }

        
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string orderId, string customerId)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(customerId))
                return BadRequest();

            await _orderService.ProcessOrderPaymentAsync(orderId, customerId);
            return RedirectToAction(nameof(Details), new { orderId, customerId });
        }

        
        [HttpPost]
        public async Task<IActionResult> Fulfill(string orderId, string customerId)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(customerId))
                return BadRequest();

            await _orderService.FulfillOrderAsync(orderId, customerId);
            return RedirectToAction(nameof(Details), new { orderId, customerId });
        }

        
        
        public async Task<IActionResult> Create()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var products = await _productService.GetAllProductsAsync();

            var viewModel = new CreateOrderViewModel
            {
                CustomerId = "",
                OrderItems = products.Where(p => p.IsActive).Select(p => new OrderItemViewModel
                {
                    ProductId = p.RowKey,          
                    ProductName = p.ProductName,   
                    Price = p.Price,
                    Quantity = 0
                }).ToList()
            };

            
            ViewBag.Customers = new SelectList(customers, "RowKey", "FullName");
            ViewBag.Products = products.Where(p => p.IsActive).ToList();

            return View(viewModel);
        }




        
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            async Task LoadDropdowns()
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var products = await _productService.GetAllProductsAsync();
                ViewBag.Customers = new SelectList(customers, "RowKey", "FullName");
                ViewBag.Products = products.Where(p => p.IsActive).ToList();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            try
            {
                var order = new Order(model.CustomerId, Guid.NewGuid().ToString())
                {
                    CustomerId = model.CustomerId,
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod,
                    Items = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                for (int i = 0; i < model.OrderItems.Count; i++)
                {
                    var item = model.OrderItems[i];
                    if (item.Quantity <= 0) continue;

                    var product = await _productService.GetProductAsync(item.ProductId);
                    if (product == null) continue;

                    if (item.Quantity > product.StockQuantity)
                    {
                        ModelState.AddModelError("", $"Product '{product.ProductName}' has only {product.StockQuantity} in stock. You selected {item.Quantity}.");
                        await LoadDropdowns();
                        return View(model);
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.RowKey,
                        ProductName = product.ProductName,
                        Quantity = item.Quantity,
                        Price = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };

                    order.Items.Add(orderItem);
                    totalAmount += orderItem.TotalPrice; 
                }

                if (totalAmount <= 0)
                {
                    ModelState.AddModelError("", "Order must include at least one product with quantity greater than 0.");
                    await LoadDropdowns();
                    return View(model);
                }

                order.TotalAmount = totalAmount;

                var orderId = await _orderService.CreateOrderAsync(order);

                TempData["SuccessMessage"] = $"Order {orderId} created successfully!";
                return RedirectToAction("Details", new { orderId, customerId = model.CustomerId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating order: " + ex.Message);
                await LoadDropdowns();
                return View(model);
            }
        }


    }

}
