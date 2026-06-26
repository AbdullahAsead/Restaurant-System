using System;
using System.Windows;

namespace Restaurant_System
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            Login_db login_Db = new Login_db();
            login_Db.Show();
            this.Close();
        }

       

        // ✅ الزر الجديد لفتح إعدادات الطباعة
        private void PrinterSettings_Click(object sender, RoutedEventArgs e)
        {
            PrinterSetting printerSetting = new PrinterSetting();
            printerSetting.ShowDialog();
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            Orders orders = new Orders();
            orders.Show();
            this.Close();
        }

        private void CashierPermissions_Click(object sender, RoutedEventArgs e)
        {
            Cashier cashier = new Cashier();
            cashier.Show();
            this.Close();
        }
    }
}
