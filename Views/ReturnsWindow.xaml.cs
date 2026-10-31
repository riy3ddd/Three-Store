using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Views
{
    public partial class ReturnsWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private DataTable returnsTable;

        public ReturnsWindow()
        {
            InitializeComponent();

            LoadReturns();
            LoadStatistics();
        }

        // =========================================================
        // تحميل المسترجعات
        // =========================================================

        private void LoadReturns()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    SELECT
                        ReturnID,
                        SaleID,
                        ReturnDate,
                        TotalAmount,
                        Notes
                    FROM Returns
                    ORDER BY ReturnID DESC;
                ";

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection);

                returnsTable = new DataTable();

                adapter.Fill(returnsTable);

                AddDisplayNumbers();

                ReturnsDataGrid.ItemsSource =
                    returnsTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المسترجعات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // رقم العرض
        // =========================================================

        private void AddDisplayNumbers()
        {
            if (returnsTable == null)
                return;

            if (!returnsTable.Columns.Contains("DisplayNumber"))
            {
                returnsTable.Columns.Add(
                    "DisplayNumber",
                    typeof(int));
            }

            for (int i = 0; i < returnsTable.Rows.Count; i++)
            {
                returnsTable.Rows[i]["DisplayNumber"] = i + 1;
            }
        }

        // =========================================================
        // الإحصائيات
        // =========================================================

        private void LoadStatistics()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                connection.Open();

                // =====================================================
                // إجمالي المسترجعات
                // =====================================================

                string returnsQuery = @"
                    SELECT
                        ISNULL(SUM(TotalAmount), 0),
                        COUNT(*)
                    FROM Returns;
                ";

                using SqlCommand returnsCommand =
                    new SqlCommand(
                        returnsQuery,
                        connection);

                using SqlDataReader reader =
                    returnsCommand.ExecuteReader();

                decimal totalReturns = 0;
                int returnsCount = 0;

                if (reader.Read())
                {
                    totalReturns =
                        reader.IsDBNull(0)
                            ? 0
                            : Convert.ToDecimal(reader[0]);

                    returnsCount =
                        reader.IsDBNull(1)
                            ? 0
                            : Convert.ToInt32(reader[1]);
                }

                reader.Close();

                // =====================================================
                // إجمالي المبيعات
                // =====================================================

                string salesQuery = @"
                    SELECT ISNULL(SUM(Total), 0)
                    FROM Sales;
                ";

                using SqlCommand salesCommand =
                    new SqlCommand(
                        salesQuery,
                        connection);

                decimal totalSales =
                    Convert.ToDecimal(
                        salesCommand.ExecuteScalar());

                // =====================================================
                // صافي المبيعات بعد المسترجعات
                // =====================================================

                decimal netSales =
                    totalSales - totalReturns;

                // =====================================================
                // عرض الإحصائيات
                // =====================================================

                TotalReturnsTextBlock.Text =
                    $"{totalReturns:N2} ج";

                ReturnsCountTextBlock.Text =
                    returnsCount.ToString();

                NetSalesTextBlock.Text =
                    $"{netSales:N2} ج";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الإحصائيات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // إضافة مسترجع
        // =========================================================

        private void AddReturnButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddReturnWindow window =
                new AddReturnWindow
                {
                    Owner = this
                };

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadReturns();
                LoadStatistics();
            }
        }

        // =========================================================
        // تحديث
        // =========================================================

        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadReturns();
            LoadStatistics();
        }

        // =========================================================
        // البحث
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (returnsTable == null)
                return;

            string search =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(search))
            {
                ReturnsDataGrid.ItemsSource =
                    returnsTable.DefaultView;

                return;
            }

            DataView view =
                returnsTable.DefaultView;

            if (int.TryParse(search, out int number))
            {
                view.RowFilter =
                    $"ReturnID = {number} OR SaleID = {number}";
            }
            else
            {
                view.RowFilter =
                    $"CONVERT(ReturnDate, 'System.String') LIKE '%{search}%'";
            }

            ReturnsDataGrid.ItemsSource = view;
        }

        // =========================================================
        // فتح تفاصيل المسترجع
        // =========================================================

        private void ReturnsDataGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (ReturnsDataGrid.SelectedItem == null)
                return;

            DataRowView row =
                ReturnsDataGrid.SelectedItem as DataRowView;

            if (row == null)
                return;

            int returnID =
                Convert.ToInt32(row["ReturnID"]);

            // =====================================================
            // فتح شاشة التفاصيل الحقيقية
            // =====================================================

            ReturnDetailsWindow window =
                new ReturnDetailsWindow(returnID)
                {
                    Owner = this,
                    WindowStartupLocation =
                        WindowStartupLocation.CenterOwner
                };

            window.ShowDialog();
        }
    }
}