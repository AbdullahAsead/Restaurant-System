using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_System.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsDelivery { get; set; }
        public decimal TotalPrice { get; set; }

        public int? TableId { get; set; }
        [ForeignKey("TableId")]
        public Table Table { get; set; }

        public int? DeliveryEmployeeId { get; set; }
        [ForeignKey("DeliveryEmployeeId")]
        public DeliveryEmployee? DeliveryEmployee { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public string? CustomerName { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal DeliveryFee { get; set; } = 0m;
        public bool IsCleared { get; set; } = false;
        public string OrderType { get; set; } = "Takeaway"; // أو "Delivery"
    }
}
