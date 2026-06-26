using ESCPOS_NET;
using Restaurant_System.Data;
using Restaurant_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace Restaurant_System
{
    public partial class Orders : Window
    {
        private Dictionary<string, List<Item>> categoryItems = new();
        private bool isDeliveryOrder = false;
        private int? selectedTableId = null;
        private int? selectedDeliveryEmployeeId = null;
        private string quantityString = "";

        private string kitchenPrinterName = "Printer"; // اسم طابعة المطبخ LAN
        private string usbPrinterName = "Printer"; // اسم طابعة USB

        private decimal selectedDeliveryCost = 0m; // ✅ متغير لحفظ السعر المختار

        public Orders()
        {
            InitializeComponent();
            LoadCategoriesAndItemsFromDB();
            LoadDeliveryPeople();
            LoadDeliveryPlaces(); // ✅ تحميل مناطق التوصيل
        }

        // ✅ كلاس لعرض الأماكن
        private class DeliveryPlaceDisplay
        {
            public int Id { get; set; }
            public string PlaceName { get; set; }
            public decimal DeliveryCost { get; set; }
            public string DisplayText => $"{PlaceName} — {DeliveryCost:F2} جنيه";
        }

        // ✅ تحميل مناطق التوصيل من قاعدة البيانات
        private void LoadDeliveryPlaces()
        {
            using var db = new RestaurantDbContext();
            var places = db.DeliveryPlaces
                .Select(p => new DeliveryPlaceDisplay
                {
                    Id = p.Id,
                    PlaceName = p.Name,
                    DeliveryCost = p.DeliveryFee
                }).ToList();

            if (DeliveryPlaceComboBox is ComboBox combo)
            {
                combo.ItemsSource = places;
                combo.DisplayMemberPath = "DisplayText";
                combo.SelectedValuePath = "DeliveryCost";
                combo.SelectionChanged += DeliveryPlaceComboBox_SelectionChanged;
            }
        }

        // ✅ لما يختار مكان توصيل
        private void DeliveryPlaceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedValue != null)
            {
                selectedDeliveryCost = Convert.ToDecimal(combo.SelectedValue);
            }
        }

        private void LoadDeliveryPeople()
        {
            using var db = new RestaurantDbContext();
            var deliveryEmployees = db.DeliveryPeople.ToList();

            DeliveryPersonComboBox.ItemsSource = deliveryEmployees;
            DeliveryPersonComboBox.DisplayMemberPath = "Name";
            DeliveryPersonComboBox.SelectedValuePath = "Id";
        }

        private void LoadCategoriesAndItemsFromDB()
        {
            using var db = new RestaurantDbContext();
            var categories = db.Categories.ToList();
            var items = db.Items.ToList();

            foreach (var category in categories)
            {
                var btn = new Button
                {
                    Content = category.Name,
                    Style = (Style)FindResource("CategoryButtonStyle")
                };
                btn.Click += Category_Click;
                CategoriesPanel.Children.Add(btn);
                categoryItems[category.Name] = items.Where(i => i.CategoryId == category.Id).ToList();
            }
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = (sender as Button)?.Content.ToString();
            if (!string.IsNullOrEmpty(categoryName))
                LoadCategory(categoryName);
        }

        private void LoadCategory(string categoryName)
        {
            ItemsPanel.Children.Clear();
            if (!categoryItems.ContainsKey(categoryName)) return;

            foreach (var item in categoryItems[categoryName])
            {
                var card = new Border
                {
                    MinWidth = 95, // أقل عرض للكرت
                    Height = Double.NaN, // يخلي الارتفاع تلقائي حسب المحتوى
                    Background = Brushes.White,
                    BorderBrush = (Brush)new BrushConverter().ConvertFrom("#2196F3"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(6),
                    Effect = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 2, Color = Colors.Gray },
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(6) };

                var nameText = new TextBlock
                {
                    Text = item.Name,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap, // يخلي النص الطويل يكسر السطر
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var priceText = new TextBlock
                {
                    Text = $"{item.Price:C}",
                    FontSize = 16,
                    Foreground = Brushes.Green,
                    TextAlignment = TextAlignment.Center
                };

                panel.Children.Add(nameText);
                panel.Children.Add(priceText);

                card.Child = panel;

                card.MouseLeftButtonUp += (s, e) =>
                {
                    int quantity = 1;
                    if (!string.IsNullOrEmpty(quantityString) && int.TryParse(quantityString, out int parsedQty))
                        quantity = parsedQty;

                    var existing = orderListBox.Items.Cast<DisplayOrderItem>().FirstOrDefault(i => i.ItemName == item.Name);
                    if (existing != null)
                        existing.Quantity += quantity;
                    else
                        orderListBox.Items.Add(new DisplayOrderItem { ItemName = item.Name, Price = (decimal)item.Price, Quantity = quantity });

                    RefreshOrderList();
                    quantityString = "";
                };

                ItemsPanel.Children.Add(card);
            }
        }


        private void RefreshOrderList()
        {
            var updatedItems = orderListBox.Items.Cast<DisplayOrderItem>().ToList();
            orderListBox.Items.Clear();
            foreach (var item in updatedItems)
                orderListBox.Items.Add(item);
        }

        private void IsDeliveryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isDeliveryOrder = true;
            DeliveryDetailsPanel.Visibility = Visibility.Visible;
            TableNumberTextBox.IsEnabled = false;
            LoadDeliveryPeople();
            LoadDeliveryPlaces();
        }

        private void IsDeliveryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isDeliveryOrder = false;
            TableNumberTextBox.IsEnabled = true;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (orderListBox.Items.Count == 0)
            {
                MessageBox.Show("لا يوجد عناصر في الطلب.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new RestaurantDbContext();
            Customer customer = null;

            if (isDeliveryOrder)
            {
                customer = new Customer
                {
                    Name = CustomerNameTextBox.Text,
                    Address = CustomerAddressTextBox.Text,
                    Phone = CustomerPhoneTextBox.Text,
                    AlternatePhone = AlternatePhoneTextBox.Text
                };
                db.Customers.Add(customer);
                db.SaveChanges();

                selectedDeliveryEmployeeId = (int?)DeliveryPersonComboBox.SelectedValue;
            }
            else if (int.TryParse(TableNumberTextBox.Text, out int tableNum))
            {
                var existingTable = db.Tables.FirstOrDefault(t => t.Id == tableNum);
                if (existingTable != null)
                {
                    selectedTableId = existingTable.Id;
                }
                else
                {
                    var newTable = new Restaurant_System.Models.Table
                    {
                        Location = $"سفرة {tableNum}",
                    };
                    db.Tables.Add(newTable);
                    db.SaveChanges();
                    selectedTableId = newTable.Id;
                }
            }

            var allItems = db.Items.ToList();
            var orderItems = orderListBox.Items.Cast<DisplayOrderItem>().ToList();
            var orderItemsWithIds = new List<Models.OrderItem>();
            var salesToSave = new List<Sale>();

            foreach (var i in orderItems)
            {
                var itemFromDb = allItems.FirstOrDefault(x => x.Name == i.ItemName);
                if (itemFromDb != null)
                {
                    orderItemsWithIds.Add(new Models.OrderItem
                    {
                        ItemId = itemFromDb.Id,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        ItemName = i.ItemName
                    });
                }
            }

            // ✅ تحديد نوع الطلب تلقائي حسب الحالة
            string orderType = "";
            if (isDeliveryOrder)
                orderType = "Delivery";
            else if (!string.IsNullOrWhiteSpace(TableNumberTextBox.Text))
                orderType = "Hall";
            else
                orderType = "Takeaway";

            // ✅ تجهيز سجل المبيعات
            foreach (var i in orderItems)
            {
                salesToSave.Add(new Sale
                {
                    ProductName = i.ItemName,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price,
                    SaleDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    DeliveryFee = 0m,
                    OrderType = orderType  // ✅ النوع اتسجل هنا
                });
            }

            decimal deliveryCost = selectedDeliveryCost;

            // ✅ إنشاء الطلب الجديد
            var newOrder = new Order
            {
                Date = DateTime.Now,
                IsDelivery = isDeliveryOrder,
                TotalPrice = orderItems.Sum(i => i.Price * i.Quantity) + deliveryCost,
                TableId = isDeliveryOrder ? null : selectedTableId,
                DeliveryEmployeeId = isDeliveryOrder ? selectedDeliveryEmployeeId : null,
                CustomerId = customer?.Id,
                DeliveryFee = deliveryCost,
                OrderItems = orderItemsWithIds,
                OrderType = orderType // ✅ النوع اتسجل كمان في الطلب
            };

            // ✅ لو توصيل أضف "خدمة التوصيل" كمنتج
            if (isDeliveryOrder && deliveryCost > 0)
            {
                salesToSave.Add(new Sale
                {
                    ProductName = "خدمة التوصيل",
                    Quantity = 1,
                    UnitPrice = deliveryCost,
                    SaleDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    DeliveryFee = deliveryCost,
                    OrderType = orderType
                });
            }

            db.Orders.Add(newOrder);
            db.SaveChanges(); // ✅ دلوقتي newOrder.Id اتولد من قاعدة البيانات

            // ✅ ربط كل مبيعة برقم الطلب الجديد
            foreach (var sale in salesToSave)
            {
                sale.OrderId = newOrder.Id;
            }

            db.Sales.AddRange(salesToSave);
            db.SaveChanges();


            if (isDeliveryOrder && selectedDeliveryEmployeeId.HasValue)
            {
                var deliveryPerson = db.DeliveryPeople.FirstOrDefault(d => d.Id == selectedDeliveryEmployeeId.Value);
                if (deliveryPerson != null)
                {
                    deliveryPerson.DailyOrdersCount += 1;
                    db.DeliveryPeople.Update(deliveryPerson);
                    db.SaveChanges();
                }
            }

            PrintReceipt(newOrder, orderItems, customer, deliveryCost);
            ClearForm();
        }

        private void PrintReceipt(Order order, List<DisplayOrderItem> items, Customer customer, decimal deliveryCost)
        {
            FlowDocument doc = BuildReceiptDocument(order, items, customer, deliveryCost);

            bool printDouble = Properties.Settings.Default.DoubleReceiptEnabled;
            bool kitchenEnabled = Properties.Settings.Default.KitchenPrinterEnabled;

            try
            {
                PrintToSpecificPrinter(doc, usbPrinterName);

                if (printDouble)
                {
                    PrintToSpecificPrinter(doc, usbPrinterName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الطباعة على USB:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (kitchenEnabled)
            {
                try
                {
                    PrintToSpecificPrinter(doc, kitchenPrinterName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ أثناء الطباعة على طابعة المطبخ:\n{ex.Message}", "خطأ بالطباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintToSpecificPrinter(FlowDocument doc, string printerName)
        {
            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                var printer = printServer.GetPrintQueues().FirstOrDefault(p =>
                    p.FullName.Equals(printerName, StringComparison.OrdinalIgnoreCase));

                if (printer == null)
                {
                    MessageBox.Show($"⚠️ الطابعة '{printerName}' غير متصلة حاليًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printer);
                IDocumentPaginatorSource dps = doc;
                writer.Write(dps.DocumentPaginator);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الطباعة على '{printerName}':\n{ex.Message}", "خطأ بالطباعة", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument BuildReceiptDocument(Order order, List<DisplayOrderItem> items, Customer customer, decimal deliveryCost)
        {
            FlowDocument doc = new FlowDocument
            {
                FontSize = 13,
                FontFamily = new FontFamily("Segoe UI"),
                PageWidth = 300,
                PagePadding = new Thickness(10, 15, 10, 15),
                TextAlignment = TextAlignment.Center,
                ColumnWidth = double.PositiveInfinity
            };

            doc.Blocks.Add(new Paragraph(new Run("مطعم أبو سليمان"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Margin = new Thickness(0, 0, 0, 4)
            });

            doc.Blocks.Add(new Paragraph(new Run("٣١١ شارع ترعه الجبل - الزيتون الغربيه - امام مسجد الظواهري"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 3)
            });

            doc.Blocks.Add(new Paragraph(new Run("📞 01107217466 / 01515453051 / 0222554553"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 6)
            });

            doc.Blocks.Add(new Paragraph(new Run("══════════════════════════════"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 2, 0, 2)
            });

            Paragraph header = new Paragraph();
            header.Inlines.Add(new Run($"رقم الفاتورة: {order.Id}\n"));
            header.Inlines.Add(new Run($"التاريخ: {order.Date:yyyy-MM-dd}   "));
            header.Inlines.Add(new Run($"الوقت: {order.Date:hh:mm tt}"));
            header.TextAlignment = TextAlignment.Center;
            doc.Blocks.Add(header);

            if (order.IsDelivery && customer != null)
            {
                doc.Blocks.Add(new Paragraph(new Run($"العميل: {customer.Name}")));
                doc.Blocks.Add(new Paragraph(new Run($"العنوان: {customer.Address}")));
                doc.Blocks.Add(new Paragraph(new Run($"هاتف: {customer.Phone}")));
                if (!string.IsNullOrWhiteSpace(customer.AlternatePhone))
                    doc.Blocks.Add(new Paragraph(new Run($"هاتف بديل: {customer.AlternatePhone}")));
                doc.Blocks.Add(new Paragraph(new Run($"المندوب: {DeliveryPersonComboBox.Text}")));
                doc.Blocks.Add(new Paragraph(new Run($"قيمة التوصيل: {deliveryCost:F2} ج.م")));
                if (!string.IsNullOrWhiteSpace(note.Text))
                    doc.Blocks.Add(new Paragraph(new Run($"ملاحظة: {note.Text}")));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(order.TableId?.ToString()))
                    doc.Blocks.Add(new Paragraph(new Run($"رقم السفرة: {order.TableId}")));
            }

            doc.Blocks.Add(new Paragraph(new Run("──────────────────────────────"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 3, 0, 3)
            });

            decimal subtotal = 0;
            foreach (var item in items)
            {
                decimal itemTotal = item.Price * item.Quantity;
                subtotal += itemTotal;

                var line = new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                line.Inlines.Add(new Run($"{item.ItemName}"));
                line.Inlines.Add(new Run($"\t×{item.Quantity}\t"));
                line.Inlines.Add(new Run($"{itemTotal:F2}"));
                doc.Blocks.Add(line);
            }

            doc.Blocks.Add(new Paragraph(new Run("──────────────────────────────"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 3, 0, 3)
            });

            decimal total = subtotal + deliveryCost;
            doc.Blocks.Add(new Paragraph(new Run($"الإجمالي الكلي: {total:F2} ج.م"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run("✨ شكرًا لزيارتكم ✨"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 13,
                Margin = new Thickness(0, 3, 0, 0)
            });

            return doc;
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (orderListBox.SelectedItem is DisplayOrderItem selected)
                orderListBox.Items.Remove(selected);
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("هل تريد مسح كل العناصر؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                orderListBox.Items.Clear();
        }

        private void ClearForm()
        {
            orderListBox.Items.Clear();
            TableNumberTextBox.Clear();
            CustomerNameTextBox.Clear();
            CustomerAddressTextBox.Clear();
            CustomerPhoneTextBox.Clear();
            AlternatePhoneTextBox.Clear();
            DeliveryPersonComboBox.SelectedIndex = -1;
            IsDeliveryCheckBox.IsChecked = false;
            quantityString = "";
            selectedDeliveryCost = 0;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new Options().Show();
            this.Close();
        }

        private void Numpad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string value)
            {
                if (value == "C") quantityString = "";
                else quantityString += value;
            }
        }

        private void ClearNumpad_Click(object sender, RoutedEventArgs e)
        {
            quantityString = "";
        }

        internal void RefreshMenu()
        {
            throw new NotImplementedException();
        }

        private void CustomerPhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string phone = CustomerPhoneTextBox.Text.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                CustomerNameTextBox.Text = "";
                CustomerAddressTextBox.Text = "";
                AlternatePhoneTextBox.Text = "";
                return;
            }

            using var db = new RestaurantDbContext();
            var customer = db.Customers.FirstOrDefault(c => c.Phone.StartsWith(phone));
            if (customer != null)
            {
                CustomerNameTextBox.Text = customer.Name;
                CustomerAddressTextBox.Text = customer.Address;
                AlternatePhoneTextBox.Text = customer.AlternatePhone;
            }
            else
            {
                CustomerNameTextBox.Text = "";
                CustomerAddressTextBox.Text = "";
                AlternatePhoneTextBox.Text = "";
            }
        }

        string currentNote = "";
        private void note_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentNote = note.Text;
        }

        private void AlternatePhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void AddDeliveryPersonButton_Click(object sender, RoutedEventArgs e)
        {
            OrderSummaryCashier orderSummaryCashier = new OrderSummaryCashier();
            orderSummaryCashier.Show();
        }

        private decimal deliveryCost = 0;

        private void DF_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(DF.Text, out decimal cost))
                deliveryCost = cost;
            else
                deliveryCost = 0;
        }
    }

    [NotMapped]
    public class DisplayOrderItem
    {
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public override string ToString() => $"{ItemName} (x{Quantity}) - {Price * Quantity:C}";
    }
}
