using Microsoft.Data.Sqlite;
using Restaurant_System.Helpers;
using Restaurant_System.Models; // ✅ استخدام الموديل من الداتا بيز
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class ViewEditDeliveryEmployees : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();

        public ObservableCollection<DeliveryEmployee> DeliveryPeople { get; set; } = new();
        private ObservableCollection<DeliveryEmployee> FilteredPeople = new();

        public ViewEditDeliveryEmployees()
        {
            InitializeComponent();
            LoadEmployees();
            DeliveryGrid.ItemsSource = FilteredPeople;
        }

        // ✅ تحميل الموظفين من جدول DeliveryPeople
        private void LoadEmployees()
        {
            DeliveryPeople.Clear();

            try
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                const string query = "SELECT Id, Name, PhoneNumber, MachineType, MachineNumber FROM DeliveryPeople";
                using var cmd = new SqliteCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DeliveryPeople.Add(new DeliveryEmployee
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        PhoneNumber = reader.GetString(2),
                        MachineType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        MachineNumber = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تحميل موظفي التوصيل: " + ex.Message);
            }

            FilteredPeople.Clear();
            foreach (var emp in DeliveryPeople)
                FilteredPeople.Add(emp);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            NameSearchBox.Text = "";
            PhoneSearchBox.Text = "";
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            string name = NameSearchBox.Text.Trim().ToLower();
            string phone = PhoneSearchBox.Text.Trim().ToLower();

            FilteredPeople.Clear();
            foreach (var emp in DeliveryPeople.Where(x =>
                x.Name.ToLower().Contains(name) &&
                x.PhoneNumber.ToLower().Contains(phone)))
            {
                FilteredPeople.Add(emp);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not DeliveryEmployee emp) return;

            string newName = Microsoft.VisualBasic.Interaction.InputBox("Edit name:", "Edit", emp.Name);
            string newPhone = Microsoft.VisualBasic.Interaction.InputBox("Edit phone number:", "Edit", emp.PhoneNumber);
            string newMachineType = Microsoft.VisualBasic.Interaction.InputBox("Edit machine type:", "Edit", emp.MachineType);
            string newMachineNumber = Microsoft.VisualBasic.Interaction.InputBox("Edit machine number:", "Edit", emp.MachineNumber);

            if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newPhone)) return;

            try
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                const string query = @"UPDATE DeliveryPeople
                                       SET Name = @Name, PhoneNumber = @PhoneNumber,
                                           MachineType = @MachineType, MachineNumber = @MachineNumber
                                       WHERE Id = @Id";

                using var cmd = new SqliteCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", newName);
                cmd.Parameters.AddWithValue("@PhoneNumber", newPhone);
                cmd.Parameters.AddWithValue("@MachineType", newMachineType);
                cmd.Parameters.AddWithValue("@MachineNumber", newMachineNumber);
                cmd.Parameters.AddWithValue("@Id", emp.Id);
                cmd.ExecuteNonQuery();

                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تعديل الموظف: " + ex.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not DeliveryEmployee emp) return;

            if (MessageBox.Show("هل أنت متأكد من حذف هذا الموظف؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                const string query = "DELETE FROM DeliveryPeople WHERE Id = @Id";
                using var cmd = new SqliteCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", emp.Id);
                cmd.ExecuteNonQuery();

                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء حذف الموظف: " + ex.Message);
            }
        }
    }
}
