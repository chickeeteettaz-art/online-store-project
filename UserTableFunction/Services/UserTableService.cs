using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UserTableFunction.Model;

namespace UserTableFunction.Services
{
    public class UserTableService
    {
        private readonly TableClient _tableClient;

        public UserTableService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            var tableName = configuration.GetSection("AzureStorage:TableName").Value;
            
            var tableServiceClient = new TableServiceClient(connectionString);
            _tableClient = tableServiceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task AddOrUpdateUserEntity(CustomerEntity customer)
        {
            await _tableClient.UpsertEntityAsync(customer);
        }
        public async Task<CustomerEntity> GetCustomerEntityAsync(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<CustomerEntity>(partitionKey, rowKey);
            return response.Value;
        }
        public async Task<List<CustomerEntity>> GetAllCustomerEntitiesAsync()
        {
            var entities = new List<CustomerEntity>();
            await foreach (var entity in _tableClient.QueryAsync<CustomerEntity>())
            {
                entities.Add(entity);
            }
            return entities;
        }
        public async Task DeleteCustomerEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
