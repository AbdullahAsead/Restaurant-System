using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Restaurant_System
{
    public partial class ClearSalesWindow : Window
    {
        public class SaleViewModel
        {
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        public class DeliveryFeeViewModel
        {
            public int OrderId { get; set; }
            public decimal DeliveryFee { get; set; }
            public string Date { get; set; } = "";
        }

        private List<SaleViewModel> takeawaySales = new();
        private List<SaleViewModel> deliverySales = new();
        private List<DeliveryFeeViewModel> deliveryFees = new();

        public ClearSalesWindow()
        {
            InitializeComponent();
            LoadSalesData();
        }

        private void LoadSalesData()
        {
            try
            {
                using var db = new RestaurantDbContext();

                var allSales = db.Sales
                    .GroupJoin(db.Orders, sale => sale.OrderId, order => order.Id, (sale, orders) => new { sale, orders })
                    .SelectMany(x => x.orders.DefaultIfEmpty(), (x, order) => new
                    {
                        x.sale.ProductName,
                        x.sale.Quantity,
                        x.sale.UnitPrice,
                        x.sale.IsCleared,
                        OrderType = order != null ? order.OrderType : "Takeaway"
                    })
                    .Where(x => x.IsCleared == false)
                    .ToList();

                takeawaySales = allSales
                    .Where(x => x.OrderType == "Takeaway" || x.OrderType == "Hall" || string.IsNullOrEmpty(x.OrderType))
                    .GroupBy(x => x.ProductName)
                    .Select(g => new SaleViewModel
                    {
                        ProductName = g.Key,
                        Quantity = g.Sum(x => x.Quantity),
                        UnitPrice = g.First().UnitPrice
                    })
                    .OrderBy(s => s.ProductName)
                    .ToList();

                deliverySales = allSales
                    .Where(x => x.OrderType == "Delivery")
                    .GroupBy(x => x.ProductName)
                    .Select(g => new SaleViewModel
                    {
                        ProductName = g.Key,
                        Quantity = g.Sum(x => x.Quantity),
                        UnitPrice = g.First().UnitPrice
                    })
                    .OrderBy(s => s.ProductName)
                    .ToList();

                deliveryFees = db.Orders
                    .AsEnumerable()
                    .Where(o => o.DeliveryFee > 0 && o.IsCleared == false)
                    .Select(o => new DeliveryFeeViewModel
                    {
                        OrderId = o.Id,
                        DeliveryFee = o.DeliveryFee,
                        Date = o.Date.ToString("yyyy-MM-dd HH:mm")
                    })
                    .OrderBy(d => d.OrderId)
                    .ToList();

                TakeawayDataGrid.ItemsSource = takeawaySales;
                DeliveryDataGrid.ItemsSource = deliverySales;
                DeliveryFeesDataGrid.ItemsSource = deliveryFees;

                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotals()
        {
            decimal takeawayTotal = takeawaySales.Sum(s => s.Total);
            decimal deliveryProductsTotal = deliverySales.Sum(s => s.Total);
            decimal deliveryFeesTotal = deliveryFees.Sum(d => d.DeliveryFee);
            decimal grandTotal = takeawayTotal + deliveryProductsTotal + deliveryFeesTotal;

            TakeawayTotalText.Text = $"🍽️ إجمالي التيك أواي/الصالة: {takeawayTotal:N2} ج";
            DeliveryTotalText.Text = $"🚚 إجمالي التوصيل (منتجات + رسوم): {deliveryProductsTotal + deliveryFeesTotal:N2} ج";
            GrandTotalText.Text = $"🧾 الإجمالي الكلي: {grandTotal:N2} ج";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("هل تريد تصفير بيانات الشيفت دون حذفها؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = new RestaurantDbContext();
                    db.Sales.Where(s => !s.IsCleared).ToList().ForEach(s => s.IsCleared = true);
                    db.Orders.Where(o => !o.IsCleared).ToList().ForEach(o => o.IsCleared = true);
                    db.SaveChanges();

                    takeawaySales.Clear();
                    deliverySales.Clear();
                    deliveryFees.Clear();

                    TakeawayDataGrid.ItemsSource = null;
                    DeliveryDataGrid.ItemsSource = null;
                    DeliveryFeesDataGrid.ItemsSource = null;
                    UpdateTotals();

                    MessageBox.Show("✅ تم تصفير الشيفت بنجاح (البيانات محفوظة للتقارير)", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء التصفير:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FlowDocument doc = BuildShiftReport();
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    doc.PageWidth = 260;   // تقليل بسيط علشان نحافظ على الحواف
                    doc.PageHeight = 1122; // A4 height
                    doc.ColumnWidth = doc.PageWidth;
                    pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, $"تقرير الشيفت - {DateTime.Now:yyyyMMdd_HHmm}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الطباعة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument BuildShiftReport()
        {
            decimal takeawayTotal = takeawaySales.Sum(s => s.Total);
            decimal deliveryProductsTotal = deliverySales.Sum(s => s.Total);
            decimal deliveryFeesTotal = deliveryFees.Sum(f => f.DeliveryFee);
            decimal grandTotal = takeawayTotal + deliveryProductsTotal + deliveryFeesTotal;

            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                TextAlignment = TextAlignment.Center,
                ColumnWidth = double.PositiveInfinity,
                PagePadding = new Thickness(15, 10, 5, 10), // ← هامش يمين أكبر علشان الحافة تبان
                PageWidth = 260,
                PageHeight = 1122
            };

            doc.Blocks.Add(new Paragraph(new Run($"🕒 التاريخ والوقت: {DateTime.Now:yyyy-MM-dd HH:mm}"))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            doc.Blocks.Add(new Paragraph(new Run("📋 تقرير الشيفت التفصيلي"))
            {
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            AddSalesTable(doc, "🍽️ مبيعات التيك أواي / الصالة", takeawaySales);
            AddSalesTable(doc, "🚚 مبيعات التوصيل", deliverySales);
            AddDeliveryFeesTable(doc, deliveryFees);

            var totalsParagraph = new Paragraph
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            totalsParagraph.Inlines.Add(new Run($"🍽️ إجمالي التيك أواي/الصالة: {takeawayTotal:N2} ج\n"));
            totalsParagraph.Inlines.Add(new Run($"🚚 إجمالي مبيعات التوصيل: {deliveryProductsTotal:N2} ج\n"));
            totalsParagraph.Inlines.Add(new Run($"💸 إجمالي رسوم التوصيل: {deliveryFeesTotal:N2} ج\n"));
            totalsParagraph.Inlines.Add(new Run($"🧾 الإجمالي الكلي: {grandTotal:N2} ج"));
            doc.Blocks.Add(totalsParagraph);

            return doc;
        }

        private void AddSalesTable(FlowDocument doc, string title, List<SaleViewModel> data)
        {
            if (data.Count == 0) return;

            doc.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 5)
            });

            var table = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                TextAlignment = TextAlignment.Left // ← علشان الجدول ميخبطش في الحافة
            };

            table.Columns.Add(new TableColumn { Width = new GridLength(95) });
            table.Columns.Add(new TableColumn { Width = new GridLength(40) });
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });
            table.Columns.Add(new TableColumn { Width = new GridLength(65) });

            TableRowGroup group = new TableRowGroup();

            var header = new TableRow();
            header.Cells.Add(CreateCell("المنتج", true));
            header.Cells.Add(CreateCell("الكمية", true));
            header.Cells.Add(CreateCell("سعر", true));
            header.Cells.Add(CreateCell("الإجمالي", true));
            group.Rows.Add(header);

            foreach (var s in data)
            {
                var row = new TableRow();
                row.Cells.Add(CreateCell(s.ProductName));
                row.Cells.Add(CreateCell(s.Quantity.ToString()));
                row.Cells.Add(CreateCell($"{s.UnitPrice:N2}"));
                row.Cells.Add(CreateCell($"{s.Total:N2}"));
                group.Rows.Add(row);
            }

            var totalRow = new TableRow();
            totalRow.Cells.Add(CreateCell("الإجمالي", true, 3));
            totalRow.Cells.Add(CreateCell($"{data.Sum(d => d.Total):N2}", true));
            group.Rows.Add(totalRow);

            table.RowGroups.Add(group);
            doc.Blocks.Add(table);
        }

        private void AddDeliveryFeesTable(FlowDocument doc, List<DeliveryFeeViewModel> fees)
        {
            if (fees.Count == 0) return;

            doc.Blocks.Add(new Paragraph(new Run("💳 جدول خدمات التوصيل"))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 5)
            });

            var table = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                TextAlignment = TextAlignment.Left
            };

            table.Columns.Add(new TableColumn { Width = new GridLength(70) });
            table.Columns.Add(new TableColumn { Width = new GridLength(60) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });

            TableRowGroup group = new TableRowGroup();

            var header = new TableRow();
            header.Cells.Add(CreateCell("رقم", true));
            header.Cells.Add(CreateCell("توصيل", true));
            header.Cells.Add(CreateCell("تاريخ", true));
            group.Rows.Add(header);

            foreach (var f in fees)
            {
                var row = new TableRow();
                row.Cells.Add(CreateCell(f.OrderId.ToString()));
                row.Cells.Add(CreateCell($"{f.DeliveryFee:N2}"));
                row.Cells.Add(CreateCell(f.Date));
                group.Rows.Add(row);
            }

            var totalRow = new TableRow();
            totalRow.Cells.Add(CreateCell("الإجمالي", true));
            totalRow.Cells.Add(CreateCell($"{fees.Sum(f => f.DeliveryFee):N2}", true));
            totalRow.Cells.Add(CreateCell(""));
            group.Rows.Add(totalRow);

            table.RowGroups.Add(group);
            doc.Blocks.Add(table);
        }

        private TableCell CreateCell(string text, bool bold = false, int colSpan = 1)
        {
            var cell = new TableCell(new Paragraph(new Run(text))
            {
                FontSize = 12,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = TextAlignment.Center
            })
            {
                Padding = new Thickness(4),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5)
            };

            if (colSpan > 1) cell.ColumnSpan = colSpan;
            return cell;
        }
    }
}
