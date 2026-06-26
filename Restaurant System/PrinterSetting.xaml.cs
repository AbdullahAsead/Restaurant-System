using System;
using System.Windows;

namespace Restaurant_System
{
    public partial class PrinterSetting : Window
    {
        public PrinterSetting()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // نقرأ الإعدادات المحفوظة إن وجدت، وإلا نضع الافتراضي false
                chkEnableKitchenPrinter.IsChecked = Properties.Settings.Default.KitchenPrinterEnabled;
                chkEnableTwoReceipts.IsChecked = Properties.Settings.Default.DoubleReceiptEnabled;
            }
            catch
            {
                // لو الخاصيتين غير معرفتين في Settings.settings — نعرض القيم الافتراضية بدون رمي استثناء
                chkEnableKitchenPrinter.IsChecked = false;
                chkEnableTwoReceipts.IsChecked = false;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // نحاول حفظ الإعدادات — تأكد أن الإعدادات موجودة في Settings.settings
                Properties.Settings.Default.KitchenPrinterEnabled = chkEnableKitchenPrinter.IsChecked ?? false;
                Properties.Settings.Default.DoubleReceiptEnabled = chkEnableTwoReceipts.IsChecked ?? false;
                Properties.Settings.Default.Save();

                MessageBox.Show("✅ تم حفظ الإعدادات بنجاح!", "تم الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء حفظ الإعدادات. تأكد من أن خصائص Settings موجودة.\n\n" + ex.Message,
                                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
