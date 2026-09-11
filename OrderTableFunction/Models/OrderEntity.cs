using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderTableFunction.Models
{
    public class OrderEntity:ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
