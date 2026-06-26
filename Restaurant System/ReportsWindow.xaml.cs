using Restaurant_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class ReportsWindow : Window
    {
        private List<SaleViewModel> allSales = new();
        private List<OrderViewModel> allOrders = new();

        public ReportsWindow()
        {
            InitializeComponent();
            DatePickerFilter.SelectedDate = DateTime.Now;
            LoadData(DateTime.Now);
        }

        public class SaleViewModel
        {
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public string SaleDate { get; set; } = "";
            public decimal Total => Quantity * UnitPrice;
        }

        public class OrderViewModel
        {
            public int OrderId { get; set; }
            public decimal DeliveryFee { get; set; }
            public string Date { get; set; } = "";
        }

        private void LoadData(DateTime date)
        {
            using var db = new RestaurantDbContext();

            DateTime start = date.Date;
            DateTime end = start.AddDays(1);

            allSales = db.Sales
                .AsEnumerable()
                .Where(s => DateTime.TryParse(s.SaleDate, out var d) && d >= start && d < end)
                .Select(s => new SaleViewModel
                {
                    ProductName = s.ProductName,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    SaleDate = s.SaleDate
                })
                .ToList();

            // ✅ هنا السطر الجديد فقط: تجميع المبيعات حسب اسم المنتج
            allSales = allSales
                .GroupBy(s => s.ProductName)
                .Select(g => new SaleViewModel
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().UnitPrice,
                    SaleDate = g.First().SaleDate
                })
                .ToList();

            allOrders = db.Orders
                .AsEnumerable()
                .Where(o => DateTime.TryParse(o.Date.ToString(), out var d) && d >= start && d < end)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    DeliveryFee = o.DeliveryFee,
                    Date = o.Date.ToString()
                })
                .ToList();

            SalesSummaryItems.ItemsSource = allSales;
            OrdersSummaryItems.ItemsSource = allOrders;
            UpdateTotals();
        }

        private void DatePickerFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerFilter.SelectedDate.HasValue)
                LoadData(DatePickerFilter.SelectedDate.Value);
        }

        private void txtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            string term = txtSearchProduct.Text.Trim().ToLower();
            SalesSummaryItems.ItemsSource = allSales
                .Where(s => string.IsNullOrEmpty(term) || s.ProductName.ToLower().Contains(term))
                .ToList();
        }

        private void UpdateTotals()
        {
            decimal totalSales = allSales.Sum(s => s.Total);
            decimal totalDelivery = allOrders.Sum(o => o.DeliveryFee);
            decimal grand = totalSales + totalDelivery;

            TotalSalesText.Text = $"إجمالي المبيعات: {totalSales:N2} ج";
            TotalDeliveryText.Text = $"إجمالي التوصيل: {totalDelivery:N2} ج";
            GrandTotalText.Text = $"الإجمالي الكلي: {grand:N2} ج";
        }
    }
}
