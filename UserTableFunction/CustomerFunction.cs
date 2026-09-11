using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using UserTableFunction.Model;
using UserTableFunction.Services;

namespace UserTableFunction
{
    public class CustomerFunction
    {
        private readonly UserTableService _userTableService;
        public CustomerFunction(UserTableService userTableService)
        {
            _userTableService = userTableService;
        }

        [Function("CreateCustomer")]
        public async Task<IActionResult> CreateNewCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route ="customers/create")] HttpRequest httpRequest)
        {
            try
            {
                string requestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();
                if (string.IsNullOrEmpty(requestBody))
                {
                    return new BadRequestObjectResult("Request body cannot be empty.");
                }
                //deserialize the request body into a CustomerEntity object
                var customer = JsonSerializer.Deserialize<CustomerEntity>(requestBody);
                if(customer==null)
                    return new BadRequestObjectResult("Invalid customer data.");

                await _userTableService.AddOrUpdateUserEntity(customer);
                return new OkObjectResult(new { message = "Customer created or updated successfully." });
            }
            catch (JsonException ex)
            {
                return new BadRequestObjectResult($"Invalid JSON format: {ex.Message}");
            }
        }

        [Function("GetCustomer")]
        public async Task<IActionResult> GetCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "customers/get")] HttpRequest httpRequest)
        {
            string? partitionKey = httpRequest.Query["partitionKey"];
            string? rowKey = httpRequest.Query["rowKey"];
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("Both 'partitionKey' and 'rowKey' query parameters are required.");
            }
            var customer = await _userTableService.GetCustomerEntityAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return new NotFoundObjectResult($"Customer with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
            }
            return new OkObjectResult(customer);
        }
        [Function("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "customers/getall")] HttpRequest httpRequest)
        {
            var customers = await _userTableService.GetAllCustomerEntitiesAsync();
            return new OkObjectResult(customers);
        }

        [Function("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "customers/delete")] HttpRequest httpRequest)
        {
            string? partitionKey = httpRequest.Query["partitionKey"];
            string? rowKey = httpRequest.Query["rowKey"];
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("Both 'partitionKey' and 'rowKey' query parameters are required.");
            }
            await _userTableService.DeleteCustomerEntityAsync(partitionKey, rowKey);
            return new OkObjectResult(new { message = "Customer deleted successfully." });
        }

        [Function("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "customers/update")] HttpRequest httpRequest)
        {
            string? partitionKey = httpRequest.Query["partitionKey"];
            string? rowKey = httpRequest.Query["rowKey"];
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("Both 'partitionKey' and 'rowKey' query parameters are required.");
            }
            string requestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body cannot be empty.");
            }
            var customer = JsonSerializer.Deserialize<CustomerEntity>(requestBody);
            if (customer == null)
            {
                return new BadRequestObjectResult("Invalid customer data.");
            }
            await _userTableService.AddOrUpdateUserEntity(customer);
            return new OkObjectResult(new { message = "Customer updated successfully." });
        }
    }
}
