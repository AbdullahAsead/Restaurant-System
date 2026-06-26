using System;
using System.Windows;

namespace Restaurant_System
{
    public partial class Cashier : Window
    {
        public Cashier()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // فتح صفحة Options
            Options optionsWindow = new Options();
            optionsWindow.Show();

            // إغلاق صفحة الكاشير الحالية
            this.Close();
        }

        // ✅ إدارة الأقسام
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            new AddCategory().Show();
        }

        private void ViewEditCategories_Click(object sender, RoutedEventArgs e)
        {
            new ViewEditCategories().Show();
        }

        // ✅ إدارة العناصر
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            new AddItem().Show();
        }

        private void ViewEditItems_Click(object sender, RoutedEventArgs e)
        {
            new ViewEditItems().Show();
        }

        // ✅ بيانات العملاء
        private void ViewCustomersData_Click(object sender, RoutedEventArgs e)
        {
            CustomersData customersDataWindow = new CustomersData();
            customersDataWindow.ShowDialog();
        }

        private void ViewOrdersSummary_Click(object sender, RoutedEventArgs e)
        {
            new OrderSummaryCashier().Show();
        }

        private void CloseDelivery_Click(object sender, RoutedEventArgs e)
        {
            new DeliveryClosing().Show();
        }
    }
}
