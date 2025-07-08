using System;

namespace ClothingStoreManager.Models
{
    public class Return
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; }
        public ReturnType Type { get; set; }
        public ReturnStatus Status { get; set; }
        public int ProcessedBy { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Notes { get; set; }

        // خصائص إضافية للعرض
        public string InvoiceNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductBarcode { get; set; }
        public string ProcessedByName { get; set; }
    }

    public enum ReturnType
    {
        Refund = 1,     // استرداد نقدي
        Exchange = 2,   // استبدال منتج
        Credit = 3      // رصيد للعميل
    }

    public enum ReturnStatus
    {
        Pending = 1,    // في الانتظار
        Approved = 2,   // معتمد
        Rejected = 3,   // مرفوض
        Completed = 4   // مكتمل
    }
}