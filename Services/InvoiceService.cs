using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class InvoiceService
    {
        private ProductService productService = new ProductService();

        public string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        public int CreateInvoice(Invoice invoice)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // إنشاء الفاتورة
                        string invoiceQuery = @"
                            INSERT INTO Invoices (InvoiceNumber, CustomerId, TotalAmount, DiscountAmount, FinalAmount, PaymentMethod)
                            VALUES (@invoiceNumber, @customerId, @totalAmount, @discountAmount, @finalAmount, @paymentMethod);
                            SELECT last_insert_rowid();";

                        int invoiceId;
                        using (var command = new SQLiteCommand(invoiceQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@invoiceNumber", invoice.InvoiceNumber);
                            command.Parameters.AddWithValue("@customerId", (object)invoice.CustomerId ?? DBNull.Value);
                            command.Parameters.AddWithValue("@totalAmount", invoice.TotalAmount);
                            command.Parameters.AddWithValue("@discountAmount", invoice.DiscountAmount);
                            command.Parameters.AddWithValue("@finalAmount", invoice.FinalAmount);
                            command.Parameters.AddWithValue("@paymentMethod", invoice.PaymentMethod);
                            
                            invoiceId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // إضافة عناصر الفاتورة وتحديث المخزون
                        foreach (var item in invoice.Items)
                        {
                            // إضافة عنصر الفاتورة
                            string itemQuery = @"
                                INSERT INTO InvoiceItems (InvoiceId, ProductId, Quantity, UnitPrice, TotalPrice)
                                VALUES (@invoiceId, @productId, @quantity, @unitPrice, @totalPrice)";

                            using (var command = new SQLiteCommand(itemQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@invoiceId", invoiceId);
                                command.Parameters.AddWithValue("@productId", item.ProductId);
                                command.Parameters.AddWithValue("@quantity", item.Quantity);
                                command.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@totalPrice", item.TotalPrice);
                                command.ExecuteNonQuery();
                            }

                            // تحديث كمية المنتج في المخزون
                            string updateStockQuery = @"
                                UPDATE Products 
                                SET CurrentQuantity = CurrentQuantity - @quantity 
                                WHERE Id = @productId";

                            using (var command = new SQLiteCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@quantity", item.Quantity);
                                command.Parameters.AddWithValue("@productId", item.ProductId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return invoiceId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public Invoice GetInvoiceById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // الحصول على بيانات الفاتورة
                string invoiceQuery = @"
                    SELECT i.*, c.Name as CustomerName 
                    FROM Invoices i 
                    LEFT JOIN Customers c ON i.CustomerId = c.Id 
                    WHERE i.Id = @id";

                Invoice invoice = null;
                using (var command = new SQLiteCommand(invoiceQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            invoice = MapReaderToInvoice(reader);
                        }
                    }
                }

                if (invoice != null)
                {
                    // الحصول على عناصر الفاتورة
                    string itemsQuery = @"
                        SELECT ii.*, p.Name as ProductName, p.Barcode as ProductBarcode
                        FROM InvoiceItems ii
                        JOIN Products p ON ii.ProductId = p.Id
                        WHERE ii.InvoiceId = @invoiceId";

                    using (var command = new SQLiteCommand(itemsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@invoiceId", invoice.Id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                invoice.Items.Add(MapReaderToInvoiceItem(reader));
                            }
                        }
                    }
                }

                return invoice;
            }
        }

        public Invoice GetInvoiceByNumber(string invoiceNumber)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                string query = @"
                    SELECT i.*, c.Name as CustomerName 
                    FROM Invoices i 
                    LEFT JOIN Customers c ON i.CustomerId = c.Id 
                    WHERE i.InvoiceNumber = @invoiceNumber";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var invoice = MapReaderToInvoice(reader);
                            // يمكن إضافة تحميل العناصر هنا إذا لزم الأمر
                            return invoice;
                        }
                    }
                }
            }

            return null;
        }

        public List<Invoice> GetInvoicesByDateRange(DateTime fromDate, DateTime toDate)
        {
            List<Invoice> invoices = new List<Invoice>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT i.*, c.Name as CustomerName 
                    FROM Invoices i 
                    LEFT JOIN Customers c ON i.CustomerId = c.Id 
                    WHERE DATE(i.InvoiceDate) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                    ORDER BY i.InvoiceDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(MapReaderToInvoice(reader));
                        }
                    }
                }
            }

            return invoices;
        }

        public List<Invoice> GetTodayInvoices()
        {
            return GetInvoicesByDateRange(DateTime.Today, DateTime.Today);
        }

        public decimal GetTodayTotalSales()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT COALESCE(SUM(FinalAmount), 0) 
                    FROM Invoices 
                    WHERE DATE(InvoiceDate) = DATE('now')";

                using (var command = new SQLiteCommand(query, connection))
                {
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }

        public int GetTodayInvoiceCount()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT COUNT(*) 
                    FROM Invoices 
                    WHERE DATE(InvoiceDate) = DATE('now')";

                using (var command = new SQLiteCommand(query, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<dynamic> GetTopSellingProducts(int limit = 10)
        {
            var topProducts = new List<dynamic>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.Name, SUM(ii.Quantity) as TotalSold, SUM(ii.TotalPrice) as TotalRevenue
                    FROM InvoiceItems ii
                    JOIN Products p ON ii.ProductId = p.Id
                    JOIN Invoices i ON ii.InvoiceId = i.Id
                    WHERE DATE(i.InvoiceDate) >= DATE('now', '-30 days')
                    GROUP BY p.Id, p.Name
                    ORDER BY TotalSold DESC
                    LIMIT @limit";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@limit", limit);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topProducts.Add(new
                            {
                                Name = reader.GetString("Name"),
                                TotalSold = reader.GetInt32("TotalSold"),
                                TotalRevenue = reader.GetDecimal("TotalRevenue")
                            });
                        }
                    }
                }
            }

            return topProducts;
        }

        private Invoice MapReaderToInvoice(SQLiteDataReader reader)
        {
            return new Invoice
            {
                Id = reader.GetInt32("Id"),
                InvoiceNumber = reader.GetString("InvoiceNumber"),
                CustomerId = reader.IsDBNull("CustomerId") ? (int?)null : reader.GetInt32("CustomerId"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                DiscountAmount = reader.GetDecimal("DiscountAmount"),
                FinalAmount = reader.GetDecimal("FinalAmount"),
                PaymentMethod = reader.GetString("PaymentMethod"),
                InvoiceDate = reader.GetDateTime("InvoiceDate"),
                CustomerName = reader.IsDBNull("CustomerName") ? "" : reader.GetString("CustomerName")
            };
        }

        private InvoiceItem MapReaderToInvoiceItem(SQLiteDataReader reader)
        {
            return new InvoiceItem
            {
                Id = reader.GetInt32("Id"),
                InvoiceId = reader.GetInt32("InvoiceId"),
                ProductId = reader.GetInt32("ProductId"),
                Quantity = reader.GetInt32("Quantity"),
                UnitPrice = reader.GetDecimal("UnitPrice"),
                TotalPrice = reader.GetDecimal("TotalPrice"),
                ProductName = reader.GetString("ProductName"),
                ProductBarcode = reader.GetString("ProductBarcode")
            };
        }
    }
}