using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using OrderTableFunction.Models;
using OrderTableFunction.Services;

namespace OrderTableFunction
{
    public class OrderFunction
    {
        private readonly OrderTableService _orderTableService;

        public OrderFunction(OrderTableService orderTableService)
        {
            _orderTableService = orderTableService;
        }

        [Function("CreateOrder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route ="orders/create")] HttpRequest req)
        {
            if (req.Method == HttpMethods.Post)
            {
                var order = await req.ReadFromJsonAsync<OrderEntity>();
                if (order != null)
                {
                    await _orderTableService.AddOrUpdateOrderEntity(order);
                    return new OkObjectResult("Order added or updated successfully.");
                }
                return new BadRequestObjectResult("Invalid order data.");
            }
            else if (req.Method == HttpMethods.Get)
            {
                var orders = await _orderTableService.GetAllOrderEntitiesAsync();
                return new OkObjectResult(orders);
            }
            return new BadRequestObjectResult("Unsupported HTTP method.");
        }

        [Function("GetOrders")]
        public async Task<IActionResult> GetOrders([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/getall")] HttpRequest req)
        {
            var orders = await _orderTableService.GetAllOrderEntitiesAsync();
            return new OkObjectResult(orders);
        }

        [Function("GetOrder")]
        public async Task<IActionResult> GetOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous,"GET",Route = "orders/getorder")] HttpRequest httpRequest)
        {
            string? partitionKey = httpRequest.Query["partitionKey"];
            string? rowKey = httpRequest.Query["rowKey"];
            if(partitionKey == null || rowKey == null)
            {
                return new BadRequestObjectResult("Missing partitionKey or rowKey in query parameters.");
            }

            var order = await _orderTableService.GetOrderEntityAsync(partitionKey, rowKey);
            return new OkObjectResult(order);
        }

        [Function("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous,"DELETE",Route = "orders/delete")] HttpRequest httpRequest)
        {
            string? partitionKey = httpRequest.Query["partitionKey"];
            string? rowKey = httpRequest.Query["rowKey"];
            if(partitionKey == null || rowKey == null)
            {
                return new BadRequestObjectResult("Missing partitionKey or rowKey in query parameters.");
            }

            await _orderTableService.DeleteOrderEntityAsync(partitionKey, rowKey);
            return new OkObjectResult("Order deleted successfully.");
        }
    }
}
