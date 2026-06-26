using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Restaurant_System.Helpers;
using System.Data.SQLite;

namespace Restaurant_System
{
    public partial class ViewEditItems : Window
    {
        private readonly string connectionString = DatabaseHelper.GetConnectionString();
        public ObservableCollection<ItemWithCategory> Items { get; set; }

        public ViewEditItems()
        {
            InitializeComponent();
            Items = new ObservableCollection<ItemWithCategory>();
            LoadItems();
            ItemsGrid.ItemsSource = Items;
        }

        // ✅ تحميل كل العناصر
        private void LoadItems(string nameFilter = "", string priceFilter = "")
        {
            Items.Clear();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT Item.Id, Item.Name, Item.Description, Item.Price, Categories.Name AS CategoryName
                    FROM Item
                    JOIN Categories ON Item.CategoryId = Categories.Id
                    WHERE 1=1";

                // ✅ لو فيه فلتر بالاسم أو السعر
                if (!string.IsNullOrEmpty(nameFilter))
                    query += " AND Item.Name LIKE @Name";

                if (!string.IsNullOrEmpty(priceFilter))
                    query += " AND Item.Price = @Price";

                using (var command = new SQLiteCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(nameFilter))
                        command.Parameters.AddWithValue("@Name", "%" + nameFilter + "%");

                    if (!string.IsNullOrEmpty(priceFilter) && decimal.TryParse(priceFilter, out decimal p))
                        command.Parameters.AddWithValue("@Price", p);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Items.Add(new ItemWithCategory
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                CategoryName = reader.GetString(4)
                            });
                        }
                    }
                }
            }
        }

        // ✅ زر البحث
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameSearchBox.Text.Trim();
            string price = PriceSearchBox.Text.Trim();
            LoadItems(name, price);
        }

        // ✅ زر المسح
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            NameSearchBox.Clear();
            PriceSearchBox.Clear();
            LoadItems();
        }

        // ✅ تعديل عنصر
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ItemWithCategory item)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Edit name:", "Edit", item.Name);
                string newDesc = Microsoft.VisualBasic.Interaction.InputBox("Edit description:", "Edit", item.Description);
                string newPriceText = Microsoft.VisualBasic.Interaction.InputBox("Edit price:", "Edit", item.Price.ToString());

                if (decimal.TryParse(newPriceText, out decimal newPrice))
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "UPDATE Item SET Name=@Name, Description=@Description, Price=@Price WHERE Id=@Id";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", newName);
                            command.Parameters.AddWithValue("@Description", newDesc);
                            command.Parameters.AddWithValue("@Price", newPrice);
                            command.Parameters.AddWithValue("@Id", item.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadItems();
                }
            }
        }

        // ✅ حذف عنصر
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ItemWithCategory item)
            {
                if (MessageBox.Show("هل أنت متأكد من حذف هذا العنصر؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Item WHERE Id=@Id";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", item.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadItems();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ItemWithCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
    }
}
