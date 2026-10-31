using System;
using Microsoft.Data.SqlClient;

namespace WpfApp1
{
    public class FinancialSummary
    {
        public decimal SalesRevenue { get; set; }
        public decimal MaintenanceRevenue { get; set; }
        public decimal Expenses { get; set; }

        public decimal GrossRevenue =>
            SalesRevenue + MaintenanceRevenue;

        public decimal NetRevenue =>
            GrossRevenue - Expenses;
    }


    public static class FinancialCalculator
    {
        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";


        // =========================================================
        // حساب الملخص المالي لفترة
        // =========================================================

        public static FinancialSummary GetSummary(
            DateTime startDate,
            DateTime endDate)
        {
            FinancialSummary summary =
                new FinancialSummary();

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();


            // =====================================================
            // مبيعات المنتجات
            // =====================================================

            string salesQuery = @"
                SELECT
                    ISNULL(SUM(Total), 0)
                FROM Sales
                WHERE SaleDate >= @StartDate
                  AND SaleDate < @EndDate;";


            using (SqlCommand command =
                new SqlCommand(salesQuery, connection))
            {
                command.Parameters.Add(
                    "@StartDate",
                    System.Data.SqlDbType.DateTime).Value =
                    startDate;

                command.Parameters.Add(
                    "@EndDate",
                    System.Data.SqlDbType.DateTime).Value =
                    endDate;

                summary.SalesRevenue =
                    Convert.ToDecimal(
                        command.ExecuteScalar());
            }


            // =====================================================
            // إيرادات الصيانة
            // =====================================================

            string maintenanceQuery = @"
                SELECT
                    ISNULL(SUM(Price), 0)
                FROM MaintenanceInvoices
                WHERE ReceiveDate >= @StartDate
                  AND ReceiveDate < @EndDate
                  AND ISNULL(Status, '') <> 'ملغاة';";


            using (SqlCommand command =
                new SqlCommand(
                    maintenanceQuery,
                    connection))
            {
                command.Parameters.Add(
                    "@StartDate",
                    System.Data.SqlDbType.DateTime).Value =
                    startDate;

                command.Parameters.Add(
                    "@EndDate",
                    System.Data.SqlDbType.DateTime).Value =
                    endDate;

                summary.MaintenanceRevenue =
                    Convert.ToDecimal(
                        command.ExecuteScalar());
            }


            // =====================================================
            // المصروفات العامة
            // =====================================================

            string expensesQuery = @"
                SELECT
                    ISNULL(SUM(Amount), 0)
                FROM Expenses
                WHERE ExpenseDate >= @StartDate
                  AND ExpenseDate < @EndDate
                  AND ISNULL(IsDeleted, 0) = 0;";


            using (SqlCommand command =
                new SqlCommand(
                    expensesQuery,
                    connection))
            {
                command.Parameters.Add(
                    "@StartDate",
                    System.Data.SqlDbType.DateTime).Value =
                    startDate;

                command.Parameters.Add(
                    "@EndDate",
                    System.Data.SqlDbType.DateTime).Value =
                    endDate;

                summary.Expenses =
                    Convert.ToDecimal(
                        command.ExecuteScalar());
            }


            return summary;
        }


        // =========================================================
        // إجمالي المصروفات فقط
        // =========================================================

        public static decimal GetExpenses(
            DateTime startDate,
            DateTime endDate)
        {
            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            string query = @"
                SELECT
                    ISNULL(SUM(Amount), 0)
                FROM Expenses
                WHERE ExpenseDate >= @StartDate
                  AND ExpenseDate < @EndDate
                  AND ISNULL(IsDeleted, 0) = 0;";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@StartDate",
                System.Data.SqlDbType.DateTime).Value =
                startDate;

            command.Parameters.Add(
                "@EndDate",
                System.Data.SqlDbType.DateTime).Value =
                endDate;

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }
    }
}