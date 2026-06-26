using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_System.Models
{
    public class OrderItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderSummaryViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string OrderType { get; set; }
        public string CustomerName { get; set; }
        public string TableId { get; set; }
        public string DeliveryPerson { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
    }
}
