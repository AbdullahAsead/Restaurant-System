using Restaurant_System.Data;
using Restaurant_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Restaurant_System
{
    public partial class DeliveryClosingForAdmin : Window
    {
        private readonly RestaurantDbContext _context;
        private List<Order> _deliveryOrders = new();

        private class OrderDisplay
        {
            public Order Order { get; set; } = null!;
            public int Id => Order.Id;
            public Customer? Customer => Order.Customer;
            public decimal TotalPrice => Order.OrderItems?.Sum(i => i.Quantity * i.Price) ?? 0m;
            public decimal DeliveryFee => Order.DeliveryFee;
            public decimal TotalWithDelivery => TotalPrice + DeliveryFee;
            public DateTime Date => Order.Date;
        }

        public DeliveryClosingForAdmin()
        {
            InitializeComponent();
            _context = new RestaurantDbContext();
            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            try
            {
                var list = _context.DeliveryPeople.ToList();
                DeliveryComboBox.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل عمال التوصيل:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeliveryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeliveryComboBox.SelectedValue == null) return;

            int deliveryId = (int)DeliveryComboBox.SelectedValue;
            var delivery = _context.DeliveryPeople.FirstOrDefault(d => d.Id == deliveryId);
            if (delivery == null) return;

            EmployeeNameText.Text = delivery.Name;
            EmployeePhoneText.Text = delivery.PhoneNumber;

            _deliveryOrders = _context.Orders
                .Where(o => o.DeliveryEmployeeId == deliveryId)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.Date)
                .ToList();

            var displayList = _deliveryOrders.Select(o => new OrderDisplay { Order = o }).ToList();
            OrdersDataGrid.ItemsSource = displayList;

            decimal totalOrdersOnly = displayList.Sum(d => d.TotalPrice);
            decimal totalDeliveryFees = displayList.Sum(d => d.DeliveryFee);
            decimal totalShift = totalOrdersOnly + totalDeliveryFees;

            OrdersCountText.Text = displayList.Count.ToString();
            TotalAmountText.Text = totalShift.ToString("0.00") + " ج.م";
        }

        private void OrdersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is OrderDisplay sel)
                new OrderDetailsWindow(sel.Order.Id).ShowDialog();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e) => PrintDeliveryReport();

        private void PrintDeliveryReport()
        {
            var displayed = OrdersDataGrid.ItemsSource as IEnumerable<OrderDisplay>;
            var listToPrint = displayed?.ToList() ?? new List<OrderDisplay>();
            if (!listToPrint.Any())
            {
                MessageBox.Show("لا توجد طلبات للطباعة!", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                FlowDirection = FlowDirection.RightToLeft,
                TextAlignment = TextAlignment.Center,
                PagePadding = new Thickness(10),
                PageWidth = 272,
                PageHeight = 1122
            };

            doc.Blocks.Add(new Paragraph(new Run("🧾 تقرير تقفيل شيفت الدليفري (الأدمن)"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"الاسم: {EmployeeNameText.Text}\nالهاتف: {EmployeePhoneText.Text}\nعدد الطلبات: {listToPrint.Count}\nالتاريخ: {DateTime.Now:dd/MM/yyyy hh:mm tt}"
            ))
            {
                FontSize = 13,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            decimal totalOrdersOnly = listToPrint.Sum(o => o.TotalPrice);
            decimal totalDeliveryFees = listToPrint.Sum(o => o.DeliveryFee);
            decimal totalShift = totalOrdersOnly + totalDeliveryFees;

            Paragraph summary = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.DarkGreen,
                Margin = new Thickness(0, 10, 0, 0)
            };
            summary.Inlines.Add(new Run($"إجمالي الأوردرات: {totalOrdersOnly:0.00} ج.م\n"));
            summary.Inlines.Add(new Run($"مجموع التوصيل: {totalDeliveryFees:0.00} ج.م\n"));
            summary.Inlines.Add(new Run($"إجمالي الشيفت الكلي: {totalShift:0.00} ج.م"));
            doc.Blocks.Add(summary);

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idp = doc;
                pd.PrintDocument(idp.DocumentPaginator, "تقرير شيفت دليفري (الأدمن)");
            }
        }

        private void ResetSingleDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (DeliveryComboBox.SelectedValue == null)
            {
                MessageBox.Show("من فضلك اختر موظف أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int deliveryId = (int)DeliveryComboBox.SelectedValue;

            // ✅ إعادة تعيين عدد الطلبات للمندوب
            var delivery = _context.DeliveryPeople.FirstOrDefault(d => d.Id == deliveryId);
            if (delivery != null)
            {
                delivery.DailyOrdersCount = 0;
            }

            // ✅ جعل الطلبات الخاصة به غير مرتبطة به (null)
            var orders = _context.Orders.Where(o => o.DeliveryEmployeeId == deliveryId).ToList();
            foreach (var order in orders)
                order.DeliveryEmployeeId = null;

            _context.SaveChanges();

            MessageBox.Show("تم تصفير بيانات هذا المندوب بنجاح ✅", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
            DeliveryComboBox_SelectionChanged(null, null);
        }

        private void ResetAllDeliveries_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("هل أنت متأكد من تصفير جميع الموظفين؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // ✅ تصفير عدد الطلبات لكل المندوبين
                foreach (var d in _context.DeliveryPeople)
                    d.DailyOrdersCount = 0;

                // ✅ جعل كل الطلبات غير مرتبطة بأي مندوب
                foreach (var o in _context.Orders)
                    o.DeliveryEmployeeId = null;

                _context.SaveChanges();

                MessageBox.Show("تم تصفير جميع بيانات الدليفري بنجاح 🔥", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
                OrdersDataGrid.ItemsSource = null;
                OrdersCountText.Text = "0";
                TotalAmountText.Text = "0.00 ج.م";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Close();
    }
}
