using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class CreditWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public CreditWindow()
        {
            InitializeComponent();

            Loaded += CreditWindow_Loaded;
        }


        // =========================================================
        // LOADED
        // =========================================================

        private void CreditWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadCreditData();
        }


        // =========================================================
        // LOAD CREDIT DATA
        // =========================================================

        private void LoadCreditData()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    WITH CreditBalances AS
                    (
                        SELECT
                            ca.CreditAccountID,
                            ca.CustomerName,
                            ca.CustomerPhone,

                            ISNULL
                            (
                                SUM
                                (
                                    CASE
                                        WHEN ct.TransactionType = N'Credit'
                                            THEN ct.Amount
                                        ELSE 0
                                    END
                                ),
                                0
                            ) AS TotalCredit,

                            ISNULL
                            (
                                SUM
                                (
                                    CASE
                                        WHEN ct.TransactionType = N'Payment'
                                            THEN ct.Amount
                                        ELSE 0
                                    END
                                ),
                                0
                            ) AS TotalPaid,

                            MAX(ct.TransactionDate)
                                AS LastTransactionDate

                        FROM dbo.CreditAccounts ca

                        INNER JOIN dbo.CreditTransactions ct
                            ON ca.CreditAccountID =
                               ct.CreditAccountID

                        WHERE
                            ca.IsActive = 1

                        GROUP BY
                            ca.CreditAccountID,
                            ca.CustomerName,
                            ca.CustomerPhone
                    )

                    SELECT
                        CreditAccountID,
                        CustomerName,

                        ISNULL
                        (
                            CustomerPhone,
                            N'غير متوفر'
                        ) AS CustomerPhone,

                        TotalCredit,

                        TotalPaid,

                        (TotalCredit - TotalPaid)
                            AS Remaining,

                        LastTransactionDate

                    FROM CreditBalances

                    WHERE
                        (TotalCredit - TotalPaid) > 0

                    ORDER BY
                        Remaining DESC,
                        CustomerName;
                ";


                DataTable table =
                    new DataTable();


                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);


                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);


                adapter.Fill(table);


                CreditDataGrid.ItemsSource =
                    table.DefaultView;


                LoadSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات الآجل:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // LOAD SUMMARY
        // =========================================================

        private void LoadSummary()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);


                string query = @"
                    SELECT

                        ISNULL
                        (
                            (
                                SELECT
                                    SUM
                                    (
                                        CASE
                                            WHEN ct.TransactionType = N'Credit'
                                                THEN ct.Amount

                                            WHEN ct.TransactionType = N'Payment'
                                                THEN -ct.Amount

                                            ELSE 0
                                        END
                                    )
                                FROM dbo.CreditTransactions ct
                            ),
                            0
                        )
                        AS TotalOutstanding,

                        ISNULL
                        (
                            (
                                SELECT
                                    SUM(ct.Amount)
                                FROM dbo.CreditTransactions ct
                                WHERE
                                    ct.TransactionType = N'Payment'
                            ),
                            0
                        )
                        AS TotalPayments;
                ";


                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);


                connection.Open();


                using SqlDataReader reader =
                    command.ExecuteReader();


                if (reader.Read())
                {
                    decimal totalOutstanding =
                        reader["TotalOutstanding"] ==
                        DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(
                                reader["TotalOutstanding"]);


                    decimal totalPayments =
                        reader["TotalPayments"] ==
                        DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(
                                reader["TotalPayments"]);


                    if (totalOutstanding < 0)
                    {
                        totalOutstanding = 0;
                    }


                    TotalCreditTextBlock.Text =
                        $"{totalOutstanding:N2} ج.م";


                    TotalPaymentsTextBlock.Text =
                        $"{totalPayments:N2} ج.م";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حساب ملخص الآجل:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // SEARCH
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (CreditDataGrid.ItemsSource
                is not DataView view)
            {
                return;
            }


            string searchText =
                SearchTextBox.Text.Trim();


            if (string.IsNullOrWhiteSpace(searchText))
            {
                view.RowFilter = "";

                return;
            }


            string safeSearch =
                searchText.Replace(
                    "'",
                    "''");


            view.RowFilter =
                $"CustomerName LIKE '%{safeSearch}%' " +
                $"OR CustomerPhone LIKE '%{safeSearch}%'";
        }


        // =========================================================
        // GET SELECTED ACCOUNT ID
        // =========================================================

        private int? GetSelectedCreditAccountId()
        {
            if (CreditDataGrid.SelectedItem
                is DataRowView row)
            {
                if (row["CreditAccountID"] !=
                    DBNull.Value)
                {
                    return Convert.ToInt32(
                        row["CreditAccountID"]);
                }
            }

            return null;
        }


        // =========================================================
        // EDIT CUSTOMER
        // =========================================================

        private void EditCustomerButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? accountId =
                GetSelectedCreditAccountId();


            if (accountId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عميلًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            EditCreditAccountWindow window =
                new EditCreditAccountWindow(
                    accountId.Value);


            window.Owner =
                this;


            bool? result =
                window.ShowDialog();


            if (result == true)
            {
                LoadCreditData();
            }
        }


        // =========================================================
        // PAY CREDIT
        // =========================================================

        private void PayCreditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? accountId =
                GetSelectedCreditAccountId();


            if (accountId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عميلًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            CreditPaymentWindow window =
                new CreditPaymentWindow(
                    accountId.Value);


            window.Owner =
                this;


            bool? result =
                window.ShowDialog();


            if (result == true)
            {
                LoadCreditData();
            }
        }


        // =========================================================
        // ACCOUNT DETAILS
        // =========================================================

        private void CreditDataGrid_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            int? accountId =
                GetSelectedCreditAccountId();


            if (accountId == null)
            {
                return;
            }


            CreditDetailsWindow window =
                new CreditDetailsWindow(
                    accountId.Value);


            window.Owner =
                this;


            window.ShowDialog();
        }
    }
}