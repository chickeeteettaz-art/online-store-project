using Microsoft.AspNetCore.Mvc;
using online_store_project.Models;
using online_store_project.Services.QueueServices;
using online_store_project.Services.TableServices;

namespace online_store_project.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderTableService _orderTableService;
        private readonly QueueService _queueService;

        public OrdersController(OrderTableService orderTableService, QueueService queueService)
        {
            _orderTableService = orderTableService;
            _queueService = queueService;
        }

        // GET: /Orders
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderTableService.GetAllOrderEntitiesAsync();
            return View(orders);
        }

        // GET: /Orders/Details?partitionKey=...&rowKey=...
        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            var order = await _orderTableService.GetOrderEntityAsync(partitionKey, rowKey);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: /Orders/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderEntity order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set required Azure Table keys
                    order.PartitionKey = "Orders";
                    order.RowKey = Guid.NewGuid().ToString();

                    // Optional: set default values
                    order.OrderDate = DateTime.UtcNow;
                    order.Status = "Pending";   

                    await _orderTableService.AddorUpdateOrderEntityAsync(order);

                    //saving message to storage queue
                    var queueMessage = new QueueMessage
                    {
                        Type = "OrderCreated",
                        OrderId = order.RowKey,
                        CustomerId = order.CustomerId,
                        ProductId = order.ProductId,
                        Quantity = order.Quantity,
                        TotalPrice = order.TotalPrice
                    };

                    await _queueService.SendMessageAsync(queueMessage);


                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the order: {ex.Message}");
                return View(order);
            }
        }

        // GET: /Orders/Edit?partitionKey=...&rowKey=...
        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            var order = await _orderTableService.GetOrderEntityAsync(partitionKey, rowKey);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: /Orders/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderEntity order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _orderTableService.AddorUpdateOrderEntityAsync(order);
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while updating the order: {ex.Message}");
                return View(order);
            }
        }

        // POST: /Orders/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                    return BadRequest();

                await _orderTableService.DeleteOrderEntityAsync(partitionKey, rowKey);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // You can log the error here
                TempData["ErrorMessage"] = $"An error occurred while deleting the order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}