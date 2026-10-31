using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class EditCreditAccountWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int accountId;


        public EditCreditAccountWindow(int id)
        {
            InitializeComponent();

            accountId = id;

            LoadCustomer();
        }


        private void LoadCustomer()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                string query = @"
                    SELECT
                        CustomerName,
                        CustomerPhone
                    FROM CreditAccounts
                    WHERE
                        CreditAccountID =
                        @CreditAccountID
                        AND IsActive = 1;";


                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);


                command.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    accountId;


                connection.Open();


                using SqlDataReader reader =
                    command.ExecuteReader();


                if (!reader.Read())
                {
                    MessageBox.Show(
                        "حساب العميل غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }


                CustomerNameTextBox.Text =
                    reader["CustomerName"]
                        ?.ToString()
                        ?? "";


                CustomerPhoneTextBox.Text =
                    reader["CustomerPhone"] ==
                    DBNull.Value
                        ? ""
                        : reader["CustomerPhone"]
                            .ToString()
                            ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات العميل:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string customerName =
                    CustomerNameTextBox.Text.Trim();

                string customerPhone =
                    CustomerPhoneTextBox.Text.Trim();


                if (string.IsNullOrWhiteSpace(
                        customerName))
                {
                    MessageBox.Show(
                        "اسم العميل مطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);


                string query = @"
                    UPDATE CreditAccounts
                    SET
                        CustomerName =
                            @CustomerName,

                        CustomerPhone =
                            @CustomerPhone,

                        UpdatedAt =
                            GETDATE()

                    WHERE
                        CreditAccountID =
                        @CreditAccountID

                        AND IsActive = 1;";


                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);


                command.Parameters.Add(
                    "@CustomerName",
                    SqlDbType.NVarChar,
                    150).Value =
                    customerName;


                command.Parameters.Add(
                    "@CustomerPhone",
                    SqlDbType.NVarChar,
                    30).Value =
                    string.IsNullOrWhiteSpace(
                        customerPhone)
                        ? DBNull.Value
                        : customerPhone;


                command.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    accountId;


                connection.Open();


                int affectedRows =
                    command.ExecuteNonQuery();


                if (affectedRows == 0)
                {
                    MessageBox.Show(
                        "لم يتم تعديل بيانات العميل.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                MessageBox.Show(
                    "تم تعديل بيانات العميل بنجاح.",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تعديل بيانات العميل:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}