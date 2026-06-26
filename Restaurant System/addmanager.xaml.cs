using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Windows;

namespace Restaurant_System
{
    public partial class addmanager : Window
    {
        public addmanager()
        {
            InitializeComponent();
        }

        private void AddManager_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("⚠️ Please fill in both fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new RestaurantDbContext())
                {
                    var manager = new DashboardPass
                    {
                        Username = username,
                        PasswordHash = password, // لو هتعمل Hash حطها هنا
                        CreatedAt = DateTime.Now
                    };

                    db.DashboardPasses.Add(manager);
                    db.SaveChanges();
                }

                MessageBox.Show("✅ Manager added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UsernameBox.Clear();
                PasswordBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error adding manager: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
