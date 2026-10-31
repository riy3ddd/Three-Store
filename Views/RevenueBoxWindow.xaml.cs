using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class RevenueBoxWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public RevenueBoxWindow()
        {
            InitializeComponent();

            // آخر 7 أيام
            PeriodComboBox.SelectedIndex = 1;

            // تحميل الإيرادات بعد اكتمال النافذة
            Loaded += RevenueBoxWindow_Loaded;
        }

        // =========================================================
        // تحميل النافذة
        // =========================================================

        private void RevenueBoxWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadRevenue();
        }

        // =========================================================
        // تحميل ملخص صندوق الإيرادات
        // =========================================================

        private void LoadRevenue()
        {
            try
            {
                GetSelectedPeriod(
                    out DateTime? startDate,
                    out DateTime? endDate);

                // =================================================
                // إيرادات المبيعات
                // =================================================

                decimal salesRevenue =
                    DatabaseHelper.GetSalesRevenue(
                        startDate,
                        endDate);

                // =================================================
                // إجمالي المسترجعات
                // =================================================

                decimal returnsRevenue =
                    GetTotalReturns(
                        startDate,
                        endDate);

                // =================================================
                // إيرادات الصيانة
                // =================================================

                decimal maintenanceRevenue =
                    DatabaseHelper.GetMaintenanceRevenue(
                        startDate,
                        endDate);

                // =================================================
                // المصروفات
                // =================================================

                decimal expenses =
                    DatabaseHelper.GetExpensesRevenue(
                        startDate,
                        endDate);

                // =================================================
                // إيرادات الأجهزة
                // =================================================

                decimal deviceRevenue =
                    GetDeviceRevenue(
                        startDate,
                        endDate);

                // =================================================
                // صافي الإيرادات الكلي
                // =================================================

                decimal netRevenue =
                    salesRevenue
                    - returnsRevenue
                    + maintenanceRevenue
                    + deviceRevenue
                    - expenses;

                // =================================================
                // عرض إيرادات المبيعات
                // =================================================

                SalesRevenueSummaryTextBlock.Text =
                    $"{salesRevenue:N2} ج.م";

                // =================================================
                // عرض المسترجعات
                // =================================================

                ReturnsSummaryTextBlock.Text =
                    $"{returnsRevenue:N2} ج.م";

                // =================================================
                // عرض إيرادات الصيانة
                // =================================================

                MaintenanceRevenueSummaryTextBlock.Text =
                    $"{maintenanceRevenue:N2} ج.م";

                // =================================================
                // عرض المصروفات
                // =================================================

                ExpensesSummaryTextBlock.Text =
                    $"{expenses:N2} ج.م";

                // =================================================
                // عرض إيرادات الأجهزة
                // =================================================

                DeviceRevenueSummaryTextBlock.Text =
                    $"{deviceRevenue:N2} ج.م";

                // =================================================
                // عرض صافي الإيرادات الكلي
                // =================================================

                NetRevenueTextBlock.Text =
                    $"{netRevenue:N2} ج.م";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل صندوق الإيرادات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // إجمالي المسترجعات
        // =========================================================

        private decimal GetTotalReturns(
            DateTime? startDate,
            DateTime? endDate)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM Returns
                WHERE
                    (@StartDate IS NULL
                     OR ReturnDate >= @StartDate)
                AND
                    (@EndDate IS NULL
                     OR ReturnDate < @EndDate);
            ";

            using SqlCommand command =
                new SqlCommand(
                    query,
                    connection);

            command.Parameters.Add(
                "@StartDate",
                SqlDbType.DateTime).Value =
                startDate.HasValue
                    ? startDate.Value
                    : DBNull.Value;

            command.Parameters.Add(
                "@EndDate",
                SqlDbType.DateTime).Value =
                endDate.HasValue
                    ? endDate.Value
                    : DBNull.Value;

            object result =
                command.ExecuteScalar();

            if (result == null ||
                result == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(result);
        }

        // =========================================================
        // إيرادات الأجهزة
        // =========================================================
        private decimal GetDeviceRevenue(
    DateTime? startDate,
    DateTime? endDate)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
        SELECT
            ISNULL(
                (
                    SELECT SUM(ds.Total)
                    FROM DeviceSales ds
                    WHERE
                        (
                            @IsAll = 1
                            OR
                            (
                                ds.SaleDate >= @StartDate
                                AND ds.SaleDate < @EndDate
                            )
                        )
                ),
                0
            )
            +
            ISNULL(
                (
                    SELECT SUM(ct.Amount)
                    FROM CreditTransactions ct
                    WHERE
                        ct.TransactionType = N'Payment'
                        AND ct.DeviceSaleID IS NOT NULL
                        AND
                        (
                            @IsAll = 1
                            OR
                            (
                                ct.TransactionDate >= @StartDate
                                AND ct.TransactionDate < @EndDate
                            )
                        )
                ),
                0
            );
    ";

            using SqlCommand command =
                new SqlCommand(
                    query,
                    connection);

            bool isAll =
                !startDate.HasValue ||
                !endDate.HasValue;

            command.Parameters.Add(
                "@IsAll",
                SqlDbType.Bit).Value =
                isAll;

            command.Parameters.Add(
                "@StartDate",
                SqlDbType.DateTime).Value =
                (object?)startDate ??
                DBNull.Value;

            command.Parameters.Add(
                "@EndDate",
                SqlDbType.DateTime).Value =
                (object?)endDate ??
                DBNull.Value;

            object result =
                command.ExecuteScalar();

            if (result == null ||
                result == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(result);
        }               
        // =========================================================
        // تحديد الفترة
        // =========================================================

        private void GetSelectedPeriod(
            out DateTime? startDate,
            out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            DateTime today =
                DateTime.Today;

            int index =
                PeriodComboBox.SelectedIndex;

            switch (index)
            {
                case 0:

                    // اليوم

                    startDate =
                        today;

                    endDate =
                        today.AddDays(1);

                    break;

                case 1:

                    // آخر 7 أيام

                    startDate =
                        today.AddDays(-6);

                    endDate =
                        today.AddDays(1);

                    break;

                case 2:

                    // آخر 30 يوم

                    startDate =
                        today.AddDays(-29);

                    endDate =
                        today.AddDays(1);

                    break;

                case 3:

                    // آخر شهر

                    startDate =
                        today.AddMonths(-1);

                    endDate =
                        today.AddDays(1);

                    break;

                case 4:

                    // آخر شهرين

                    startDate =
                        today.AddMonths(-2);

                    endDate =
                        today.AddDays(1);

                    break;

                case 5:

                    // آخر 3 شهور

                    startDate =
                        today.AddMonths(-3);

                    endDate =
                        today.AddDays(1);

                    break;

                case 6:

                    // آخر 6 شهور

                    startDate =
                        today.AddMonths(-6);

                    endDate =
                        today.AddDays(1);

                    break;

                case 7:

                    // آخر سنة

                    startDate =
                        today.AddYears(-1);

                    endDate =
                        today.AddDays(1);

                    break;

                case 8:

                    // كل الفترات

                    startDate = null;
                    endDate = null;

                    break;

                default:

                    // آخر 7 أيام

                    startDate =
                        today.AddDays(-6);

                    endDate =
                        today.AddDays(1);

                    break;
            }
        }

        // =========================================================
        // تغيير الفترة
        // =========================================================

        private void PeriodComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            LoadRevenue();
        }
    }
}