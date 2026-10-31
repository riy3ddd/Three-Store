namespace WpfApp1.Models
{
    public class CartItem
    {
        // رقم المنتج الحقيقي في قاعدة البيانات
        public int ProductID { get; set; }

        // رقم المنتج داخل الفاتورة فقط
        public int CartNumber { get; set; }

        // اسم المنتج
        public string ProductName { get; set; } = "";

        // الكمية المطلوبة للبيع
        public int Quantity { get; set; }

        // الكمية المتاحة في المخزون
        public int AvailableQuantity { get; set; }

        // سعر البيع
        public decimal SellingPrice { get; set; }

        // إجمالي هذا المنتج
        public decimal Total
        {
            get
            {
                return Quantity * SellingPrice;
            }
        }
    }
}