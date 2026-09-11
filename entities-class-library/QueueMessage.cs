using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entities_class_library
{
    public class QueueMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string? PopReceipt { get; set; }          // ← needed to delete the message
        public string Type { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AdditionalInfo { get; set; }
    }
}
