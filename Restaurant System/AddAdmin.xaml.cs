using Restaurant_System.Data;  // ⬅️ مهم
using Restaurant_System.Models;
using System;
using System.Linq;
using System.Windows;

namespace Restaurant_System
{
    public partial class AddAdmin : Window
    {
        public AddAdmin()
        {
            InitializeComponent();
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("يرجى إدخال جميع البيانات", "تنبيه",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new RestaurantDbContext();

                // 🔍 هل اسم المستخدم مكرر؟
                bool userExists = db.Admins.Any(a => a.Username == username);
                if (userExists)
                {
                    MessageBox.Show("اسم المستخدم موجود بالفعل!", "خطأ",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ➕ إضافة الأدمن
                var admin = new Admin
                {
                    FullName = fullName,
                    Username = username,
                    Password = password
                };

                db.Admins.Add(admin);
                db.SaveChanges();

                MessageBox.Show("تم إضافة المدير بنجاح", "نجاح",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ غير متوقع: {ex.Message}", "خطأ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
