using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class ReportService
    {
        private UserService userService = new UserService();

        public class SalesReport
        {
            public DateTime Date { get; set; }
            public int InvoiceCount { get; set; }
            public decimal TotalSales { get; set; }
            public decimal TotalDiscount { get; set; }
            public decimal NetSales { get; set; }
            public decimal TotalProfit { get; set; }
        }

        public class ProductReport
        {
            public string ProductName { get; set; }
            public string Barcode { get; set; }
            public int QuantitySold { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal TotalProfit { get; set; }
            public int CurrentStock { get; set; }
        }

        public class CustomerReport
        {
            public string CustomerName { get; set; }
            public string Phone { get; set; }
            public int TotalPurchases { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime LastPurchase { get; set; }
        }

        public class DashboardData
        {
            public decimal TodaySales { get; set; }
            public int TodayInvoices { get; set; }
            public decimal MonthSales { get; set; }
            public int LowStockCount { get; set; }
            public int TotalProducts { get; set; }
            public int TotalCustomers { get; set; }
            public decimal TotalReturns { get; set; }
            public List<dynamic> TopProducts { get; set; }
            public List<SalesReport> WeeklySales { get; set; }
        }

        public DashboardData GetDashboardData()
        {
            var dashboard = new DashboardData();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // مبيعات اليوم
                string todaySalesQuery = @"
                    SELECT COALESCE(SUM(FinalAmount), 0) as TodaySales, COUNT(*) as TodayInvoices
                    FROM Invoices 
                    WHERE DATE(InvoiceDate) = DATE('now')";

                using (var command = new SQLiteCommand(todaySalesQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dashboard.TodaySales = reader.GetDecimal("TodaySales");
                        dashboard.TodayInvoices = reader.GetInt32("TodayInvoices");
                    }
                }

                // مبيعات الشهر
                string monthSalesQuery = @"
                    SELECT COALESCE(SUM(FinalAmount), 0) as MonthSales
                    FROM Invoices 
                    WHERE strftime('%Y-%m', InvoiceDate) = strftime('%Y-%m', 'now')";

                using (var command = new SQLiteCommand(monthSalesQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    dashboard.MonthSales = Convert.ToDecimal(result ?? 0);
                }

                // المنتجات منخفضة المخزون
                string lowStockQuery = @"
                    SELECT COUNT(*) as LowStockCount
                    FROM Products 
                    WHERE CurrentQuantity <= (SELECT LowStockAlert FROM Settings WHERE Id = 1)";

                using (var command = new SQLiteCommand(lowStockQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    dashboard.LowStockCount = Convert.ToInt32(result ?? 0);
                }

                // إجمالي المنتجات
                string totalProductsQuery = "SELECT COUNT(*) FROM Products";
                using (var command = new SQLiteCommand(totalProductsQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    dashboard.TotalProducts = Convert.ToInt32(result ?? 0);
                }

                // إجمالي العملاء
                string totalCustomersQuery = "SELECT COUNT(*) FROM Customers";
                using (var command = new SQLiteCommand(totalCustomersQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    dashboard.TotalCustomers = Convert.ToInt32(result ?? 0);
                }

                // إجمالي المرتجعات
                string totalReturnsQuery = @"
                    SELECT COALESCE(SUM(TotalAmount), 0) 
                    FROM Returns 
                    WHERE Status = @status AND DATE(ReturnDate) = DATE('now')";

                using (var command = new SQLiteCommand(totalReturnsQuery, connection))
                {
                    command.Parameters.AddWithValue("@status", (int)ReturnStatus.Approved);
                    var result = command.ExecuteScalar();
                    dashboard.TotalReturns = Convert.ToDecimal(result ?? 0);
                }

                // أفضل المنتجات مبيعاً
                dashboard.TopProducts = GetTopSellingProducts(5);

                // مبيعات الأسبوع
                dashboard.WeeklySales = GetWeeklySalesReport();
            }

            return dashboard;
        }

        public List<SalesReport> GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            var salesReport = new List<SalesReport>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        DATE(i.InvoiceDate) as Date,
                        COUNT(*) as InvoiceCount,
                        SUM(i.TotalAmount) as TotalSales,
                        SUM(i.DiscountAmount) as TotalDiscount,
                        SUM(i.FinalAmount) as NetSales,
                        SUM((ii.UnitPrice - p.PurchasePrice) * ii.Quantity) as TotalProfit
                    FROM Invoices i
                    LEFT JOIN InvoiceItems ii ON i.Id = ii.InvoiceId
                    LEFT JOIN Products p ON ii.ProductId = p.Id
                    WHERE DATE(i.InvoiceDate) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                    GROUP BY DATE(i.InvoiceDate)
                    ORDER BY Date";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salesReport.Add(new SalesReport
                            {
                                Date = reader.GetDateTime("Date"),
                                InvoiceCount = reader.GetInt32("InvoiceCount"),
                                TotalSales = reader.GetDecimal("TotalSales"),
                                TotalDiscount = reader.GetDecimal("TotalDiscount"),
                                NetSales = reader.GetDecimal("NetSales"),
                                TotalProfit = reader.IsDBNull("TotalProfit") ? 0 : reader.GetDecimal("TotalProfit")
                            });
                        }
                    }
                }
            }

            return salesReport;
        }

        public List<SalesReport> GetWeeklySalesReport()
        {
            var fromDate = DateTime.Today.AddDays(-6);
            var toDate = DateTime.Today;
            return GetSalesReport(fromDate, toDate);
        }

        public List<ProductReport> GetProductReport(DateTime fromDate, DateTime toDate)
        {
            var productReport = new List<ProductReport>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        p.Name as ProductName,
                        p.Barcode,
                        p.CurrentQuantity as CurrentStock,
                        COALESCE(SUM(ii.Quantity), 0) as QuantitySold,
                        COALESCE(SUM(ii.TotalPrice), 0) as TotalRevenue,
                        COALESCE(SUM((ii.UnitPrice - p.PurchasePrice) * ii.Quantity), 0) as TotalProfit
                    FROM Products p
                    LEFT JOIN InvoiceItems ii ON p.Id = ii.ProductId
                    LEFT JOIN Invoices i ON ii.InvoiceId = i.Id
                    WHERE i.InvoiceDate IS NULL OR DATE(i.InvoiceDate) BETWEEN DATE(@fromDate) AND DATE(@toDate)
                    GROUP BY p.Id, p.Name, p.Barcode, p.CurrentQuantity
                    ORDER BY QuantitySold DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productReport.Add(new ProductReport
                            {
                                ProductName = reader.GetString("ProductName"),
                                Barcode = reader.GetString("Barcode"),
                                CurrentStock = reader.GetInt32("CurrentStock"),
                                QuantitySold = reader.GetInt32("QuantitySold"),
                                TotalRevenue = reader.GetDecimal("TotalRevenue"),
                                TotalProfit = reader.GetDecimal("TotalProfit")
                            });
                        }
                    }
                }
            }

            return productReport;
        }

        public List<CustomerReport> GetCustomerReport()
        {
            var customerReport = new List<CustomerReport>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        c.Name as CustomerName,
                        c.Phone,
                        COUNT(i.Id) as TotalPurchases,
                        COALESCE(SUM(i.FinalAmount), 0) as TotalSpent,
                        MAX(i.InvoiceDate) as LastPurchase
                    FROM Customers c
                    LEFT JOIN Invoices i ON c.Id = i.CustomerId
                    GROUP BY c.Id, c.Name, c.Phone
                    HAVING COUNT(i.Id) > 0
                    ORDER BY TotalSpent DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customerReport.Add(new CustomerReport
                        {
                            CustomerName = reader.GetString("CustomerName"),
                            Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                            TotalPurchases = reader.GetInt32("TotalPurchases"),
                            TotalSpent = reader.GetDecimal("TotalSpent"),
                            LastPurchase = reader.GetDateTime("LastPurchase")
                        });
                    }
                }
            }

            return customerReport;
        }

        public List<dynamic> GetTopSellingProducts(int limit = 10)
        {
            var topProducts = new List<dynamic>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        p.Name,
                        p.Barcode,
                        SUM(ii.Quantity) as TotalSold,
                        SUM(ii.TotalPrice) as TotalRevenue
                    FROM InvoiceItems ii
                    JOIN Products p ON ii.ProductId = p.Id
                    JOIN Invoices i ON ii.InvoiceId = i.Id
                    WHERE DATE(i.InvoiceDate) >= DATE('now', '-30 days')
                    GROUP BY p.Id, p.Name, p.Barcode
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
                                Barcode = reader.GetString("Barcode"),
                                TotalSold = reader.GetInt32("TotalSold"),
                                TotalRevenue = reader.GetDecimal("TotalRevenue")
                            });
                        }
                    }
                }
            }

            return topProducts;
        }

        public string ExportSalesReportToCSV(List<SalesReport> salesData, string fileName = null)
        {
            if (fileName == null)
                fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var csv = new StringBuilder();
            csv.AppendLine("التاريخ,عدد الفواتير,إجمالي المبيعات,إجمالي الخصم,صافي المبيعات,إجمالي الربح");

            foreach (var sale in salesData)
            {
                csv.AppendLine($"{sale.Date:yyyy-MM-dd},{sale.InvoiceCount},{sale.TotalSales},{sale.TotalDiscount},{sale.NetSales},{sale.TotalProfit}");
            }

            string filePath = Path.Combine("Reports", fileName);
            Directory.CreateDirectory("Reports");
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

            // حفظ معلومات التقرير في قاعدة البيانات
            SaveReportInfo("تقرير المبيعات", "CSV", filePath);

            return filePath;
        }

        public string ExportProductReportToCSV(List<ProductReport> productData, string fileName = null)
        {
            if (fileName == null)
                fileName = $"ProductReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var csv = new StringBuilder();
            csv.AppendLine("اسم المنتج,الباركود,الكمية المباعة,إجمالي الإيرادات,إجمالي الربح,المخزون الحالي");

            foreach (var product in productData)
            {
                csv.AppendLine($"{product.ProductName},{product.Barcode},{product.QuantitySold},{product.TotalRevenue},{product.TotalProfit},{product.CurrentStock}");
            }

            string filePath = Path.Combine("Reports", fileName);
            Directory.CreateDirectory("Reports");
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

            SaveReportInfo("تقرير المنتجات", "CSV", filePath);

            return filePath;
        }

        public string GenerateInventoryReport()
        {
            var fileName = $"InventoryReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csv = new StringBuilder();
            csv.AppendLine("اسم المنتج,الباركود,المقاس,اللون,المورد,سعر الشراء,سعر البيع,الكمية الحالية,حالة المخزون");

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName,
                           CASE 
                               WHEN p.CurrentQuantity <= (SELECT LowStockAlert FROM Settings WHERE Id = 1) THEN 'منخفض'
                               WHEN p.CurrentQuantity = 0 THEN 'نفد المخزون'
                               ELSE 'متوفر'
                           END as StockStatus
                    FROM Products p
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                    ORDER BY p.Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        csv.AppendLine($"{reader.GetString("Name")},{reader.GetString("Barcode")},{reader.GetString("Size")},{reader.GetString("Color")},{reader.GetString("SupplierName") ?? ""},{reader.GetDecimal("PurchasePrice")},{reader.GetDecimal("SalePrice")},{reader.GetInt32("CurrentQuantity")},{reader.GetString("StockStatus")}");
                    }
                }
            }

            string filePath = Path.Combine("Reports", fileName);
            Directory.CreateDirectory("Reports");
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

            SaveReportInfo("تقرير المخزون", "CSV", filePath);

            return filePath;
        }

        private void SaveReportInfo(string reportName, string reportType, string filePath)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Reports (ReportName, ReportType, GeneratedBy, FilePath)
                        VALUES (@reportName, @reportType, @generatedBy, @filePath)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@reportName", reportName);
                        command.Parameters.AddWithValue("@reportType", reportType);
                        command.Parameters.AddWithValue("@generatedBy", (object)UserService.CurrentUser?.Id ?? DBNull.Value);
                        command.Parameters.AddWithValue("@filePath", filePath);
                        command.ExecuteNonQuery();
                    }
                }

                userService.LogActivity(UserService.CurrentUser?.Id, $"إنشاء تقرير: {reportName}", "Reports");
            }
            catch
            {
                // تجاهل الأخطاء في حفظ معلومات التقرير
            }
        }
    }
}