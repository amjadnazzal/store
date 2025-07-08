using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class ProductService
    {
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName 
                    FROM Products p 
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                    ORDER BY p.Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(MapReaderToProduct(reader));
                    }
                }
            }

            return products;
        }

        public Product GetProductByBarcode(string barcode)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName 
                    FROM Products p 
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                    WHERE p.Barcode = @barcode";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@barcode", barcode);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToProduct(reader);
                        }
                    }
                }
            }

            return null;
        }

        public Product GetProductById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName 
                    FROM Products p 
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                    WHERE p.Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToProduct(reader);
                        }
                    }
                }
            }

            return null;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            List<Product> products = new List<Product>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName 
                    FROM Products p 
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                    WHERE p.Barcode LIKE @search 
                       OR p.Name LIKE @search 
                       OR p.Description LIKE @search 
                       OR p.Color LIKE @search 
                       OR p.Size LIKE @search
                    ORDER BY p.Name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapReaderToProduct(reader));
                        }
                    }
                }
            }

            return products;
        }

        public int AddProduct(Product product)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Products (Barcode, Name, Description, Size, Color, SupplierId, 
                                        PurchasePrice, SalePrice, CurrentQuantity, ImagePath)
                    VALUES (@barcode, @name, @description, @size, @color, @supplierId, 
                           @purchasePrice, @salePrice, @currentQuantity, @imagePath);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddProductParameters(command, product);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateProduct(Product product)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Products 
                    SET Barcode = @barcode, Name = @name, Description = @description, 
                        Size = @size, Color = @color, SupplierId = @supplierId,
                        PurchasePrice = @purchasePrice, SalePrice = @salePrice, 
                        CurrentQuantity = @currentQuantity, ImagePath = @imagePath
                    WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddProductParameters(command, product);
                    command.Parameters.AddWithValue("@id", product.Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteProduct(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Products WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateQuantity(int productId, int newQuantity)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Products SET CurrentQuantity = @quantity WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@quantity", newQuantity);
                    command.Parameters.AddWithValue("@id", productId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Product> GetLowStockProducts(int threshold = 5)
        {
            List<Product> products = new List<Product>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT p.*, s.Name as SupplierName 
                    FROM Products p 
                    LEFT JOIN Suppliers s ON p.SupplierId = s.Id 
                    WHERE p.CurrentQuantity <= @threshold
                    ORDER BY p.CurrentQuantity ASC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@threshold", threshold);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapReaderToProduct(reader);
                            product.IsLowStock = true;
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        private void AddProductParameters(SQLiteCommand command, Product product)
        {
            command.Parameters.AddWithValue("@barcode", product.Barcode);
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@description", product.Description ?? "");
            command.Parameters.AddWithValue("@size", product.Size ?? "");
            command.Parameters.AddWithValue("@color", product.Color ?? "");
            command.Parameters.AddWithValue("@supplierId", (object)product.SupplierId ?? DBNull.Value);
            command.Parameters.AddWithValue("@purchasePrice", product.PurchasePrice);
            command.Parameters.AddWithValue("@salePrice", product.SalePrice);
            command.Parameters.AddWithValue("@currentQuantity", product.CurrentQuantity);
            command.Parameters.AddWithValue("@imagePath", product.ImagePath ?? "");
        }

        private Product MapReaderToProduct(SQLiteDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32("Id"),
                Barcode = reader.GetString("Barcode"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                Size = reader.IsDBNull("Size") ? "" : reader.GetString("Size"),
                Color = reader.IsDBNull("Color") ? "" : reader.GetString("Color"),
                SupplierId = reader.IsDBNull("SupplierId") ? (int?)null : reader.GetInt32("SupplierId"),
                PurchasePrice = reader.GetDecimal("PurchasePrice"),
                SalePrice = reader.GetDecimal("SalePrice"),
                CurrentQuantity = reader.GetInt32("CurrentQuantity"),
                ImagePath = reader.IsDBNull("ImagePath") ? "" : reader.GetString("ImagePath"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                SupplierName = reader.IsDBNull("SupplierName") ? "" : reader.GetString("SupplierName")
            };
        }
    }
}