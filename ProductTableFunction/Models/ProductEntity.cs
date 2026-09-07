using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductTableFunction.Models
{
    public class ProductEntity:ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductImageUrl { get; set; }
        public int ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public int StockQuantity { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
