using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class ReturnDetailsWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int returnID;

        public ReturnDetailsWindow(int returnID)
        {
            InitializeComponent();

            this.returnID = returnID;

            LoadReturnDetails();
            LoadReturnItems();
        }

        // =========================================================
        // تحميل بيانات المسترجع + الفاتورة
        // =========================================================

        private void LoadReturnDetails()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    SELECT
                        r.ReturnID,
                        r.SaleID,
                        r.ReturnDate,
                        r.TotalAmount,
                        r.Notes,
                        r.CreatedBy,
                        r.CreatedAt,

                        s.SaleDate,
                        s.Subtotal,
                        s.Discount,
                        s.Total AS SaleTotal,
                        s.Paid,
                        s.Change,
                        s.PaymentMethod,
                        s.VodafoneCashNumber,
                        s.CustomerName,
                        s.CustomerPhone

                    FROM Returns r

                    LEFT JOIN Sales s
                        ON r.SaleID = s.SaleID

                    WHERE r.ReturnID = @ReturnID;
                ";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@ReturnID",
                    returnID);

                connection.Open();

                using SqlDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "لم يتم العثور على بيانات المسترجع رقم " +
                        returnID +
                        ".",
                        "المسترجع غير موجود",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }

                // =====================================================
                // Return ID
                // =====================================================

                int loadedReturnID =
                    reader["ReturnID"] == DBNull.Value
                        ? returnID
                        : Convert.ToInt32(reader["ReturnID"]);

                HeaderReturnIdTextBlock.Text =
                    loadedReturnID.ToString();

                ReturnIdTextBlock.Text =
                    loadedReturnID.ToString();

                // =====================================================
                // Sale ID
                // =====================================================

                if (reader["SaleID"] != DBNull.Value)
                {
                    SaleIdTextBlock.Text =
                        reader["SaleID"].ToString();
                }
                else
                {
                    SaleIdTextBlock.Text =
                        "غير محدد";
                }

                // =====================================================
                // Return Date
                // =====================================================

                if (reader["ReturnDate"] != DBNull.Value)
                {
                    DateTime returnDate =
                        Convert.ToDateTime(
                            reader["ReturnDate"]);

                    ReturnDateTextBlock.Text =
                        returnDate.ToString(
                            "dd/MM/yyyy - hh:mm tt");
                }
                else
                {
                    ReturnDateTextBlock.Text =
                        "غير محدد";
                }

                // =====================================================
                // Sale Date
                // =====================================================

                if (reader["SaleDate"] != DBNull.Value)
                {
                    DateTime saleDate =
                        Convert.ToDateTime(
                            reader["SaleDate"]);

                    SaleDateTextBlock.Text =
                        saleDate.ToString(
                            "dd/MM/yyyy - hh:mm tt");
                }
                else
                {
                    SaleDateTextBlock.Text =
                        "غير محدد";
                }

                // =====================================================
                // Customer Name
                // =====================================================

                string customerName =
                    reader["CustomerName"] == DBNull.Value
                        ? ""
                        : reader["CustomerName"].ToString();

                CustomerNameTextBlock.Text =
                    string.IsNullOrWhiteSpace(customerName)
                        ? "عميل نقدي"
                        : customerName;

                // =====================================================
                // Customer Phone
                // =====================================================

                string customerPhone =
                    reader["CustomerPhone"] == DBNull.Value
                        ? ""
                        : reader["CustomerPhone"].ToString();

                CustomerPhoneTextBlock.Text =
                    string.IsNullOrWhiteSpace(customerPhone)
                        ? "غير محدد"
                        : customerPhone;

                // =====================================================
                // Payment Method
                // =====================================================

                string paymentMethod =
                    reader["PaymentMethod"] == DBNull.Value
                        ? ""
                        : reader["PaymentMethod"].ToString();

                PaymentMethodTextBlock.Text =
                    string.IsNullOrWhiteSpace(paymentMethod)
                        ? "غير محدد"
                        : paymentMethod;

                // =====================================================
                // Original Sale Total
                // =====================================================

                decimal saleTotal =
                    reader["SaleTotal"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            reader["SaleTotal"]);

                SaleTotalTextBlock.Text =
                    $"{saleTotal:N2} ج";

                // =====================================================
                // Return Total
                // =====================================================

                decimal returnTotal =
                    reader["TotalAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            reader["TotalAmount"]);

                ReturnTotalTextBlock.Text =
                    $"{returnTotal:N2} ج";

                FooterTotalTextBlock.Text =
                    $"{returnTotal:N2} ج";

                // =====================================================
                // Notes
                // =====================================================

                string notes =
                    reader["Notes"] == DBNull.Value
                        ? ""
                        : reader["Notes"].ToString();

                NotesTextBlock.Text =
                    string.IsNullOrWhiteSpace(notes)
                        ? "لا توجد ملاحظات"
                        : notes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل تفاصيل المسترجع:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // تحميل المنتجات المرتجعة
        // =========================================================

        private void LoadReturnItems()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    SELECT
                        ri.ReturnItemID,
                        ri.ReturnID,
                        ri.ProductID,
                        p.ProductName,
                        ri.Quantity,
                        ri.UnitPrice,
                        ri.Total

                    FROM ReturnItems ri

                    LEFT JOIN Products p
                        ON ri.ProductID = p.ProductID

                    WHERE ri.ReturnID = @ReturnID

                    ORDER BY ri.ReturnItemID ASC;
                ";

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(
                        query,
                        connection);

                adapter.SelectCommand.Parameters.AddWithValue(
                    "@ReturnID",
                    returnID);

                DataTable table =
                    new DataTable();

                adapter.Fill(table);

                // =====================================================
                // رقم العرض
                // =====================================================

                if (!table.Columns.Contains("DisplayNumber"))
                {
                    table.Columns.Add(
                        "DisplayNumber",
                        typeof(int));
                }

                for (int i = 0;
                     i < table.Rows.Count;
                     i++)
                {
                    table.Rows[i]["DisplayNumber"] =
                        i + 1;
                }

                ReturnItemsDataGrid.ItemsSource =
                    table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المنتجات المرتجعة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // إغلاق
        // =========================================================

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}