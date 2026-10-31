using System;
using System.Data;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly DataRowView orderRow;

        public OrderDetailsWindow(DataRowView row)
        {
            InitializeComponent();

            orderRow = row;

            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                // رقم الطلبية
                OrderNumberText.Text =
                    "رقم الطلبية: " +
                    GetValue("OrderID", "غير محدد");

                // اسم العميل
                CustomerNameText.Text =
                    GetValue("CustomerName", "غير محدد");

                // رقم الهاتف
                CustomerPhoneText.Text =
                    GetValue("CustomerPhone", "غير مسجل");

                // اسم المنتج
                ProductNameText.Text =
                    GetValue("ProductName", "غير محدد");

                // هل المنتج يدوي؟
                string manualValue =
                    GetValue("IsManualProduct", "False");

                bool isManual =
                    manualValue.Equals(
                        "True",
                        StringComparison.OrdinalIgnoreCase)
                    || manualValue == "1";

                ManualProductText.Visibility =
                    isManual
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // الكمية
                QuantityText.Text =
                    GetValue("Quantity", "0");

                // التاريخ
                object dateValue =
                    orderRow["OrderDate"];

                if (dateValue != null &&
                    dateValue != DBNull.Value)
                {
                    DateTime date =
                        Convert.ToDateTime(dateValue);

                    OrderDateText.Text =
                        date.ToString("yyyy/MM/dd - HH:mm");
                }
                else
                {
                    OrderDateText.Text =
                        "غير محدد";
                }

                // الملاحظات
                string note =
                    GetValue("Note", "");

                NoteText.Text =
                    string.IsNullOrWhiteSpace(note)
                        ? "لا توجد ملاحظات"
                        : note;

                // الحالة
                StatusText.Text =
                    GetValue("Status", "غير محددة");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء عرض تفاصيل الطلبية:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =====================================================
        // قراءة قيمة من DataRowView
        // =====================================================

        private string GetValue(
            string columnName,
            string defaultValue)
        {
            try
            {
                object value =
                    orderRow[columnName];

                if (value == null ||
                    value == DBNull.Value)
                {
                    return defaultValue;
                }

                string result =
                    value.ToString() ?? "";

                return string.IsNullOrWhiteSpace(result)
                    ? defaultValue
                    : result;
            }
            catch
            {
                return defaultValue;
            }
        }

        // =====================================================
        // إغلاق
        // =====================================================

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}