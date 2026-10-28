using Microsoft.AspNetCore.Mvc;
using ABCRetailAzureApp.Models;
using ABCRetailAzureApp.Services;

namespace ABCRetailAzureApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return View(customers.OrderBy(c => c.LastName).ToList());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading customers: " + ex.Message;
                return View(new List<CustomerProfile>());
            }
        }

        
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading customer details: " + ex.Message;
                return View();
            }
        }

        
        public IActionResult Create()
        {
            return View();
        }
        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,Address,City,Country,DateOfBirth,PreferredCategory,ReceiveMarketing")] CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (customer.DateOfBirth.HasValue)
                    {
                        customer.DateOfBirth = DateTime.SpecifyKind(customer.DateOfBirth.Value, DateTimeKind.Utc);
                    }

                    
                    customer.RegistrationDate = DateTime.SpecifyKind(customer.RegistrationDate, DateTimeKind.Utc);

                    await _customerService.CreateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error creating customer: " + ex.Message;
                }
            }

            return View(customer);
        }


        
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading customer for edit: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PartitionKey,RowKey,FirstName,LastName,Email,PhoneNumber,Address,City,Country,DateOfBirth,PreferredCategory,ReceiveMarketing,RegistrationDate,ETag")] CustomerProfile customer)
        {
            if (id != customer.RowKey)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error updating customer: " + ex.Message;
                }
            }

            return View(customer);
        }

        
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading customer for deletion: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting customer: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}