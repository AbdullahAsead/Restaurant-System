using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using Restaurant_System.Models;
using System.Linq;
using System.Windows;

namespace Restaurant_System
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly RestaurantDbContext _context;
        private readonly int _orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _context = new RestaurantDbContext();
            _orderId = orderId;
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            var order = _context.Orders
                .Include(o => o.Customer) // ✅ عشان نجيب بيانات العميل
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefault(o => o.Id == _orderId);

            if (order == null)
            {
                MessageBox.Show("لم يتم العثور على الطلب.");
                return;
            }

            // ✅ عرض بيانات العميل في TextBlocks
            CustomerNameText.Text = order.Customer?.Name ?? "غير متوفر";
            CustomerAddressText.Text = order.Customer?.Address ?? "غير متوفر";

            // ✅ عرض تفاصيل الأصناف
            var items = order.OrderItems.Select(i => new
            {
                ItemName = i.Item?.Name ?? i.ItemName,
                Quantity = i.Quantity,
                Price = i.Price,
                Total = i.Quantity * i.Price
            }).ToList();

            ItemsDataGrid.ItemsSource = items;
        }
    }
}
