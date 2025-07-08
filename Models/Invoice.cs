using System;
using System.Collections.Generic;

namespace ClothingStoreManager.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int? CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime InvoiceDate { get; set; }

        // خصائص إضافية للعرض
        public string CustomerName { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // خصائص إضافية للعرض
        public string ProductName { get; set; }
        public string ProductBarcode { get; set; }
    }
}