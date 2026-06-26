using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using Restaurant_System.Helpers;
using Restaurant_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class OrderSummaryCashier : Window
    {
        private readonly RestaurantDbContext _context;
        private List<Order> allOrders = new();
        public List<DeliveryEmployee> DeliveryEmployees { get; set; } = new();

        public OrderSummaryCashier()
        {
            InitializeComponent();
            _context = new RestaurantDbContext();

            LoadDeliveryEmployees();
            LoadOrders();
        }

        // تحميل بيانات الطيارين
        private void LoadDeliveryEmployees()
        {
            try
            {
                DeliveryEmployees = _context.DeliveryPeople
                    .OrderBy(d => d.Name)
                    .ToList();

                DeliveryEmployees.Insert(0, new DeliveryEmployee { Id = 0, Name = "— فارغ —" });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تحميل الطيارين:\n{ex.Message}");
            }
        }

        // تحميل الطلبات
        private void LoadOrders()
        {
            try
            {
                allOrders = _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .Include(o => o.DeliveryEmployee)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                    .OrderByDescending(o => o.Date)
                    .ToList();

                var orderView = allOrders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    Date = o.Date,
                    OrderType = o.IsDelivery ? "توصيل" : "طاولة",
                    CustomerName = o.Customer?.Name ?? "—",
                    TableId = o.TableId?.ToString() ?? "—",
                    DeliveryPerson = o.DeliveryEmployee?.Name ?? "—",
                    TotalPrice = o.TotalPrice,
                    OrderItems = o.OrderItems.Select(i => new OrderItemViewModel
                    {
                        ItemName = i.Item?.Name ?? "—",
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                }).ToList();

                OrdersGrid.ItemsSource = orderView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تحميل الطلبات:\n{ex.Message}");
            }
        }

        // إضافة طيار
        private void AddPilot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderSummaryViewModel orderVm)
            {
                var selectWindow = new Window
                {
                    Title = "اختر الطيار",
                    Width = 300,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize
                };

                var stack = new StackPanel { Margin = new Thickness(20) };
                var label = new TextBlock
                {
                    Text = "اختر الطيار:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var combo = new ComboBox
                {
                    ItemsSource = DeliveryEmployees.Where(d => d.Id != 0).ToList(),
                    DisplayMemberPath = "Name",
                    SelectedValuePath = "Id",
                    Width = 220,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var btnSave = new Button
                {
                    Content = "حفظ",
                    Width = 80,
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White
                };

                btnSave.Click += (s, e2) =>
                {
                    if (combo.SelectedValue != null)
                    {
                        int selectedPilotId = (int)combo.SelectedValue;
                        SavePilotToDatabase(orderVm.Id, selectedPilotId);
                        selectWindow.Close();
                    }
                    else
                    {
                        MessageBox.Show("⚠️ اختر الطيار أولاً!");
                    }
                };

                stack.Children.Add(label);
                stack.Children.Add(combo);
                stack.Children.Add(btnSave);
                selectWindow.Content = stack;
                selectWindow.ShowDialog();
            }
        }

        // حفظ الطيار
        private void SavePilotToDatabase(int orderId, int pilotId)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.DeliveryEmployeeId = pilotId;

                    var deliveryPerson = _context.DeliveryPeople.FirstOrDefault(d => d.Id == pilotId);
                    if (deliveryPerson != null)
                        deliveryPerson.DailyOrdersCount += 1;

                    _context.SaveChanges();
                    LoadOrders();
                    MessageBox.Show("✅ تم تعيين الطيار وزيادة عدد الطلبات اليومية بنجاح", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ الطلب غير موجود!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تعيين الطيار:\n{ex.Message}");
            }
        }

        // عرض تفاصيل الطلب
        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderSummaryViewModel order)
            {
                string msg = $"رقم الطلب: {order.Id}\n" +
                             $"التاريخ: {order.Date}\n" +
                             $"النوع: {order.OrderType}\n" +
                             $"العميل: {order.CustomerName}\n" +
                             $"الطاولة: {order.TableId}\n" +
                             $"الإجمالي: {order.TotalPrice}\n" +
                             $"الطيار: {order.DeliveryPerson}";

                MessageBox.Show(msg, "تفاصيل الطلب", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // طباعة الطلب
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

        // البحث
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string nameFilter = CustomerSearchBox.Text.Trim().ToLower();
            string totalFilter = TotalSearchBox.Text.Trim();

            var filtered = allOrders
                .Where(o =>
                    (string.IsNullOrWhiteSpace(nameFilter) ||
                     (o.Customer?.Name?.ToLower().Contains(nameFilter) ?? false))
                    &&
                    (string.IsNullOrWhiteSpace(totalFilter) ||
                     o.TotalPrice.ToString().Contains(totalFilter)))
                .Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    Date = o.Date,
                    OrderType = o.IsDelivery ? "توصيل" : "طاولة",
                    CustomerName = o.Customer?.Name ?? "—",
                    TableId = o.TableId?.ToString() ?? "—",
                    DeliveryPerson = o.DeliveryEmployee?.Name ?? "—",
                    TotalPrice = o.TotalPrice,
                    OrderItems = o.OrderItems.Select(i => new OrderItemViewModel
                    {
                        ItemName = i.Item?.Name ?? "—",
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                }).ToList();

            OrdersGrid.ItemsSource = filtered;
        }

        // مسح الفلاتر
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            CustomerSearchBox.Clear();
            TotalSearchBox.Clear();
            LoadOrders();
        }
    }
}
