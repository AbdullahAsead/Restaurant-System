using Restaurant_System.Data;
using System.Linq;
using System.Windows;

namespace Restaurant_System
{
    public partial class Login_db : Window
    {
        public Login_db()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Password;

            using (var db = new RestaurantDbContext())
            {
                // ✅ التعديل: البحث في جدول DashboardPass بدلاً من Admins
                var dashboardUser = db.DashboardPasses
                    .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                if (dashboardUser != null)
                {
                    MessageBox.Show($"✅ Welcome, {dashboardUser.Username}!",
                        "Login Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // ✅ فتح الداشبورد
                    Dashboard dashboard = new Dashboard();
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("❌ Invalid dashboard username or password.",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
