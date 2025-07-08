using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class ReturnService
    {
        private ProductService productService = new ProductService();
        private UserService userService = new UserService();

        public string GenerateReturnNumber()
        {
            return $"RET-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        public int CreateReturn(Return returnItem)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // إنشاء المرتجع
                        string returnQuery = @"
                            INSERT INTO Returns (ReturnNumber, InvoiceId, ProductId, Quantity, UnitPrice, TotalAmount, 
                                               Reason, Type, Status, ProcessedBy, Notes)
                            VALUES (@returnNumber, @invoiceId, @productId, @quantity, @unitPrice, @totalAmount, 
                                   @reason, @type, @status, @processedBy, @notes);
                            SELECT last_insert_rowid();";

                        int returnId;
                        using (var command = new SQLiteCommand(returnQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@returnNumber", returnItem.ReturnNumber);
                            command.Parameters.AddWithValue("@invoiceId", returnItem.InvoiceId);
                            command.Parameters.AddWithValue("@productId", returnItem.ProductId);
                            command.Parameters.AddWithValue("@quantity", returnItem.Quantity);
                            command.Parameters.AddWithValue("@unitPrice", returnItem.UnitPrice);
                            command.Parameters.AddWithValue("@totalAmount", returnItem.TotalAmount);
                            command.Parameters.AddWithValue("@reason", returnItem.Reason ?? "");
                            command.Parameters.AddWithValue("@type", (int)returnItem.Type);
                            command.Parameters.AddWithValue("@status", (int)returnItem.Status);
                            command.Parameters.AddWithValue("@processedBy", returnItem.ProcessedBy);
                            command.Parameters.AddWithValue("@notes", returnItem.Notes ?? "");
                            
                            returnId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // إذا كان المرتجع معتمد، إعادة المنتج للمخزون
                        if (returnItem.Status == ReturnStatus.Approved)
                        {
                            string updateStockQuery = @"
                                UPDATE Products 
                                SET CurrentQuantity = CurrentQuantity + @quantity 
                                WHERE Id = @productId";

                            using (var command = new SQLiteCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@quantity", returnItem.Quantity);
                                command.Parameters.AddWithValue("@productId", returnItem.ProductId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        
                        // تسجيل العملية
                        userService.LogActivity(UserService.CurrentUser?.Id, "إنشاء مرتجع", "Returns", returnId);
                        
                        return returnId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Return> GetAllReturns()
        {
            List<Return> returns = new List<Return>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT r.*, i.InvoiceNumber, p.Name as ProductName, p.Barcode as ProductBarcode, u.FullName as ProcessedByName
                    FROM Returns r
                    LEFT JOIN Invoices i ON r.InvoiceId = i.Id
                    LEFT JOIN Products p ON r.ProductId = p.Id
                    LEFT JOIN Users u ON r.ProcessedBy = u.Id
                    ORDER BY r.ReturnDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        returns.Add(MapReaderToReturn(reader));
                    }
                }
            }

            return returns;
        }

        public Return GetReturnById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT r.*, i.InvoiceNumber, p.Name as ProductName, p.Barcode as ProductBarcode, u.FullName as ProcessedByName
                    FROM Returns r
                    LEFT JOIN Invoices i ON r.InvoiceId = i.Id
                    LEFT JOIN Products p ON r.ProductId = p.Id
                    LEFT JOIN Users u ON r.ProcessedBy = u.Id
                    WHERE r.Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToReturn(reader);
                        }
                    }
                }
            }

            return null;
        }

        public List<Return> GetReturnsByInvoice(int invoiceId)
        {
            List<Return> returns = new List<Return>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT r.*, i.InvoiceNumber, p.Name as ProductName, p.Barcode as ProductBarcode, u.FullName as ProcessedByName
                    FROM Returns r
                    LEFT JOIN Invoices i ON r.InvoiceId = i.Id
                    LEFT JOIN Products p ON r.ProductId = p.Id
                    LEFT JOIN Users u ON r.ProcessedBy = u.Id
                    WHERE r.InvoiceId = @invoiceId
                    ORDER BY r.ReturnDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceId", invoiceId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returns.Add(MapReaderToReturn(reader));
                        }
                    }
                }
            }

            return returns;
        }

        public List<Return> GetReturnsByDateRange(DateTime fromDate, DateTime toDate)
        {
            List<Return> returns = new List<Return>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT r.*, i.InvoiceNumber, p.Name as ProductName, p.Barcode as ProductBarcode, u.FullName as ProcessedByName
                    FROM Returns r
                    LEFT JOIN Invoices i ON r.InvoiceId = i.Id
                    LEFT JOIN Products p ON r.ProductId = p.Id
                    LEFT JOIN Users u ON r.ProcessedBy = u.Id
                    WHERE DATE(r.ReturnDate) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                    ORDER BY r.ReturnDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returns.Add(MapReaderToReturn(reader));
                        }
                    }
                }
            }

            return returns;
        }

        public bool UpdateReturnStatus(int returnId, ReturnStatus newStatus, string notes = null)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // الحصول على بيانات المرتجع الحالية
                        var returnItem = GetReturnById(returnId);
                        if (returnItem == null) return false;

                        // تحديث حالة المرتجع
                        string updateQuery = @"
                            UPDATE Returns 
                            SET Status = @status, Notes = @notes
                            WHERE Id = @id";

                        using (var command = new SQLiteCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@status", (int)newStatus);
                            command.Parameters.AddWithValue("@notes", notes ?? returnItem.Notes);
                            command.Parameters.AddWithValue("@id", returnId);
                            command.ExecuteNonQuery();
                        }

                        // إذا تم اعتماد المرتجع وكان في حالة أخرى سابقاً، إعادة المنتج للمخزون
                        if (newStatus == ReturnStatus.Approved && returnItem.Status != ReturnStatus.Approved)
                        {
                            string updateStockQuery = @"
                                UPDATE Products 
                                SET CurrentQuantity = CurrentQuantity + @quantity 
                                WHERE Id = @productId";

                            using (var command = new SQLiteCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@quantity", returnItem.Quantity);
                                command.Parameters.AddWithValue("@productId", returnItem.ProductId);
                                command.ExecuteNonQuery();
                            }
                        }
                        // إذا تم رفض المرتجع وكان معتمد سابقاً، خصم المنتج من المخزون
                        else if (newStatus != ReturnStatus.Approved && returnItem.Status == ReturnStatus.Approved)
                        {
                            string updateStockQuery = @"
                                UPDATE Products 
                                SET CurrentQuantity = CurrentQuantity - @quantity 
                                WHERE Id = @productId";

                            using (var command = new SQLiteCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@quantity", returnItem.Quantity);
                                command.Parameters.AddWithValue("@productId", returnItem.ProductId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        
                        // تسجيل العملية
                        userService.LogActivity(UserService.CurrentUser?.Id, $"تحديث حالة مرتجع إلى {GetStatusName(newStatus)}", "Returns", returnId);
                        
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public decimal GetTotalReturnsAmount(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT COALESCE(SUM(TotalAmount), 0) 
                    FROM Returns 
                    WHERE Status = @status";

                if (fromDate.HasValue && toDate.HasValue)
                {
                    query += " AND DATE(ReturnDate) BETWEEN DATE(@fromDate) AND DATE(@toDate)";
                }

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", (int)ReturnStatus.Approved);
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
                    }
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }

        public List<dynamic> GetTopReturnedProducts(int limit = 10)
        {
            var topReturned = new List<dynamic>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.Name, SUM(r.Quantity) as TotalReturned, SUM(r.TotalAmount) as TotalAmount
                    FROM Returns r
                    JOIN Products p ON r.ProductId = p.Id
                    WHERE r.Status = @status
                    GROUP BY p.Id, p.Name
                    ORDER BY TotalReturned DESC
                    LIMIT @limit";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", (int)ReturnStatus.Approved);
                    command.Parameters.AddWithValue("@limit", limit);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topReturned.Add(new
                            {
                                Name = reader.GetString("Name"),
                                TotalReturned = reader.GetInt32("TotalReturned"),
                                TotalAmount = reader.GetDecimal("TotalAmount")
                            });
                        }
                    }
                }
            }

            return topReturned;
        }

        private string GetStatusName(ReturnStatus status)
        {
            return status switch
            {
                ReturnStatus.Pending => "في الانتظار",
                ReturnStatus.Approved => "معتمد",
                ReturnStatus.Rejected => "مرفوض",
                ReturnStatus.Completed => "مكتمل",
                _ => "غير محدد"
            };
        }

        private Return MapReaderToReturn(SQLiteDataReader reader)
        {
            return new Return
            {
                Id = reader.GetInt32("Id"),
                ReturnNumber = reader.GetString("ReturnNumber"),
                InvoiceId = reader.GetInt32("InvoiceId"),
                ProductId = reader.GetInt32("ProductId"),
                Quantity = reader.GetInt32("Quantity"),
                UnitPrice = reader.GetDecimal("UnitPrice"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                Reason = reader.IsDBNull("Reason") ? "" : reader.GetString("Reason"),
                Type = (ReturnType)reader.GetInt32("Type"),
                Status = (ReturnStatus)reader.GetInt32("Status"),
                ProcessedBy = reader.GetInt32("ProcessedBy"),
                ReturnDate = reader.GetDateTime("ReturnDate"),
                Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes"),
                InvoiceNumber = reader.IsDBNull("InvoiceNumber") ? "" : reader.GetString("InvoiceNumber"),
                ProductName = reader.IsDBNull("ProductName") ? "" : reader.GetString("ProductName"),
                ProductBarcode = reader.IsDBNull("ProductBarcode") ? "" : reader.GetString("ProductBarcode"),
                ProcessedByName = reader.IsDBNull("ProcessedByName") ? "" : reader.GetString("ProcessedByName")
            };
        }
    }
}