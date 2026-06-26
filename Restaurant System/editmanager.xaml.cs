using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Linq;
using System.Windows;

namespace Restaurant_System
{
    public partial class editmanager : Window
    {
        private RestaurantDbContext db;

        public editmanager()
        {
            InitializeComponent();
            LoadManagers();
        }

        private void LoadManagers()
        {
            try
            {
                db = new RestaurantDbContext();
                ManagersGrid.ItemsSource = db.DashboardPasses.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error loading managers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                db.SaveChanges();
                MessageBox.Show("✅ Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadManagers(); // إعادة تحميل البيانات بعد الحفظ
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteManager_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is int managerId)
            {
                var result = MessageBox.Show("⚠️ هل تريد حذف هذا المدير؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var manager = db.DashboardPasses.FirstOrDefault(m => m.Id == managerId);
                        if (manager != null)
                        {
                            db.DashboardPasses.Remove(manager);
                            db.SaveChanges();
                            MessageBox.Show("🗑 Manager deleted successfully!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadManagers(); // تحديث الجدول بعد الحذف
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error deleting manager: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
