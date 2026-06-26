using Restaurant_System.Models;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Restaurant_System.Helpers
{
    public static class ReceiptPrinter
    {
        public static void PrintOrder(OrderSummaryViewModel order)
        {
            var document = new FlowDocument
            {
                FontSize = 14,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                PageWidth = 300,
                PagePadding = new Thickness(20)
            };

            var title = new Paragraph(new Run("إيصال الطلب"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                TextAlignment = TextAlignment.Center
            };
            document.Blocks.Add(title);

            var info = new Paragraph(new Run(
                $"رقم الطلب: {order.Id}\n" +
                $"التاريخ: {order.Date}\n" +
                $"نوع الطلب: {order.OrderType}\n" +
                $"العميل: {order.CustomerName}\n" +
                $"الطاولة: {order.TableId}\n" +
                $"المندوب: {order.DeliveryPerson ?? "—"}\n"
            ));
            document.Blocks.Add(info);

            var itemsHeader = new Paragraph(new Run("تفاصيل الأصناف:"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            document.Blocks.Add(itemsHeader);

            foreach (var item in order.OrderItems)
            {
                decimal subtotal = item.Price * item.Quantity;
                var itemLine = new Paragraph(new Run($"{item.ItemName} × {item.Quantity} = {subtotal} جنيه"))
                {
                    Margin = new Thickness(0, 0, 0, 2)
                };
                document.Blocks.Add(itemLine);
            }

            var total = new Paragraph(new Run($"\nالإجمالي: {order.TotalPrice} جنيه"))
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right
            };
            document.Blocks.Add(total);

            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = document;
                pd.PrintDocument(idpSource.DocumentPaginator, "إيصال الطلب");
            }
        }
    }
}
