using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using ProductTableFunction.Models;
using ProductTableFunction.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ProductTableFunction
{
    public class ProductFunction
    {
        private readonly ProductTableService _productTableService;

        public ProductFunction(ProductTableService productTableService)
        {
            _productTableService = productTableService;
        }

        [Function("Create")]
        public async Task<IActionResult> CreateNewProduct([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "products/create")] HttpRequest req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if(string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult("Request body cannot be empty.");
                }

                //Deserialize the request body into a ProductEntity object
                var product = JsonSerializer.Deserialize<ProductEntity>(requestBody);

                if(product == null)
                {
                    return new BadRequestObjectResult("Invalid request body.");
                }

                await _productTableService.AddorUpdateProductEntityAsync(product);
                return new OkObjectResult(new { message = "Product created or updated successfully." });


            }
            catch(JsonException ex)
            {
                return new BadRequestObjectResult($"Invalid JSON format: {ex.Message}");
            }
            

        }

        [Function("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route ="products/update")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body cannot be empty.");
            }
            var product = JsonSerializer.Deserialize<ProductEntity>(requestBody);

            await _productTableService.AddorUpdateProductEntityAsync(product);
            return new OkObjectResult(new { Status = "Success", Message = "Product updated successfully." });   

        }
        

        [Function("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "products/getproducts")] HttpRequest req)
        {

            var products = await _productTableService.GetAllProductEntitiesAsync();
            return new OkObjectResult(products);
        }

        [Function("GetProductFunction")]
        public async Task<IActionResult> GetProduct([HttpTrigger(AuthorizationLevel.Anonymous,"GET",Route ="products/getproduct")] HttpRequest req)
        {
            try
            {
                
                string? partitionKey = req.Query["partitionKey"];
                string? rowKey = req.Query["rowKey"];
                if(string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                {
                    return new BadRequestObjectResult("Both partitionKey and rowKey are required.");
                }   

                var product = await _productTableService.GetProductEntityAsync(partitionKey, rowKey);
                return new OkObjectResult(product);
            }
            catch(JsonException ex)
            {
                return new BadRequestObjectResult($"Invalid JSON format: {ex.Message}");
            }
            
        }
        [Function("DeleteProduct")]
        public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Anonymous,"DELETE",Route ="products/delete")] HttpRequest req)
        {
            try
            {
                string? partitionKey = req.Query["partitionKey"];
                string? rowKey = req.Query["rowKey"];
                if(partitionKey == null || rowKey == null)
                {
                    return new BadRequestObjectResult("The row key and partition key cannot be empty");
                }
                await _productTableService.DeleteProductEntityAsync(partitionKey, rowKey);
                return new OkObjectResult(new { Status = "Success" });
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult($"Operation failed. Error message: {ex.Message}");
            }
            


        }
    }
}
