using System;

namespace ClothingStoreManager.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public int? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrentQuantity { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }

        // خصائص إضافية للعرض
        public string SupplierName { get; set; }
        public decimal Profit => SalePrice - PurchasePrice;
        public bool IsLowStock { get; set; }
    }
}