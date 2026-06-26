using System;
using System.Collections.ObjectModel;
using System.Windows;
using Restaurant_System.Models;

namespace Restaurant_System
{
    public partial class Dashboard : Window
    {
        public ObservableCollection<Sale> CurrentSales { get; set; } = new ObservableCollection<Sale>();

        public Dashboard()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Options option = new Options();
            option.Show();
            this.Close();
        }

        // ✅ إدارة الكاشير
        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            new AddAdmin().Show();
        }

        private void ViewEditAdmins_Click(object sender, RoutedEventArgs e)
        {
            new ViewEditAdmins().Show();
        }

        // ✅ موظفي التوصيل
        private void AddDeliveryEmployee_Click(object sender, RoutedEventArgs e)
        {
            new AddDeliveryEmployee().Show();
        }

        private void ViewEditDeliveryEmployees_Click(object sender, RoutedEventArgs e)
        {
            new ViewEditDeliveryEmployees().Show();
        }

        // ✅ ملخص الطلبات
        private void ViewOrdersSummary_Click(object sender, RoutedEventArgs e)
        {
            new OrdersSummary().Show();
        }

        // ✅ التقارير
        private void ViewReports_Click(object sender, RoutedEventArgs e)
        {
            new ReportsWindow().Show();
        }

        // ✅ تصفير الوردية
        private void OpenClearSalesWindow_Click(object sender, RoutedEventArgs e)
        {
            new ClearSalesWindow().ShowDialog();
        }

        // ✅ مستخدمي لوحة التحكم
        private void AddDashboardUser_Click(object sender, RoutedEventArgs e)
        {
            new addmanager().Show();
        }

        private void EditDashboardUser_Click(object sender, RoutedEventArgs e)
        {
            new editmanager().Show();
        }

        private void ManageDeliveryPlaces_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageDeliveryPlaces();
            window.ShowDialog();
        }

        private void OpenDeliveryClosingForAdmin_Click(object sender, RoutedEventArgs e)
        {
            new DeliveryClosingForAdmin().Show();
        }
    }
}
