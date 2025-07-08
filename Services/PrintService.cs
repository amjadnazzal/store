using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class PrintService
    {
        private PrintDocument printDocument;
        private Invoice currentInvoice;
        private string currentReport;
        private Font titleFont = new Font("Arial", 16, FontStyle.Bold);
        private Font headerFont = new Font("Arial", 12, FontStyle.Bold);
        private Font normalFont = new Font("Arial", 10);
        private Font smallFont = new Font("Arial", 8);

        public PrintService()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        public void PrintInvoice(Invoice invoice)
        {
            currentInvoice = invoice;
            currentReport = null;

            try
            {
                // إعداد خصائص الطباعة
                printDocument.DocumentName = $"فاتورة {invoice.InvoiceNumber}";
                
                // عرض معاينة الطباعة
                PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                previewDialog.Document = printDocument;
                previewDialog.WindowState = FormWindowState.Maximized;
                previewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PrintReport(string reportContent, string reportTitle)
        {
            currentInvoice = null;
            currentReport = reportContent;

            try
            {
                printDocument.DocumentName = reportTitle;
                
                PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                previewDialog.Document = printDocument;
                previewDialog.WindowState = FormWindowState.Maximized;
                previewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في طباعة التقرير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DirectPrintInvoice(Invoice invoice)
        {
            currentInvoice = invoice;
            currentReport = null;

            try
            {
                printDocument.DocumentName = $"فاتورة {invoice.InvoiceNumber}";
                printDocument.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الطباعة المباشرة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentInvoice != null)
            {
                PrintInvoiceContent(e);
            }
            else if (currentReport != null)
            {
                PrintReportContent(e);
            }
        }

        private void PrintInvoiceContent(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            float yPosition = 50;
            float leftMargin = 50;
            float rightMargin = e.PageBounds.Width - 50;
            float centerX = e.PageBounds.Width / 2;

            // رأس الفاتورة
            string storeName = GetStoreName();
            SizeF storeNameSize = g.MeasureString(storeName, titleFont);
            g.DrawString(storeName, titleFont, Brushes.Black, centerX - storeNameSize.Width / 2, yPosition);
            yPosition += storeNameSize.Height + 10;

            string storeInfo = GetStoreInfo();
            if (!string.IsNullOrEmpty(storeInfo))
            {
                SizeF storeInfoSize = g.MeasureString(storeInfo, normalFont);
                g.DrawString(storeInfo, normalFont, Brushes.Black, centerX - storeInfoSize.Width / 2, yPosition);
                yPosition += storeInfoSize.Height + 20;
            }

            // خط فاصل
            g.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
            yPosition += 20;

            // معلومات الفاتورة
            g.DrawString($"رقم الفاتورة: {currentInvoice.InvoiceNumber}", headerFont, Brushes.Black, leftMargin, yPosition);
            g.DrawString($"التاريخ: {currentInvoice.InvoiceDate:yyyy/MM/dd HH:mm}", normalFont, Brushes.Black, rightMargin - 150, yPosition);
            yPosition += 40;

            if (!string.IsNullOrEmpty(currentInvoice.CustomerName))
            {
                g.DrawString($"العميل: {currentInvoice.CustomerName}", normalFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;
            }

            // رأس جدول المنتجات
            yPosition += 10;
            g.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
            yPosition += 10;

            g.DrawString("المنتج", headerFont, Brushes.Black, leftMargin, yPosition);
            g.DrawString("الكمية", headerFont, Brushes.Black, leftMargin + 250, yPosition);
            g.DrawString("السعر", headerFont, Brushes.Black, leftMargin + 350, yPosition);
            g.DrawString("المجموع", headerFont, Brushes.Black, leftMargin + 450, yPosition);
            yPosition += 30;

            g.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
            yPosition += 10;

            // المنتجات
            foreach (var item in currentInvoice.Items)
            {
                g.DrawString(item.ProductName, normalFont, Brushes.Black, leftMargin, yPosition);
                g.DrawString(item.Quantity.ToString(), normalFont, Brushes.Black, leftMargin + 250, yPosition);
                g.DrawString(item.UnitPrice.ToString("C2"), normalFont, Brushes.Black, leftMargin + 350, yPosition);
                g.DrawString(item.TotalPrice.ToString("C2"), normalFont, Brushes.Black, leftMargin + 450, yPosition);
                yPosition += 25;
            }

            yPosition += 10;
            g.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
            yPosition += 20;

            // المجاميع
            g.DrawString($"المجموع الفرعي: {currentInvoice.TotalAmount:C2}", normalFont, Brushes.Black, leftMargin + 300, yPosition);
            yPosition += 25;

            if (currentInvoice.DiscountAmount > 0)
            {
                g.DrawString($"الخصم: {currentInvoice.DiscountAmount:C2}", normalFont, Brushes.Black, leftMargin + 300, yPosition);
                yPosition += 25;
            }

            g.DrawString($"المجموع الإجمالي: {currentInvoice.FinalAmount:C2}", headerFont, Brushes.Black, leftMargin + 300, yPosition);
            yPosition += 30;

            g.DrawString($"طريقة الدفع: {currentInvoice.PaymentMethod}", normalFont, Brushes.Black, leftMargin + 300, yPosition);
            yPosition += 40;

            // رسالة الشكر
            string welcomeMessage = GetWelcomeMessage();
            if (!string.IsNullOrEmpty(welcomeMessage))
            {
                SizeF messageSize = g.MeasureString(welcomeMessage, normalFont);
                g.DrawString(welcomeMessage, normalFont, Brushes.Black, centerX - messageSize.Width / 2, yPosition);
            }
        }

        private void PrintReportContent(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            float yPosition = 50;
            float leftMargin = 50;

            string[] lines = currentReport.Split('\n');
            
            foreach (string line in lines)
            {
                if (yPosition > e.PageBounds.Height - 100)
                {
                    e.HasMorePages = true;
                    return;
                }

                g.DrawString(line, normalFont, Brushes.Black, leftMargin, yPosition);
                yPosition += normalFont.GetHeight() + 5;
            }

            e.HasMorePages = false;
        }

        public string GenerateInvoiceReceipt(Invoice invoice)
        {
            var receipt = new StringBuilder();
            string storeName = GetStoreName();
            string storeInfo = GetStoreInfo();

            // رأس الإيصال
            receipt.AppendLine(new string('=', 40));
            receipt.AppendLine(CenterText(storeName, 40));
            if (!string.IsNullOrEmpty(storeInfo))
            {
                receipt.AppendLine(CenterText(storeInfo, 40));
            }
            receipt.AppendLine(new string('=', 40));

            // معلومات الفاتورة
            receipt.AppendLine($"رقم الفاتورة: {invoice.InvoiceNumber}");
            receipt.AppendLine($"التاريخ: {invoice.InvoiceDate:yyyy/MM/dd HH:mm}");
            if (!string.IsNullOrEmpty(invoice.CustomerName))
            {
                receipt.AppendLine($"العميل: {invoice.CustomerName}");
            }
            receipt.AppendLine(new string('-', 40));

            // المنتجات
            foreach (var item in invoice.Items)
            {
                receipt.AppendLine($"{item.ProductName}");
                receipt.AppendLine($"  {item.Quantity} x {item.UnitPrice:C2} = {item.TotalPrice:C2}");
            }

            receipt.AppendLine(new string('-', 40));

            // المجاميع
            receipt.AppendLine($"المجموع الفرعي: {invoice.TotalAmount:C2}");
            if (invoice.DiscountAmount > 0)
            {
                receipt.AppendLine($"الخصم: {invoice.DiscountAmount:C2}");
            }
            receipt.AppendLine($"المجموع الإجمالي: {invoice.FinalAmount:C2}");
            receipt.AppendLine($"طريقة الدفع: {invoice.PaymentMethod}");

            receipt.AppendLine(new string('=', 40));
            
            string welcomeMessage = GetWelcomeMessage();
            if (!string.IsNullOrEmpty(welcomeMessage))
            {
                receipt.AppendLine(CenterText(welcomeMessage, 40));
            }

            return receipt.ToString();
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int spaces = (width - text.Length) / 2;
            return new string(' ', spaces) + text;
        }

        private string GetStoreName()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT StoreName FROM Settings WHERE Id = 1";
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "متجر الملابس";
                    }
                }
            }
            catch
            {
                return "متجر الملابس";
            }
        }

        private string GetStoreInfo()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT StoreAddress, StorePhone FROM Settings WHERE Id = 1";
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var address = reader.IsDBNull("StoreAddress") ? "" : reader.GetString("StoreAddress");
                            var phone = reader.IsDBNull("StorePhone") ? "" : reader.GetString("StorePhone");
                            
                            var info = new StringBuilder();
                            if (!string.IsNullOrEmpty(address))
                                info.AppendLine(address);
                            if (!string.IsNullOrEmpty(phone))
                                info.AppendLine($"هاتف: {phone}");
                            
                            return info.ToString().Trim();
                        }
                    }
                }
            }
            catch
            {
                // تجاهل الأخطاء
            }
            return "";
        }

        private string GetWelcomeMessage()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT WelcomeMessage FROM Settings WHERE Id = 1";
                    using (var command = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "شكراً لزيارتكم";
                    }
                }
            }
            catch
            {
                return "شكراً لزيارتكم";
            }
        }

        public void Dispose()
        {
            titleFont?.Dispose();
            headerFont?.Dispose();
            normalFont?.Dispose();
            smallFont?.Dispose();
            printDocument?.Dispose();
        }
    }
}