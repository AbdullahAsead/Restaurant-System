using Microsoft.Data.Sqlite;
using Restaurant_System.Helpers;
using System;
using System.Windows;

namespace Restaurant_System
{
    public partial class AddDeliveryEmployee : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();

        public AddDeliveryEmployee()
        {
            InitializeComponent();
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string phone = PhoneNumberTextBox.Text.Trim();
            string machineType = MachineTypeTextBox.Text.Trim();
            string machineNumber = MachineNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("من فضلك ادخل الاسم ورقم الهاتف.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    // ✅ إضافة الموظف مع تعيين DailyOrdersCount = 0 تلقائياً
                    string query = @"
                        INSERT INTO DeliveryPeople (Name, PhoneNumber, MachineType, MachineNumber, DailyOrdersCount)
                        VALUES (@Name, @PhoneNumber, @MachineType, @MachineNumber, 0);
                    ";

                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                        cmd.Parameters.AddWithValue("@MachineType", machineType);
                        cmd.Parameters.AddWithValue("@MachineNumber", machineNumber);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("✅ تم إضافة موظف التوصيل بنجاح!", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ في قاعدة البيانات:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
