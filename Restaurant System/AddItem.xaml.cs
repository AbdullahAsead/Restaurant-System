using System;
using System.Collections.ObjectModel;
using System.Windows;
using Restaurant_System.Models;
using Restaurant_System.Helpers;
using System.Data.SQLite;

namespace Restaurant_System
{
    public partial class AddItem : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Categories> Categories { get; set; } = new ObservableCollection<Categories>();

        public AddItem()
        {
            InitializeComponent();
            DataContext = this;

            EnsureTablesExist(); // ✅ تأكد من وجود الجداول
            LoadCategories();
        }

        // إنشاء الجداول إذا مش موجودة
        private void EnsureTablesExist()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createCategories = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT
                    );";
                using (var command = new SQLiteCommand(createCategories, connection))
                    command.ExecuteNonQuery();

                string createItems = @"
                    CREATE TABLE IF NOT EXISTS Item (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Price REAL NOT NULL,
                        Description TEXT,
                        Weight REAL,
                        Barcode TEXT,
                        CategoryId INTEGER NOT NULL,
                        FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
                    );";
                using (var command = new SQLiteCommand(createItems, connection))
                    command.ExecuteNonQuery();
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Name FROM Categories";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categories.Add(new Categories
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأقسام: " + ex.Message);
            }
        }

        private void SaveItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ItemsDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
                ItemsDataGrid.CommitEdit();

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    foreach (var item in Items)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Name) && item.CategoryId != 0)
                        {
                            string query = @"INSERT INTO Item (Name, Price, Description, Weight, Barcode, CategoryId)
                                             VALUES (@Name, @Price, @Description, @Weight, @Barcode, @CategoryId)";
                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Name", item.Name);
                                command.Parameters.AddWithValue("@Price", item.Price);
                                command.Parameters.AddWithValue("@Description", item.Description ?? "");
                                command.Parameters.AddWithValue("@Weight", item.Weight ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@Barcode", item.Barcode ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@CategoryId", item.CategoryId);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                MessageBox.Show("✅ Items added successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error saving items: " + ex.Message);
            }
        }
    }
}
