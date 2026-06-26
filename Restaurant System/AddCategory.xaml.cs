using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Restaurant_System
{
    public partial class AddCategory : Window
    {
        private readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restaurant.db");
        private int _categoryId = -1;

        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();

        public AddCategory()
        {
            InitializeComponent();
            this.DataContext = this; // ربط DataGrid و binding
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text.Trim();
            string categoryDesc = ""; // لا يوجد TextBox للوصف الآن

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Please enter a category name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Items.Count == 0)
            {
                MessageBox.Show("No items added for this category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new SqliteConnection($"Data Source={dbPath}"))
                {
                    conn.Open();

                    // إضافة القسم
                    string catQuery = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description); SELECT last_insert_rowid();";
                    using (var catCmd = new SqliteCommand(catQuery, conn))
                    {
                        catCmd.Parameters.AddWithValue("@Name", categoryName);
                        catCmd.Parameters.AddWithValue("@Description", categoryDesc);
                        _categoryId = Convert.ToInt32(catCmd.ExecuteScalar());
                    }

                    // إضافة العناصر
                    foreach (var item in Items)
                    {
                        string itemQuery = "INSERT INTO Item (Name, Price, Description, CategoryId, Weight, Barcode) VALUES (@Name, @Price, @Description, @CategoryId, @Weight, @Barcode)";
                        using (var itemCmd = new SqliteCommand(itemQuery, conn))
                        {
                            itemCmd.Parameters.AddWithValue("@Name", item.Name);
                            itemCmd.Parameters.AddWithValue("@Price", item.Price);
                            itemCmd.Parameters.AddWithValue("@Description", item.Description ?? "");
                            itemCmd.Parameters.AddWithValue("@CategoryId", _categoryId);
                            itemCmd.Parameters.AddWithValue("@Weight", item.Weight ?? (object)DBNull.Value);
                            itemCmd.Parameters.AddWithValue("@Barcode", item.Barcode ?? (object)DBNull.Value);
                            itemCmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Category and all items added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving category and items: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ItemModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public decimal? Weight { get; set; }
        public string Barcode { get; set; }
    }
}
