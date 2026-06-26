using Microsoft.EntityFrameworkCore;
using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Restaurant_System
{
    public partial class ManageDeliveryPlaces : Window
    {
        private readonly RestaurantDbContext db;

        public ManageDeliveryPlaces()
        {
            InitializeComponent();
            db = new RestaurantDbContext();
            LoadPlaces();
        }

        private void LoadPlaces()
        {
            PlacesDataGrid.ItemsSource = db.DeliveryPlaces.AsNoTracking().ToList();
        }

        private void AddPlace_Click(object sender, RoutedEventArgs e)
        {
            string name = PlaceNameTextBox.Text.Trim();
            string feeText = DeliveryFeeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(feeText))
            {
                MessageBox.Show("من فضلك أدخل اسم المنطقة وسعر التوصيل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(feeText, out decimal fee))
            {
                MessageBox.Show("أدخل رقم صالح لسعر التوصيل", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newPlace = new DeliveryPlace
            {
                Name = name,
                DeliveryFee = fee
            };

            db.DeliveryPlaces.Add(newPlace);
            db.SaveChanges();

            PlaceNameTextBox.Clear();
            DeliveryFeeTextBox.Clear();
            LoadPlaces();

            MessageBox.Show("✅ تمت إضافة المنطقة بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditPlace_Click(object sender, RoutedEventArgs e)
        {
            if (PlacesDataGrid.SelectedItem is DeliveryPlace selected)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("أدخل الاسم الجديد:", "تعديل المنطقة", selected.Name);
                string newFeeText = Microsoft.VisualBasic.Interaction.InputBox("أدخل سعر التوصيل الجديد:", "تعديل السعر", selected.DeliveryFee.ToString());

                if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newFeeText))
                    return;

                if (!decimal.TryParse(newFeeText, out decimal newFee))
                {
                    MessageBox.Show("أدخل رقم صالح للسعر", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                selected.Name = newName;
                selected.DeliveryFee = newFee;

                db.DeliveryPlaces.Update(selected);
                db.SaveChanges();
                LoadPlaces();
            }
        }

        private void DeletePlace_Click(object sender, RoutedEventArgs e)
        {
            if (PlacesDataGrid.SelectedItem is DeliveryPlace selected)
            {
                if (MessageBox.Show($"هل أنت متأكد من حذف المنطقة '{selected.Name}'؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    db.DeliveryPlaces.Remove(selected);
                    db.SaveChanges();
                    LoadPlaces();
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                LoadPlaces();
            }
            else
            {
                PlacesDataGrid.ItemsSource = db.DeliveryPlaces
                    .AsNoTracking()
                    .Where(p => p.Name.ToLower().Contains(query) ||
                                p.DeliveryFee.ToString().Contains(query))
                    .ToList();
            }
        }
    }
}
