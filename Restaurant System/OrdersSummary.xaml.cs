using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using Restaurant_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Restaurant_System.Helpers;
using System.Data.SQLite;

namespace Restaurant_System
{
    public partial class OrdersSummary : Window
    {
        public OrdersSummary()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            using var db = new RestaurantDbContext();

            var orders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.DeliveryEmployee)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .ToList()
                .Select(order => new OrderSummaryViewModel
                {
                    Id = order.Id,
                    Date = order.Date,
                    OrderType = order.IsDelivery ? "توصيل" : "طاولة",
                    CustomerName = order.Customer?.Name ?? "—",
                    TableId = order.TableId?.ToString() ?? "—",
                    DeliveryPerson = order.DeliveryEmployee?.Name ?? "—",
                    TotalPrice = order.TotalPrice,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ItemName = oi.Item?.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                }).ToList();

            OrdersGrid.ItemsSource = orders;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is OrderSummaryViewModel selectedOrder)
            {
                string message = $"رقم الطلب: {selectedOrder.Id}\n" +
                                 $"التاريخ: {selectedOrder.Date}\n" +
                                 $"نوع الطلب: {selectedOrder.OrderType}\n" +
                                 $"العميل: {selectedOrder.CustomerName}\n" +
                                 $"الطاولة: {selectedOrder.TableId}\n" +
                                 $"الموصل: {selectedOrder.DeliveryPerson}\n" +
                                 $"إجمالي السعر: {selectedOrder.TotalPrice} جنيه\n\n";

                message += "الأصناف:\n";
                foreach (var item in selectedOrder.OrderItems)
                {
                    var subtotal = item.Price * item.Quantity;
                    message += $"- {item.ItemName} × {item.Quantity} = {subtotal} جنيه\n";
                }

                MessageBox.Show(message, "تفاصيل الطلب", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("من فضلك اختر طلبًا لعرض تفاصيله", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is OrderSummaryViewModel selectedOrder)
            {
                ReceiptPrinter.PrintOrder(selectedOrder);
            }
            else
            {
                MessageBox.Show("من فضلك اختر طلبًا لطباعته", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is OrderSummaryViewModel selectedOrder)
            {
                var result = MessageBox.Show("هل أنت متأكد من حذف هذا الطلب؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                        {
                            connection.Open();

                            // ✅ حذف تفاصيل الطلب من جدول OrderItem (الاسم الصحيح)
                            string deleteOrderItemsQuery = "DELETE FROM OrderItem WHERE OrderId = @OrderId";
                            using (var cmdItems = new SQLiteCommand(deleteOrderItemsQuery, connection))
                            {
                                cmdItems.Parameters.AddWithValue("@OrderId", selectedOrder.Id);
                                cmdItems.ExecuteNonQuery();
                            }

                            // حذف الطلب من جدول Orders
                            string deleteOrderQuery = "DELETE FROM Orders WHERE Id = @Id";
                            using (var cmdOrder = new SQLiteCommand(deleteOrderQuery, connection))
                            {
                                cmdOrder.Parameters.AddWithValue("@Id", selectedOrder.Id);
                                cmdOrder.ExecuteNonQuery();
                            }

                            connection.Close();
                        }

                        // تحديث الواجهة بعد الحذف
                        LoadOrders();

                        MessageBox.Show("تم حذف الطلب بنجاح", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("حدث خطأ أثناء حذف الطلب:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("من فضلك اختر طلبًا أولاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
