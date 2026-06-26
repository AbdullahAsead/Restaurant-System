using Microsoft.Data.Sqlite;
using Restaurant_System.Helpers;
using Restaurant_System.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class CustomersData : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();
        public ObservableCollection<Customer> Customers { get; set; }

        public CustomersData()
        {
            InitializeComponent();
            Customers = new ObservableCollection<Customer>();
            CustomersDataGrid.ItemsSource = Customers; // ربط الـ DataGrid مباشرة بالـ ObservableCollection
            LoadCustomers();
        }

        // ✅ تحميل كل العملاء بدون تكرار
        private void LoadCustomers()
        {
            try
            {
                Customers.Clear(); // تنظيف القائمة قبل التحميل

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Name, Address, Phone, AlternatePhone, CreatedAt FROM Customers ORDER BY Id DESC";

                    using (var command = new SqliteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);

                            // منع التكرار: لو العميل موجود بالفعل في القائمة لا نضيفه
                            if (Customers.Any(c => c.Id == id))
                                continue;

                            Customers.Add(new Customer
                            {
                                Id = id,
                                Name = reader.GetString(1),
                                Address = reader.GetString(2),
                                Phone = reader.GetString(3),
                                AlternatePhone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل بيانات العملاء:\n" + ex.Message);
            }
        }

        // ✅ بحث بالاسم أو رقم الهاتف
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string nameSearch = NameSearchBox.Text.Trim().ToLower();
            string phoneSearch = PhoneSearchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(nameSearch) && string.IsNullOrWhiteSpace(phoneSearch))
            {
                MessageBox.Show("من فضلك أدخل الاسم أو رقم الهاتف للبحث", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var filtered = Customers
                .Where(c =>
                    (!string.IsNullOrWhiteSpace(nameSearch) && c.Name.ToLower().Contains(nameSearch)) ||
                    (!string.IsNullOrWhiteSpace(phoneSearch) && c.Phone.ToLower().Contains(phoneSearch))
                ).ToList();

            CustomersDataGrid.ItemsSource = filtered;
        }

        // ✅ زر مسح البحث
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            NameSearchBox.Text = "";
            PhoneSearchBox.Text = "";
            CustomersDataGrid.ItemsSource = Customers;
        }

        // ✅ تعديل العميل
        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Customer customer)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("اكتب الاسم الجديد:", "تعديل العميل", customer.Name);
                if (string.IsNullOrWhiteSpace(newName)) return;

                string newAddress = Microsoft.VisualBasic.Interaction.InputBox("اكتب العنوان الجديد:", "تعديل العميل", customer.Address);
                string newPhone = Microsoft.VisualBasic.Interaction.InputBox("اكتب رقم الهاتف الجديد:", "تعديل العميل", customer.Phone);
                string newAltPhone = Microsoft.VisualBasic.Interaction.InputBox("اكتب الهاتف البديل (اختياري):", "تعديل العميل", customer.AlternatePhone);

                try
                {
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        string updateQuery = @"UPDATE Customers 
                                              SET Name=@Name, Address=@Address, Phone=@Phone, AlternatePhone=@AltPhone 
                                              WHERE Id=@Id";

                        using (var command = new SqliteCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Name", newName);
                            command.Parameters.AddWithValue("@Address", newAddress);
                            command.Parameters.AddWithValue("@Phone", newPhone);
                            command.Parameters.AddWithValue("@AltPhone", string.IsNullOrEmpty(newAltPhone) ? DBNull.Value : newAltPhone);
                            command.Parameters.AddWithValue("@Id", customer.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("✅ تم تعديل بيانات العميل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCustomers(); // إعادة تحميل العملاء بعد التعديل
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء تعديل بيانات العميل:\n" + ex.Message);
                }
            }
        }

        // ✅ حذف العميل من الداتا بيز
        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Customer customer)
            {
                var result = MessageBox.Show($"هل أنت متأكد أنك تريد حذف العميل '{customer.Name}'؟", "تأكيد الحذف",
                                             MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            string deleteQuery = "DELETE FROM Customers WHERE Id=@Id";
                            using (var command = new SqliteCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Id", customer.Id);
                                command.ExecuteNonQuery();
                            }
                        }

                        Customers.Remove(customer); // إزالة العميل مباشرة من الـ ObservableCollection
                        MessageBox.Show("🗑️ تم حذف العميل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("حدث خطأ أثناء حذف العميل:\n" + ex.Message);
                    }
                }
            }
        }
    }
}
