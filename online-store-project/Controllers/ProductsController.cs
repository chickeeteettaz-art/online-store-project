using Microsoft.AspNetCore.Mvc;
using online_store_project.Models;
using online_store_project.Services.BlobStorageServices;
using online_store_project.Services.QueueServices;
using online_store_project.Services.TableServices;

namespace online_store_project.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductTableService _tableService;
        private readonly IBlobStorageService _blobService;
        private readonly QueueService _queueService;

        public ProductsController(ProductTableService tableService, IBlobStorageService blobService, QueueService queueService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = _tableService.GetAllProductEntitiesAsync().Result;
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Details(string partitionKey, string rowKey)
        {
            var product = _tableService.GetProductEntityAsync(partitionKey, rowKey).Result;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductEntity productEntity,IFormFile ProductImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    productEntity.PartitionKey = "Products";
                    productEntity.RowKey = Guid.NewGuid().ToString();

                    if(ProductImage != null && ProductImage.Length > 0)
                    {
                        var imageUrl = await _blobService.UploadFileAsync(ProductImage);
                        productEntity.ProductImageUrl = imageUrl;
                    }

                    await _tableService.AddorUpdateProductEntityAsync(productEntity);
                    await _queueService.SendMessageAsync(new QueueMessage
                    {
                        Type = "ProductCreated",                        
                        ProductId = productEntity.RowKey,
                        CreatedAt = DateTime.UtcNow,
                        CustomerId = "AdminUser",
                        Quantity = productEntity.StockQuantity,
                        TotalPrice = productEntity.ProductPrice * productEntity.StockQuantity,
                        AdditionalInfo = $"Product '{productEntity.ProductName}' created with quantity {productEntity.StockQuantity}."

                    });
                    return RedirectToAction("Index");

                }
                return View(productEntity);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the product: {ex.Message}");
                return View(productEntity);
            }
        }

        public IActionResult Edit(string partitionKey, string rowKey)
        {
            var entity = _tableService.GetProductEntityAsync(partitionKey, rowKey).Result;
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProductEntity entity, IFormFile? ProductImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Handle new image upload (only if user selected a new file)
                    if (ProductImage != null && ProductImage.Length > 0)
                    {
                        var imageUrl = await _blobService.UploadFileAsync(ProductImage);
                        entity.ProductImageUrl = imageUrl;
                    }
                    // else: keep the existing ProductImageUrl that was posted via the hidden field

                    await _tableService.AddorUpdateProductEntityAsync(entity);
                    return RedirectToAction("Index");
                }

                return View("Edit", entity);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while updating the product: {ex.Message}");
                return View("Edit", entity);
            }
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                await _tableService.DeleteProductEntityAsync(partitionKey, rowKey);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting the borrower: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}
