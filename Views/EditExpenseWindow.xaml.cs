using System;
using System.Data;
using System.Globalization;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class EditExpenseWindow : Window
    {
        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int expenseID;

        public EditExpenseWindow(int expenseID)
        {
            InitializeComponent();

            this.expenseID = expenseID;

            Loaded += EditExpenseWindow_Loaded;
        }


        // =========================================================
        // تحميل
        // =========================================================

        private void EditExpenseWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadCategories();
            LoadExpense();
        }


        // =========================================================
        // التصنيفات
        // =========================================================

        private void LoadCategories()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                string query = @"
                    SELECT
                        ExpenseCategoryID,
                        CategoryName
                    FROM ExpenseCategories
                    WHERE ISNULL(IsActive, 1) = 1
                    ORDER BY CategoryName;";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                SqlDataAdapter adapter =
                    new SqlDataAdapter(command);

                DataTable table =
                    new DataTable();

                adapter.Fill(table);

                CategoryComboBox.ItemsSource =
                    table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل التصنيفات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تحميل المصروف
        // =========================================================

        private void LoadExpense()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    SELECT
                        ExpenseNumber,
                        ExpenseCategoryID,
                        ExpenseType,
                        Description,
                        Amount,
                        ExpenseDate,
                        Notes
                    FROM Expenses
                    WHERE ExpenseID = @ExpenseID
                      AND ISNULL(IsDeleted, 0) = 0;";


                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@ExpenseID",
                    SqlDbType.Int).Value =
                    expenseID;


                using SqlDataReader reader =
                    command.ExecuteReader();


                if (!reader.Read())
                {
                    MessageBox.Show(
                        "المصروف غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }


                ExpenseNumberText.Text =
                    reader["ExpenseNumber"]?.ToString();


                DescriptionTextBox.Text =
                    reader["Description"] == DBNull.Value
                        ? ""
                        : reader["Description"]?.ToString();


                AmountTextBox.Text =
                    reader["Amount"] == DBNull.Value
                        ? "0"
                        : Convert.ToDecimal(
                            reader["Amount"])
                            .ToString(
                                "0.00",
                                CultureInfo.InvariantCulture);


                if (reader["ExpenseDate"] != DBNull.Value)
                {
                    ExpenseDatePicker.SelectedDate =
                        Convert.ToDateTime(
                            reader["ExpenseDate"]);
                }


                NotesTextBox.Text =
                    reader["Notes"] == DBNull.Value
                        ? ""
                        : reader["Notes"]?.ToString();


                if (reader["ExpenseCategoryID"] != DBNull.Value)
                {
                    CategoryComboBox.SelectedValue =
                        Convert.ToInt32(
                            reader["ExpenseCategoryID"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المصروف:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // حفظ
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (CategoryComboBox.SelectedValue == null)
                {
                    MessageBox.Show(
                        "من فضلك اختر نوع المصروف.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                string description =
                    DescriptionTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(description))
                {
                    MessageBox.Show(
                        "من فضلك اكتب البيان.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                string amountText =
                    AmountTextBox.Text.Trim()
                        .Replace(",", ".");


                if (!decimal.TryParse(
                        amountText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out decimal amount))
                {
                    MessageBox.Show(
                        "من فضلك أدخل مبلغًا صحيحًا.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (amount <= 0)
                {
                    MessageBox.Show(
                        "المبلغ يجب أن يكون أكبر من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                int categoryID =
                    Convert.ToInt32(
                        CategoryComboBox.SelectedValue);


                DateTime expenseDate =
                    ExpenseDatePicker.SelectedDate
                    ?? DateTime.Today;


                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();


                string query = @"
                    UPDATE Expenses
                    SET
                        ExpenseCategoryID = @ExpenseCategoryID,
                        ExpenseType = @ExpenseType,
                        Description = @Description,
                        Amount = @Amount,
                        ExpenseDate = @ExpenseDate,
                        Notes = @Notes
                    WHERE ExpenseID = @ExpenseID
                      AND ISNULL(IsDeleted, 0) = 0;";


                using SqlCommand command =
                    new SqlCommand(query, connection);


                DataRowView selectedCategory =
                    CategoryComboBox.SelectedItem
                    as DataRowView;


                string categoryName =
                    selectedCategory?["CategoryName"]?.ToString()
                    ?? "أخرى";


                command.Parameters.Add(
                    "@ExpenseCategoryID",
                    SqlDbType.Int).Value =
                    categoryID;


                command.Parameters.Add(
                    "@ExpenseType",
                    SqlDbType.NVarChar,
                    100).Value =
                    categoryName;


                command.Parameters.Add(
                    "@Description",
                    SqlDbType.NVarChar,
                    500).Value =
                    description;


                SqlParameter amountParameter =
                    command.Parameters.Add(
                        "@Amount",
                        SqlDbType.Decimal);

                amountParameter.Precision = 18;
                amountParameter.Scale = 2;
                amountParameter.Value = amount;


                command.Parameters.Add(
                    "@ExpenseDate",
                    SqlDbType.DateTime).Value =
                    expenseDate;


                command.Parameters.Add(
                    "@Notes",
                    SqlDbType.NVarChar,
                    1000).Value =
                    string.IsNullOrWhiteSpace(NotesTextBox.Text)
                        ? DBNull.Value
                        : NotesTextBox.Text.Trim();


                command.Parameters.Add(
                    "@ExpenseID",
                    SqlDbType.Int).Value =
                    expenseID;


                int affected =
                    command.ExecuteNonQuery();


                if (affected > 0)
                {
                    MessageBox.Show(
                        "تم تعديل المصروف بنجاح.",
                        "تم الحفظ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show(
                        "لم يتم العثور على المصروف.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تعديل المصروف:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إلغاء
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}