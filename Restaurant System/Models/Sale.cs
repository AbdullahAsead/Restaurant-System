using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_System.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public string SaleDate { get; set; }

        // ✅ لو السطر ده يمثل رسوم توصيل
        public decimal DeliveryFee { get; set; } = 0m;
        public bool IsCleared { get; set; } = false;
        public int? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public string OrderType { get; set; }

    }
}
