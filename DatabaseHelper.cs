using System;
using System.Data.SQLite;
using System.IO;

namespace ClothingStoreManager
{
    public static class DatabaseHelper
    {
        private static readonly string DatabasePath = "ClothingStore.db";
        private static readonly string ConnectionString = $"Data Source={DatabasePath};Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(DatabasePath))
                {
                    SQLiteConnection.CreateFile(DatabasePath);
                }

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    CreateTables(connection);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تهيئة قاعدة البيانات: {ex.Message}");
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            string[] createTableQueries = {
                // جدول الموردين
                @"CREATE TABLE IF NOT EXISTS Suppliers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    Address TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                )",

                // جدول العملاء
                @"CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Phone TEXT,
                    Address TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                )",

                // جدول المنتجات
                @"CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Barcode TEXT UNIQUE NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Size TEXT,
                    Color TEXT,
                    SupplierId INTEGER,
                    PurchasePrice DECIMAL(10,2),
                    SalePrice DECIMAL(10,2),
                    CurrentQuantity INTEGER DEFAULT 0,
                    ImagePath TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                )",

                // جدول الفواتير
                @"CREATE TABLE IF NOT EXISTS Invoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT UNIQUE NOT NULL,
                    CustomerId INTEGER,
                    TotalAmount DECIMAL(10,2),
                    DiscountAmount DECIMAL(10,2) DEFAULT 0,
                    FinalAmount DECIMAL(10,2),
                    PaymentMethod TEXT,
                    InvoiceDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                )",

                // جدول تفاصيل الفواتير
                @"CREATE TABLE IF NOT EXISTS InvoiceItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER,
                    ProductId INTEGER,
                    Quantity INTEGER,
                    UnitPrice DECIMAL(10,2),
                    TotalPrice DECIMAL(10,2),
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )",

                // جدول المرتجعات
                @"CREATE TABLE IF NOT EXISTS Returns (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER,
                    ProductId INTEGER,
                    Quantity INTEGER,
                    Reason TEXT,
                    ReturnDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )",

                // جدول الإعدادات
                @"CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY,
                    StoreName TEXT,
                    StoreAddress TEXT,
                    WelcomeMessage TEXT,
                    LowStockAlert INTEGER DEFAULT 5
                )"
            };

            foreach (string query in createTableQueries)
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // إدراج الإعدادات الافتراضية إذا لم تكن موجودة
            using (var command = new SQLiteCommand(
                "INSERT OR IGNORE INTO Settings (Id, StoreName, StoreAddress, WelcomeMessage, LowStockAlert) " +
                "VALUES (1, 'متجر الملابس', '', 'شكراً لزيارتكم', 5)", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public static void BackupDatabase(string backupPath)
        {
            try
            {
                File.Copy(DatabasePath, backupPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}");
            }
        }

        public static void RestoreDatabase(string backupPath)
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, DatabasePath, true);
                }
                else
                {
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}");
            }
        }
    }
}