using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Restaurant_System.Models;
using Restaurant_System.Helpers;
using System.Data.SQLite;

namespace Restaurant_System
{
    public partial class ViewEditAdmins : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();

        public ObservableCollection<Admin> Admins { get; set; }
        private ObservableCollection<Admin> AllAdmins { get; set; } // النسخة الكاملة لنتائج البحث

        public ViewEditAdmins()
        {
            InitializeComponent();
            Admins = new ObservableCollection<Admin>();
            AllAdmins = new ObservableCollection<Admin>();
            LoadAdmins();
            AdminsGrid.ItemsSource = Admins;
        }

        private void LoadAdmins()
        {
            Admins.Clear();
            AllAdmins.Clear();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, FullName, Username, Password FROM Admins";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var admin = new Admin
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Username = reader.GetString(2),
                            Password = reader.GetString(3)
                        };

                        Admins.Add(admin);
                        AllAdmins.Add(admin);
                    }
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Admin admin)
            {
                string newFullName = Microsoft.VisualBasic.Interaction.InputBox("Edit Full Name:", "Edit", admin.FullName);
                string newUsername = Microsoft.VisualBasic.Interaction.InputBox("Edit Username:", "Edit", admin.Username);
                string newPassword = Microsoft.VisualBasic.Interaction.InputBox("Edit Password:", "Edit", admin.Password);

                if (!string.IsNullOrWhiteSpace(newFullName) &&
                    !string.IsNullOrWhiteSpace(newUsername) &&
                    !string.IsNullOrWhiteSpace(newPassword))
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "UPDATE Admins SET FullName = @FullName, Username = @Username, Password = @Password WHERE Id = @Id";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@FullName", newFullName);
                            command.Parameters.AddWithValue("@Username", newUsername);
                            command.Parameters.AddWithValue("@Password", newPassword);
                            command.Parameters.AddWithValue("@Id", admin.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadAdmins();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Admin admin)
            {
                if (MessageBox.Show("Are you sure you want to delete this admin?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Admins WHERE Id = @Id";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", admin.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadAdmins();
                }
            }
        }

        // 🔍 البحث الفوري
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim().ToLower();

            var filtered = AllAdmins
                .Where(a =>
                    string.IsNullOrEmpty(searchTerm) ||
                    a.FullName.ToLower().Contains(searchTerm) ||
                    a.Username.ToLower().Contains(searchTerm))
                .ToList();

            Admins.Clear();
            foreach (var admin in filtered)
                Admins.Add(admin);
        }

        // 🔁 مسح البحث
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
        }
    }
}
