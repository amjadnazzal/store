using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using ClothingStoreManager.Models;

namespace ClothingStoreManager.Services
{
    public class UserService
    {
        public static User CurrentUser { get; private set; }

        public User AuthenticateUser(string username, string password)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Users 
                    WHERE Username = @username AND IsActive = 1";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = MapReaderToUser(reader);
                            if (VerifyPassword(password, user.PasswordHash))
                            {
                                // تحديث تاريخ آخر تسجيل دخول
                                UpdateLastLogin(user.Id);
                                CurrentUser = user;
                                
                                // تسجيل العملية في سجل النشاط
                                LogActivity(user.Id, "تسجيل دخول", "Users", user.Id);
                                
                                return user;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = GetUserById(userId);
            if (user == null || !VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            var newPasswordHash = HashPassword(newPassword);
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Users SET PasswordHash = @passwordHash WHERE Id = @id";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@passwordHash", newPasswordHash);
                    command.Parameters.AddWithValue("@id", userId);
                    
                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        LogActivity(CurrentUser?.Id ?? userId, "تغيير كلمة المرور", "Users", userId);
                    }
                    return success;
                }
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Users ORDER BY CreatedDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(MapReaderToUser(reader));
                    }
                }
            }

            return users;
        }

        public User GetUserById(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Users WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        public int AddUser(User user, string password)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedBy)
                    VALUES (@username, @passwordHash, @fullName, @email, @phone, @role, @isActive, @createdBy);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@passwordHash", HashPassword(password));
                    command.Parameters.AddWithValue("@fullName", user.FullName);
                    command.Parameters.AddWithValue("@email", user.Email ?? "");
                    command.Parameters.AddWithValue("@phone", user.Phone ?? "");
                    command.Parameters.AddWithValue("@role", (int)user.Role);
                    command.Parameters.AddWithValue("@isActive", user.IsActive);
                    command.Parameters.AddWithValue("@createdBy", CurrentUser?.Username ?? "System");
                    
                    int newUserId = Convert.ToInt32(command.ExecuteScalar());
                    LogActivity(CurrentUser?.Id, "إضافة مستخدم جديد", "Users", newUserId);
                    return newUserId;
                }
            }
        }

        public bool UpdateUser(User user)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Users 
                    SET Username = @username, FullName = @fullName, Email = @email, 
                        Phone = @phone, Role = @role, IsActive = @isActive
                    WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@fullName", user.FullName);
                    command.Parameters.AddWithValue("@email", user.Email ?? "");
                    command.Parameters.AddWithValue("@phone", user.Phone ?? "");
                    command.Parameters.AddWithValue("@role", (int)user.Role);
                    command.Parameters.AddWithValue("@isActive", user.IsActive);
                    command.Parameters.AddWithValue("@id", user.Id);
                    
                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        LogActivity(CurrentUser?.Id, "تعديل بيانات مستخدم", "Users", user.Id);
                    }
                    return success;
                }
            }
        }

        public bool DeactivateUser(int userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Users SET IsActive = 0 WHERE Id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        LogActivity(CurrentUser?.Id, "إلغاء تفعيل مستخدم", "Users", userId);
                    }
                    return success;
                }
            }
        }

        public bool HasPermission(string permission)
        {
            if (CurrentUser == null) return false;
            
            var permissions = UserPermissions.GetPermissions(CurrentUser.Role);
            
            return permission switch
            {
                "ManageProducts" => permissions.CanManageProducts,
                "ManageCustomers" => permissions.CanManageCustomers,
                "ManageSuppliers" => permissions.CanManageSuppliers,
                "MakeSales" => permissions.CanMakeSales,
                "ProcessReturns" => permissions.CanProcessReturns,
                "ViewReports" => permissions.CanViewReports,
                "ManageUsers" => permissions.CanManageUsers,
                "BackupRestore" => permissions.CanBackupRestore,
                "ChangeSettings" => permissions.CanChangeSettings,
                "ApplyDiscounts" => permissions.CanApplyDiscounts,
                _ => false
            };
        }

        public void LogActivity(int? userId, string action, string tableName, int? recordId = null, string oldValues = null, string newValues = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO ActivityLog (UserId, Action, TableName, RecordId, OldValues, NewValues)
                        VALUES (@userId, @action, @tableName, @recordId, @oldValues, @newValues)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", (object)userId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@tableName", tableName ?? "");
                        command.Parameters.AddWithValue("@recordId", (object)recordId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@oldValues", oldValues ?? "");
                        command.Parameters.AddWithValue("@newValues", newValues ?? "");
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // تجاهل أخطاء تسجيل النشاط لعدم تعطيل العمليات الأساسية
            }
        }

        public void Logout()
        {
            if (CurrentUser != null)
            {
                LogActivity(CurrentUser.Id, "تسجيل خروج", "Users", CurrentUser.Id);
                CurrentUser = null;
            }
        }

        private void UpdateLastLogin(int userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Users SET LastLogin = CURRENT_TIMESTAMP WHERE Id = @id";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ClothingStore2024"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }

        private User MapReaderToUser(SQLiteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                PasswordHash = reader.GetString("PasswordHash"),
                FullName = reader.GetString("FullName"),
                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                Role = (UserRole)reader.GetInt32("Role"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastLogin = reader.IsDBNull("LastLogin") ? null : reader.GetDateTime("LastLogin"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? "" : reader.GetString("CreatedBy")
            };
        }
    }
}