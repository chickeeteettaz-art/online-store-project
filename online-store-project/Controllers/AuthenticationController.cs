using Microsoft.AspNetCore.Mvc;
using online_store_project.Models;
using online_store_project.Services.BlobStorageServices;
using online_store_project.Services.TableServices;

namespace online_store_project.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly TableService tableService;
        private readonly IBlobStorageService blobStorageService;

        public AuthenticationController(TableService tableService, IBlobStorageService blobStorageService)
        {
            this.tableService = tableService;
            this.blobStorageService = blobStorageService;
        }
        public IActionResult Signup() 
        { 
            return View(); 
        }
        public IActionResult UserLogin()
        {
            return View();
        }

        public async Task<IActionResult> Authenticate(string email, string firstName)
        {
            var customers = await tableService.GetAllCustomerEntities();

            foreach (var customer in customers)
            {
                if (customer.Email == email && customer.FirstName == firstName)
                {
                    // Store user info in Session
                    HttpContext.Session.SetString("UserEmail", customer.Email ?? "");
                    HttpContext.Session.SetString("UserFirstName", customer.FirstName ?? "");
                    HttpContext.Session.SetString("IsAdmin",
                        customer.Email == "Admin@outlook.com" ? "true" : "false");

                    if (customer.Email == "Admin@outlook.com")
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToAction("Index", "Products");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or first name.");
            return View("UserLogin");
        }

        // Add this Logout action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("UserLogin");
        }
        [HttpPost]
        public async Task<IActionResult> Create(CustomerEntity customer, IFormFile ProfilePhoto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.PartitionKey = "Customers";
                    customer.RowKey = Guid.NewGuid().ToString();
                    var photoUrl = await blobStorageService.UploadFileAsync(ProfilePhoto, "customers");
                    customer.PhotoUrl = photoUrl;
                    await tableService.AddorUpdateCustomerEntity(customer);
                    return RedirectToAction("UserLogin");
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the customer: {ex.Message}");
                return View(customer);
            }
        }
    }
}
