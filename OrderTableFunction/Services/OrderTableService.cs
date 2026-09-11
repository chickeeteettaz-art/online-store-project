using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using OrderTableFunction.Models;

namespace OrderTableFunction.Services
{
    public class OrderTableService
    {
        private readonly TableClient _tableClient;

        public OrderTableService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            var tableName = configuration.GetSection("AzureStorage:OrderTableName").Value;

            var tableService = new TableServiceClient(connectionString);
            _tableClient = tableService.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task AddOrUpdateOrderEntity(OrderEntity order)
        {
            await _tableClient.UpsertEntityAsync(order);
        }

        public async Task<OrderEntity> GetOrderEntityAsync(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<OrderEntity>(partitionKey, rowKey);
            return response.Value;
        }

        public async Task<List<OrderEntity>> GetAllOrderEntitiesAsync()
        {
            var entities = new List<OrderEntity>();
            await foreach (var entity in _tableClient.QueryAsync<OrderEntity>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        public async Task DeleteOrderEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
