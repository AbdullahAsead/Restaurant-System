using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using System;
using System.Windows;

namespace Restaurant_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ✅ 1. تحقق من صلاحية البرنامج
            if (!IsAppLicensed())
            {
                MessageBox.Show(".يرجى الاتصال بالدعم الفني", "انتهاء الصلاحية", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown(); // اقفل البرنامج
                return;
            }

            // ✅ 2. تأكد من إن الـ DB محدثة
            try
            {
                using (var db = new RestaurantDbContext())
                {
                    db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحديث قاعدة البيانات:\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private bool IsAppLicensed()
        {
            DateTime startDate = new DateTime(2025, 8, 3); // تاريخ بداية الصلاحية
            DateTime endDate = startDate.AddYears(1);      // مدة سنة
            DateTime now = DateTime.Now;
            return now <= endDate;
        }

        // ✅ زر الدخول كأدمن
        private void RegisterAdminButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login_db adminLogin = new Login_db();
                adminLogin.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء فتح صفحة الأدمن:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ زر الدخول ككاشير
        private void RegisterCashierButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginWindow cashierLogin = new LoginWindow();
                cashierLogin.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء فتح صفحة الكاشير:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // الزر الأصلي بتاعك (ماحذفناهوش)
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
