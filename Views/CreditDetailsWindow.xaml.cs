using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class CreditDetailsWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int accountId;


        public CreditDetailsWindow(int id)
        {
            InitializeComponent();

            accountId = id;

            LoadDetails();
        }


        private void LoadDetails()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                string summaryQuery = @"
                    SELECT

                        ca.CustomerName,

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
                        ) AS TotalPaid

                    FROM CreditAccounts ca

                    LEFT JOIN CreditTransactions ct
                        ON ca.CreditAccountID =
                           ct.CreditAccountID

                    WHERE
                        ca.CreditAccountID =
                        @CreditAccountID

                    GROUP BY
                        ca.CustomerName;";


                using SqlCommand summaryCommand =
                    new SqlCommand(
                        summaryQuery,
                        connection);


                summaryCommand.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    accountId;


                connection.Open();


                using SqlDataReader reader =
                    summaryCommand.ExecuteReader();


                if (reader.Read())
                {
                    string customerName =
                        reader["CustomerName"]
                            ?.ToString()
                            ?? "";


                    decimal totalCredit =
                        reader["TotalCredit"] ==
                        DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(
                                reader["TotalCredit"]);


                    decimal totalPaid =
                        reader["TotalPaid"] ==
                        DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(
                                reader["TotalPaid"]);


                    decimal remaining =
                        totalCredit -
                        totalPaid;


                    if (remaining < 0)
                        remaining = 0;


                    CustomerNameHeaderText.Text =
                        $"العميل: {customerName}";


                    TotalCreditText.Text =
                        $"{totalCredit:N2} ج.م";


                    TotalPaidText.Text =
                        $"{totalPaid:N2} ج.م";


                    RemainingText.Text =
                        $"{remaining:N2} ج.م";
                }


                reader.Close();


                string transactionQuery = @"
                    SELECT

                        TransactionDate,

                        CASE
                            WHEN TransactionType = N'Credit'
                                THEN N'إضافة آجل'
                            ELSE N'تسديد'
                        END AS TransactionTypeText,

                        Amount,

                        ISNULL(
                            PaymentMethod,
                            N'-'
                        ) AS PaymentMethodDisplay,

                        ISNULL(
                            Notes,
                            N'-'
                        ) AS NotesDisplay

                    FROM CreditTransactions

                    WHERE
                        CreditAccountID =
                        @CreditAccountID

                    ORDER BY
                        TransactionDate DESC,
                        CreditTransactionID DESC;";


                DataTable table =
                    new DataTable();


                using SqlCommand transactionCommand =
                    new SqlCommand(
                        transactionQuery,
                        connection);


                transactionCommand.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    accountId;


                using SqlDataAdapter adapter =
                    new SqlDataAdapter(
                        transactionCommand);


                adapter.Fill(table);


                TransactionsDataGrid.ItemsSource =
                    table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل كشف الحساب:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}