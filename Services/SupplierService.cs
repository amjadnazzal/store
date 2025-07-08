using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class SupplierService
    {
        public List<Supplier> GetAllSuppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Suppliers ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(MapReaderToSupplier(reader));
                    }
                }
            }

            return suppliers;
        }

        public Supplier GetSupplierById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Suppliers WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToSupplier(reader);
                        }
                    }
                }
            }

            return null;
        }

        public List<Supplier> SearchSuppliers(string searchTerm)
        {
            List<Supplier> suppliers = new List<Supplier>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Suppliers 
                    WHERE Name LIKE @search 
                       OR Phone LIKE @search 
                       OR Email LIKE @search
                    ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(MapReaderToSupplier(reader));
                        }
                    }
                }
            }

            return suppliers;
        }

        public int AddSupplier(Supplier supplier)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Suppliers (Name, Phone, Email, Address)
                    VALUES (@name, @phone, @email, @address);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddSupplierParameters(command, supplier);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateSupplier(Supplier supplier)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Suppliers 
                    SET Name = @name, Phone = @phone, Email = @email, Address = @address
                    WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddSupplierParameters(command, supplier);
                    command.Parameters.AddWithValue("@id", supplier.Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteSupplier(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // التحقق من وجود منتجات مرتبطة بالمورد
                string checkQuery = "SELECT COUNT(*) FROM Products WHERE SupplierId = @id";
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@id", id);
                    int productCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    
                    if (productCount > 0)
                    {
                        throw new InvalidOperationException("لا يمكن حذف المورد لأن هناك منتجات مرتبطة به");
                    }
                }

                string query = "DELETE FROM Suppliers WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private void AddSupplierParameters(SQLiteCommand command, Supplier supplier)
        {
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@phone", supplier.Phone ?? "");
            command.Parameters.AddWithValue("@email", supplier.Email ?? "");
            command.Parameters.AddWithValue("@address", supplier.Address ?? "");
        }

        private Supplier MapReaderToSupplier(SQLiteDataReader reader)
        {
            return new Supplier
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}