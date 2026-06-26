using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Restaurant_System.Data; 
using Restaurant_System.Models;

namespace Restaurant_System
{
    public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Password;

            using (var db = new RestaurantDbContext())
            {
                var admin = db.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);

                if (admin != null)
                {
                    MessageBox.Show($"Welcome, {admin.FullName}!", "Login Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Options optionsWindow = new Options();
                    optionsWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



    }
}
