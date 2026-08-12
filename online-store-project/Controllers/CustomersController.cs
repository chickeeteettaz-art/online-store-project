using Microsoft.AspNetCore.Mvc;
using online_store_project.Models;
using online_store_project.Services.BlobStorageServices;
using online_store_project.Services.TableServices;

namespace online_store_project.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableService tableService;
        private readonly IBlobStorageService blobStorageService;

        public CustomersController(TableService tableService, IBlobStorageService blobStorageService)
        {
            this.tableService = tableService;
            this.blobStorageService = blobStorageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var customers = tableService.GetAllCustomerEntities().Result;
            return View(customers);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Details(string partitionKey, string rowKey)
        {
            var customer = tableService.GetCustomerEntity(partitionKey, rowKey).Result;
            return View(customer);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CustomerEntity customer,IFormFile ProfilePhoto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.PartitionKey = "Customers";
                    customer.RowKey = Guid.NewGuid().ToString();
                    var photoUrl = await blobStorageService.UploadFileAsync(ProfilePhoto,"customers");
                    customer.PhotoUrl = photoUrl;
                    await tableService.AddorUpdateCustomerEntity(customer);
                    return RedirectToAction("Index");
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the customer: {ex.Message}");
                return View(customer);
            }
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = tableService.GetCustomerEntity(partitionKey, rowKey).Result;
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerEntity customer, IFormFile ProfilePhoto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    if(ProfilePhoto != null && ProfilePhoto.Length > 0)
                    {
                        var photoUrl = await blobStorageService.UploadFileAsync(ProfilePhoto, "customers");
                        customer.PhotoUrl = photoUrl;
                    }
                    
                    await tableService.AddorUpdateCustomerEntity(customer);
                    return RedirectToAction("Index");
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the customer: {ex.Message}");
                return View(customer);
            }
        }

        
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                await tableService.DeleteCustomerEntity(partitionKey, rowKey);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting the customer: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

    }
}
