using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class ViewEditCategories : Window
    {
        private readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restaurant.db");

        public ObservableCollection<CategoryModel> Categories { get; } = new();
        private ObservableCollection<CategoryModel> FilteredCategories = new();

        public ViewEditCategories()
        {
            InitializeComponent();
            CategoriesGrid.ItemsSource = FilteredCategories;
            LoadCategories();
        }

        // 🔹 تحميل الأقسام من قاعدة البيانات
        private void LoadCategories()
        {
            Categories.Clear();

            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                const string query = "SELECT Id, Name, Description FROM Categories;";
                using var cmd = new SqliteCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Categories.Add(new CategoryModel
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories:\n{ex.Message}",
                                "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // تحميل أولي في الجدول
            ApplyFilter();
        }

        // 🔍 تطبيق الفلترة على حسب خانات البحث
        private void ApplyFilter()
        {
            string nameText = NameSearchBox.Text.Trim().ToLower();
            string descText = DescSearchBox.Text.Trim().ToLower();

            FilteredCategories.Clear();

            foreach (var c in Categories.Where(c =>
                (string.IsNullOrEmpty(nameText) || c.Name.ToLower().Contains(nameText)) &&
                (string.IsNullOrEmpty(descText) || c.Description.ToLower().Contains(descText))))
            {
                FilteredCategories.Add(c);
            }
        }

        // 🔹 عند الكتابة في أي مربع بحث
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        // 🔎 عند الضغط على زر البحث
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        // 🧹 عند الضغط على زر المسح
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            NameSearchBox.Text = "";
            DescSearchBox.Text = "";
            ApplyFilter();
        }

        // ✏️ زر «Edit»
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not CategoryModel cat) return;

            string newName = Microsoft.VisualBasic.Interaction.InputBox("Edit category name:", "Edit", cat.Name);
            string newDesc = Microsoft.VisualBasic.Interaction.InputBox("Edit description:", "Edit", cat.Description);

            if (string.IsNullOrWhiteSpace(newName)) return;

            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                const string q = @"UPDATE Categories SET Name = @Name, Description = @Desc WHERE Id = @Id;";
                using var cmd = new SqliteCommand(q, conn);
                cmd.Parameters.AddWithValue("@Name", newName);
                cmd.Parameters.AddWithValue("@Desc", newDesc);
                cmd.Parameters.AddWithValue("@Id", cat.Id);
                cmd.ExecuteNonQuery();

                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating:\n{ex.Message}",
                                "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🗑️ زر «Delete»
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not CategoryModel cat) return;

            if (MessageBox.Show("هل أنت متأكد من حذف هذا القسم؟", "تأكيد الحذف",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                const string q = "DELETE FROM Categories WHERE Id = @Id;";
                using var cmd = new SqliteCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", cat.Id);
                cmd.ExecuteNonQuery();

                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting:\n{ex.Message}",
                                "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
