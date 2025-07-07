using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class CustomerService
    {
        public List<Customer> GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(MapReaderToCustomer(reader));
                    }
                }
            }

            return customers;
        }

        public Customer GetCustomerById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToCustomer(reader);
                        }
                    }
                }
            }

            return null;
        }

        public Customer GetCustomerByPhone(string phone)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers WHERE Phone = @phone";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@phone", phone);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToCustomer(reader);
                        }
                    }
                }
            }

            return null;
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            List<Customer> customers = new List<Customer>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Customers 
                    WHERE Name LIKE @search 
                       OR Phone LIKE @search 
                       OR Address LIKE @search
                    ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(MapReaderToCustomer(reader));
                        }
                    }
                }
            }

            return customers;
        }

        public int AddCustomer(Customer customer)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Customers (Name, Phone, Address)
                    VALUES (@name, @phone, @address);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddCustomerParameters(command, customer);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Customers 
                    SET Name = @name, Phone = @phone, Address = @address
                    WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    AddCustomerParameters(command, customer);
                    command.Parameters.AddWithValue("@id", customer.Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCustomer(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // التحقق من وجود فواتير مرتبطة بالعميل
                string checkQuery = "SELECT COUNT(*) FROM Invoices WHERE CustomerId = @id";
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@id", id);
                    int invoiceCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    
                    if (invoiceCount > 0)
                    {
                        throw new InvalidOperationException("لا يمكن حذف العميل لأن له فواتير مسجلة");
                    }
                }

                string query = "DELETE FROM Customers WHERE Id = @id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private void AddCustomerParameters(SQLiteCommand command, Customer customer)
        {
            command.Parameters.AddWithValue("@name", customer.Name);
            command.Parameters.AddWithValue("@phone", customer.Phone ?? "");
            command.Parameters.AddWithValue("@address", customer.Address ?? "");
        }

        private Customer MapReaderToCustomer(SQLiteDataReader reader)
        {
            return new Customer
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}