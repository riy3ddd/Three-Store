using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class ExpensesWindow : Window
    {
        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private DataTable allExpenses = new DataTable();

        public ExpensesWindow()
        {
            InitializeComponent();

            Loaded += ExpensesWindow_Loaded;
        }

        private void ExpensesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadExpenses();
            LoadStatistics();
        }


        // =========================================================
        // تحميل المصروفات
        // =========================================================

        private void LoadExpenses()
        {
            try
            {
                DateTime startDate = GetStartDate();
                DateTime endDate = DateTime.Now;

                using SqlConnection connection = new SqlConnection(ConnectionString);

                string query = @"
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY E.ExpenseDate DESC, E.ExpenseID DESC) AS RowNumber,
                        E.ExpenseID,
                        E.ExpenseNumber,
                        E.ExpenseType,
                        E.Description,
                        E.Amount,
                        E.ExpenseDate,
                        E.Notes,
                        E.CreatedBy,
                        E.CreatedAt,
                        E.ExpenseCategoryID,
                        EC.CategoryName,
                        U.Username
                    FROM Expenses E
                    LEFT JOIN ExpenseCategories EC
                        ON E.ExpenseCategoryID = EC.ExpenseCategoryID
                    LEFT JOIN Users U
                        ON E.CreatedBy = U.UserID
                    WHERE
                        E.ExpenseDate >= @StartDate
                        AND E.ExpenseDate <= @EndDate
                    ORDER BY
                        E.ExpenseDate DESC,
                        E.ExpenseID DESC;";

                using SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                allExpenses.Clear();
                adapter.Fill(allExpenses);

                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المصروفات:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // الإحصائيات
        // =========================================================

        private void LoadStatistics()
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    SELECT

                        ISNULL(
                            SUM(
                                CASE
                                    WHEN CAST(ExpenseDate AS DATE) = CAST(GETDATE() AS DATE)
                                    THEN Amount
                                    ELSE 0
                                END
                            ), 0
                        ) AS TodayTotal,

                        ISNULL(
                            SUM(
                                CASE
                                    WHEN ExpenseDate >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
                                    THEN Amount
                                    ELSE 0
                                END
                            ), 0
                        ) AS Last7DaysTotal,

                        ISNULL(
                            SUM(
                                CASE
                                    WHEN YEAR(ExpenseDate) = YEAR(GETDATE())
                                     AND MONTH(ExpenseDate) = MONTH(GETDATE())
                                    THEN Amount
                                    ELSE 0
                                END
                            ), 0
                        ) AS MonthTotal,

                        COUNT(*) AS ExpensesCount

                    FROM Expenses
                    WHERE ISNULL(IsDeleted, 0) = 0;";

                using SqlCommand command = new SqlCommand(query, connection);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    decimal today =
                        reader["TodayTotal"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(reader["TodayTotal"]);

                    decimal last7 =
                        reader["Last7DaysTotal"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(reader["Last7DaysTotal"]);

                    decimal month =
                        reader["MonthTotal"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(reader["MonthTotal"]);

                    int count =
                        reader["ExpensesCount"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["ExpensesCount"]);

                    TodayExpensesText.Text =
                        $"{today:N2} ج.م";

                    Last7DaysExpensesText.Text =
                        $"{last7:N2} ج.م";

                    ThisMonthExpensesText.Text =
                        $"{month:N2} ج.م";

                    ExpensesCountText.Text =
                        count.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل إحصائيات المصروفات:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تحديد بداية الفترة
        // =========================================================

        private DateTime GetStartDate()
        {
            DateTime now = DateTime.Now;

            int index = PeriodComboBox.SelectedIndex;

            switch (index)
            {
                case 0:
                    return now.Date;

                case 1:
                    return now.Date.AddDays(-6);

                case 2:
                    return now.Date.AddDays(-29);

                case 3:
                    return now.Date.AddMonths(-2);

                case 4:
                    return now.Date.AddMonths(-3);

                case 5:
                    return now.Date.AddMonths(-6);

                case 6:
                    return now.Date.AddMonths(-12);

                case 7:
                    return now.Date.AddMonths(-24);

                default:
                    return now.Date.AddDays(-29);
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

            LoadExpenses();
        }


        // =========================================================
        // البحث
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            ApplySearch();
        }


        private void ApplySearch()
        {
            try
            {
                if (allExpenses == null)
                    return;

                string searchText =
                    SearchTextBox?.Text?.Trim() ?? "";

                DataView view = allExpenses.DefaultView;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    view.RowFilter = "";
                }
                else
                {
                    string escaped =
                        searchText.Replace("'", "''");

                    view.RowFilter =
                        $"ExpenseNumber LIKE '%{escaped}%' " +
                        $"OR Description LIKE '%{escaped}%' " +
                        $"OR CategoryName LIKE '%{escaped}%'";
                }

                ExpensesDataGrid.ItemsSource = view;

                CalculateSelectedPeriodTotal(view);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء البحث:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إجمالي الفترة
        // =========================================================

        private void CalculateSelectedPeriodTotal(DataView view)
        {
            decimal total = 0;

            foreach (DataRowView row in view)
            {
                if (row["Amount"] != DBNull.Value)
                {
                    total += Convert.ToDecimal(row["Amount"]);
                }
            }

            SelectedPeriodTotalText.Text =
                $"{total:N2} ج.م";
        }


        // =========================================================
        // إضافة مصروف
        // =========================================================

        private void AddExpenseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddExpenseWindow window =
                new AddExpenseWindow();

            window.Owner = this;

            bool? result = window.ShowDialog();

            if (result == true)
            {
                LoadExpenses();
                LoadStatistics();
            }
        }


        // =========================================================
        // فتح تفاصيل المصروف
        // =========================================================

        private void ExpensesDataGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (ExpensesDataGrid.SelectedItem == null)
                return;

            if (ExpensesDataGrid.SelectedItem is DataRowView row)
            {
                int expenseID =
                    Convert.ToInt32(row["ExpenseID"]);

                ExpenseDetailsWindow window =
                    new ExpenseDetailsWindow(expenseID);

                window.Owner = this;

                bool? result = window.ShowDialog();

                if (result == true)
                {
                    LoadExpenses();
                    LoadStatistics();
                }
            }
        }
    }
}