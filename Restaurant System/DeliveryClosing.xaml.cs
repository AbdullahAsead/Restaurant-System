using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Restaurant_System
{
    public partial class DeliveryClosing : Window
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

        public DeliveryClosing()
        {
            InitializeComponent();
            _context = new RestaurantDbContext();
            LoadDeliveryEmployees();
        }

        private void LoadDeliveryEmployees()
        {
            try
            {
                var deliveries = _context.DeliveryPeople.ToList();
                DeliveryComboBox.ItemsSource = deliveries;
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل عمال التوصيل:\n" + ex.Message);
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
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Where(o => o.DeliveryEmployeeId == deliveryId)
                .OrderByDescending(o => o.Date)
                .ToList();

            var displayList = _deliveryOrders.Select(o => new OrderDisplay { Order = o }).ToList();
            OrdersDataGrid.ItemsSource = displayList;

            OrdersCountText.Text = displayList.Count.ToString();
            decimal totalOrdersOnly = displayList.Sum(o => o.TotalPrice);
            decimal totalDeliveryFees = displayList.Sum(o => o.DeliveryFee);
            decimal totalShift = totalOrdersOnly + totalDeliveryFees;
            TotalAmountText.Text = totalShift.ToString("0.00") + " ج.م";
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderDisplay od)
            {
                var detailsWindow = new OrderDetailsWindow(od.Order.Id);
                detailsWindow.ShowDialog();
            }
        }

        private void PrintDeliveryReport()
        {
            var displayed = OrdersDataGrid.ItemsSource as IEnumerable<OrderDisplay>;
            var listToPrint = displayed?.ToList() ?? new List<OrderDisplay>();

            if (!listToPrint.Any())
            {
                MessageBox.Show("لا توجد طلبات للطباعة!");
                return;
            }

            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                FlowDirection = FlowDirection.RightToLeft,
                TextAlignment = TextAlignment.Center,
                PagePadding = new Thickness(10),
                PageWidth = 270 // مناسب لطابعة حرارية 80mm
            };

            // 🔹 العنوان الرئيسي
            Paragraph title = new Paragraph(new Run("🧾 تقرير تقفيل شيفت الدليفري"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            doc.Blocks.Add(title);

            // 🔹 بيانات العامل
            Paragraph info = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            info.Inlines.Add(new Run($"الاسم: {EmployeeNameText.Text}\n"));
            info.Inlines.Add(new Run($"الهاتف: {EmployeePhoneText.Text}\n"));
            info.Inlines.Add(new Run($"عدد الطلبات: {listToPrint.Count}\n"));
            info.Inlines.Add(new Run($"التاريخ: {DateTime.Now:dd/MM/yyyy  hh:mm tt}"));
            doc.Blocks.Add(info);

            doc.Blocks.Add(new Paragraph(new Run(new string('═', 42)))
            {
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray
            });

            decimal totalOrdersOnly = 0m;
            decimal totalDeliveryFees = 0m;

            foreach (var d in listToPrint)
            {
                var o = d.Order;
                totalOrdersOnly += d.TotalPrice;
                totalDeliveryFees += d.DeliveryFee;

                // ✅ نستخدم System.Windows.Documents.Table بشكل صريح
                var table = new System.Windows.Documents.Table
                {
                    CellSpacing = 0,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Margin = new Thickness(0, 5, 0, 10)
                };

                table.Columns.Add(new TableColumn { Width = new GridLength(120) });
                table.Columns.Add(new TableColumn { Width = new GridLength(40) });
                table.Columns.Add(new TableColumn { Width = new GridLength(60) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);

                // 🔹 رقم الأوردر
                var header = new TableRow();
                header.Cells.Add(new TableCell(new Paragraph(new Run($"رقم الطلب: {o.Id}"))
                {
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold
                })
                {
                    ColumnSpan = 3,
                    Background = Brushes.LightGray,
                    Padding = new Thickness(2)
                });
                group.Rows.Add(header);

                // 🔸 بيانات العميل
                var customerRow = new TableRow();
                customerRow.Cells.Add(new TableCell(new Paragraph(new Run($"العميل: {o.Customer?.Name ?? "-"}"))
                {
                    TextAlignment = TextAlignment.Center
                })
                { ColumnSpan = 3, Padding = new Thickness(2) });
                group.Rows.Add(customerRow);

                var addressRow = new TableRow();
                addressRow.Cells.Add(new TableCell(new Paragraph(new Run($"العنوان: {o.Customer?.Address ?? "-"}"))
                {
                    TextAlignment = TextAlignment.Center
                })
                { ColumnSpan = 3, Padding = new Thickness(2) });
                group.Rows.Add(addressRow);

                // 🔹 رؤوس الأعمدة
                var head = new TableRow();
                string[] headers = { "الصنف", "الكمية", "السعر" };
                foreach (string h in headers)
                {
                    head.Cells.Add(new TableCell(new Paragraph(new Run(h))
                    {
                        TextAlignment = TextAlignment.Center,
                        FontWeight = FontWeights.Bold
                    })
                    { Background = Brushes.AliceBlue, BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Gray });
                }
                group.Rows.Add(head);

                // 🔸 الأصناف
                foreach (var item in o.OrderItems)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Item?.Name ?? item.ItemName ?? "-")) { TextAlignment = TextAlignment.Center })
                    { BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Gray });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center })
                    { BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Gray });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Price.ToString("0.00"))) { TextAlignment = TextAlignment.Center })
                    { BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Gray });
                    group.Rows.Add(row);
                }

                // 🔹 الإجماليات
                var totalRow = new TableRow();
                totalRow.Cells.Add(new TableCell(new Paragraph(new Run($"الإجمالي: {d.TotalPrice:0.00} ج.م"))
                { TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold })
                { ColumnSpan = 3, Padding = new Thickness(2) });
                group.Rows.Add(totalRow);

                var deliveryRow = new TableRow();
                deliveryRow.Cells.Add(new TableCell(new Paragraph(new Run($"التوصيل: {d.DeliveryFee:0.00} ج.م"))
                { TextAlignment = TextAlignment.Center })
                { ColumnSpan = 3, Padding = new Thickness(2) });
                group.Rows.Add(deliveryRow);

                var finalRow = new TableRow();
                finalRow.Cells.Add(new TableCell(new Paragraph(new Run($"الإجمالي مع التوصيل: {d.TotalWithDelivery:0.00} ج.م"))
                {
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue
                })
                { ColumnSpan = 3, Padding = new Thickness(2) });
                group.Rows.Add(finalRow);

                doc.Blocks.Add(table);

                // 🔸 فاصل بين الطلبات
                doc.Blocks.Add(new Paragraph(new Run(new string('─', 45)))
                {
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Gray
                });
            }

            // 🔷 إجمالي الشيفت
            decimal totalShift = totalOrdersOnly + totalDeliveryFees;
            Paragraph summary = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = Brushes.DarkGreen
            };
            summary.Inlines.Add(new Run($"إجمالي الأوردرات: {totalOrdersOnly:0.00} ج.م\n"));
            summary.Inlines.Add(new Run($"إجمالي التوصيل: {totalDeliveryFees:0.00} ج.م\n"));
            summary.Inlines.Add(new Run($"إجمالي الشيفت: {totalShift:0.00} ج.م"));
            doc.Blocks.Add(summary);

            // 🖨️ تنفيذ الطباعة
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "تقرير شيفت دليفري");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDeliveryReport();
        }
    }
}
