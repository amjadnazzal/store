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

                // جدول المستخدمين
                @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT,
                    Role INTEGER NOT NULL,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLogin DATETIME,
                    CreatedBy TEXT
                )",

                // جدول المرتجعات المحدث
                @"CREATE TABLE IF NOT EXISTS Returns (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReturnNumber TEXT UNIQUE NOT NULL,
                    InvoiceId INTEGER,
                    ProductId INTEGER,
                    Quantity INTEGER,
                    UnitPrice DECIMAL(10,2),
                    TotalAmount DECIMAL(10,2),
                    Reason TEXT,
                    Type INTEGER DEFAULT 1,
                    Status INTEGER DEFAULT 1,
                    ProcessedBy INTEGER,
                    ReturnDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Notes TEXT,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id),
                    FOREIGN KEY (ProcessedBy) REFERENCES Users(Id)
                )",

                // جدول تقارير النظام
                @"CREATE TABLE IF NOT EXISTS Reports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReportName TEXT NOT NULL,
                    ReportType TEXT NOT NULL,
                    GeneratedBy INTEGER,
                    GeneratedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Parameters TEXT,
                    FilePath TEXT,
                    FOREIGN KEY (GeneratedBy) REFERENCES Users(Id)
                )",

                // جدول سجل العمليات
                @"CREATE TABLE IF NOT EXISTS ActivityLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    Action TEXT NOT NULL,
                    TableName TEXT,
                    RecordId INTEGER,
                    OldValues TEXT,
                    NewValues TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )",

                // جدول الإعدادات المحدث
                @"CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY,
                    StoreName TEXT,
                    StoreAddress TEXT,
                    StorePhone TEXT,
                    StoreEmail TEXT,
                    WelcomeMessage TEXT,
                    LowStockAlert INTEGER DEFAULT 5,
                    TaxRate DECIMAL(5,2) DEFAULT 0,
                    Currency TEXT DEFAULT 'EGP',
                    PrinterName TEXT,
                    AutoPrint BOOLEAN DEFAULT 0,
                    BackupInterval INTEGER DEFAULT 7
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
                "INSERT OR IGNORE INTO Settings (Id, StoreName, StoreAddress, StorePhone, StoreEmail, WelcomeMessage, LowStockAlert, TaxRate, Currency, AutoPrint, BackupInterval) " +
                "VALUES (1, 'متجر الملابس', '', '', '', 'شكراً لزيارتكم', 5, 0, 'EGP', 0, 7)", connection))
            {
                command.ExecuteNonQuery();
            }

            // إنشاء مستخدم المدير الافتراضي (admin123)
            using (var command = new SQLiteCommand(
                "INSERT OR IGNORE INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedBy) " +
                "VALUES ('admin', 'W6ph5Mm5Pz8GgiULbPgzG37mj9g=', 'مدير النظام', 1, 1, 'System')", connection))
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