using System;

namespace ClothingStoreManager.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public string CreatedBy { get; set; }
    }

    public enum UserRole
    {
        Admin = 1,      // مدير النظام - جميع الصلاحيات
        Manager = 2,    // مدير المتجر - معظم الصلاحيات
        Cashier = 3,    // كاشير - نقطة البيع والعرض فقط
        Employee = 4    // موظف - صلاحيات محدودة
    }

    public class UserPermissions
    {
        public bool CanManageProducts { get; set; }
        public bool CanManageCustomers { get; set; }
        public bool CanManageSuppliers { get; set; }
        public bool CanMakeSales { get; set; }
        public bool CanProcessReturns { get; set; }
        public bool CanViewReports { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanBackupRestore { get; set; }
        public bool CanChangeSettings { get; set; }
        public bool CanApplyDiscounts { get; set; }

        public static UserPermissions GetPermissions(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => new UserPermissions
                {
                    CanManageProducts = true,
                    CanManageCustomers = true,
                    CanManageSuppliers = true,
                    CanMakeSales = true,
                    CanProcessReturns = true,
                    CanViewReports = true,
                    CanManageUsers = true,
                    CanBackupRestore = true,
                    CanChangeSettings = true,
                    CanApplyDiscounts = true
                },
                UserRole.Manager => new UserPermissions
                {
                    CanManageProducts = true,
                    CanManageCustomers = true,
                    CanManageSuppliers = true,
                    CanMakeSales = true,
                    CanProcessReturns = true,
                    CanViewReports = true,
                    CanManageUsers = false,
                    CanBackupRestore = true,
                    CanChangeSettings = false,
                    CanApplyDiscounts = true
                },
                UserRole.Cashier => new UserPermissions
                {
                    CanManageProducts = false,
                    CanManageCustomers = true,
                    CanManageSuppliers = false,
                    CanMakeSales = true,
                    CanProcessReturns = true,
                    CanViewReports = false,
                    CanManageUsers = false,
                    CanBackupRestore = false,
                    CanChangeSettings = false,
                    CanApplyDiscounts = false
                },
                UserRole.Employee => new UserPermissions
                {
                    CanManageProducts = false,
                    CanManageCustomers = false,
                    CanManageSuppliers = false,
                    CanMakeSales = true,
                    CanProcessReturns = false,
                    CanViewReports = false,
                    CanManageUsers = false,
                    CanBackupRestore = false,
                    CanChangeSettings = false,
                    CanApplyDiscounts = false
                },
                _ => new UserPermissions()
            };
        }
    }
}