using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class AddExpenseWindow : Window
    {
        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddExpenseWindow()
        {
            InitializeComponent();

            ExpenseDatePicker.SelectedDate = DateTime.Today;

            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    SELECT
                        ExpenseCategoryID,
                        CategoryName
                    FROM ExpenseCategories
                    WHERE IsActive = 1
                    ORDER BY CategoryName;";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                using SqlDataReader reader =
                    command.ExecuteReader();

                CategoryComboBox.Items.Clear();

                while (reader.Read())
                {
                    CategoryComboBox.Items.Add(
                        new ExpenseCategoryItem
                        {
                            ExpenseCategoryID =
                                Convert.ToInt32(reader["ExpenseCategoryID"]),

                            CategoryName =
                                reader["CategoryName"]?.ToString() ?? ""
                        });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل تصنيفات المصروفات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // =========================================================
            // التأكد من وجود مستخدم مسجل الدخول
            // =========================================================

            if (SessionManager.CurrentUser == null)
            {
                MessageBox.Show(
                    "لا يوجد مستخدم مسجل الدخول حاليًا.\n\n" +
                    "يرجى تسجيل الدخول أولًا قبل تسجيل المصروف.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int currentUserId =
                SessionManager.CurrentUser.UserID;


            // =========================================================
            // التحقق من التصنيف
            // =========================================================

            if (CategoryComboBox.SelectedItem is not ExpenseCategoryItem selectedCategory)
            {
                MessageBox.Show(
                    "من فضلك اختر تصنيف المصروف.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CategoryComboBox.Focus();
                return;
            }


            // =========================================================
            // التحقق من الوصف
            // =========================================================

            string description =
                DescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show(
                    "من فضلك اكتب وصف المصروف.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                DescriptionTextBox.Focus();
                return;
            }


            // =========================================================
            // التحقق من المبلغ
            // =========================================================

            if (!decimal.TryParse(
                    AmountTextBox.Text.Trim(),
                    out decimal amount))
            {
                MessageBox.Show(
                    "من فضلك أدخل مبلغًا صحيحًا.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                AmountTextBox.Focus();
                return;
            }

            if (amount <= 0)
            {
                MessageBox.Show(
                    "مبلغ المصروف يجب أن يكون أكبر من صفر.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                AmountTextBox.Focus();
                return;
            }


            // =========================================================
            // التاريخ والملاحظات
            // =========================================================

            DateTime expenseDate =
                ExpenseDatePicker.SelectedDate ?? DateTime.Today;

            string notes =
                NotesTextBox.Text.Trim();


            // =========================================================
            // حفظ المصروف
            // =========================================================

            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                using SqlTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    string temporaryNumber = "E";

                    string insertQuery = @"
                        INSERT INTO Expenses
                        (
                            ExpenseNumber,
                            ExpenseType,
                            Description,
                            Amount,
                            ExpenseDate,
                            Notes,
                            CreatedBy,
                            CreatedAt,
                            ExpenseCategoryID,
                            IsDeleted
                        )
                        OUTPUT INSERTED.ExpenseID
                        VALUES
                        (
                            @ExpenseNumber,
                            @ExpenseType,
                            @Description,
                            @Amount,
                            @ExpenseDate,
                            @Notes,
                            @CreatedBy,
                            GETDATE(),
                            @ExpenseCategoryID,
                            0
                        );";


                    using SqlCommand insertCommand =
                        new SqlCommand(
                            insertQuery,
                            connection,
                            transaction);


                    insertCommand.Parameters.Add(
                        "@ExpenseNumber",
                        System.Data.SqlDbType.NVarChar,
                        50).Value =
                            temporaryNumber;


                    insertCommand.Parameters.Add(
                        "@ExpenseType",
                        System.Data.SqlDbType.NVarChar,
                        100).Value =
                            selectedCategory.CategoryName;


                    insertCommand.Parameters.Add(
                        "@Description",
                        System.Data.SqlDbType.NVarChar,
                        500).Value =
                            description;


                    var amountParameter =
                        insertCommand.Parameters.Add(
                            "@Amount",
                            System.Data.SqlDbType.Decimal);

                    amountParameter.Precision = 18;
                    amountParameter.Scale = 2;
                    amountParameter.Value = amount;


                    insertCommand.Parameters.Add(
                        "@ExpenseDate",
                        System.Data.SqlDbType.DateTime).Value =
                            expenseDate;


                    insertCommand.Parameters.Add(
                        "@Notes",
                        System.Data.SqlDbType.NVarChar,
                        -1).Value =
                            string.IsNullOrWhiteSpace(notes)
                                ? DBNull.Value
                                : notes;


                    insertCommand.Parameters.Add(
                        "@CreatedBy",
                        System.Data.SqlDbType.Int).Value =
                            currentUserId;


                    insertCommand.Parameters.Add(
                        "@ExpenseCategoryID",
                        System.Data.SqlDbType.Int).Value =
                            selectedCategory.ExpenseCategoryID;


                    int expenseId =
                        Convert.ToInt32(
                            insertCommand.ExecuteScalar());


                    // =================================================
                    // إنشاء رقم المصروف النهائي
                    // =================================================

                    string expenseNumber =
                        $"EXP-{expenseId:D6}";


                    string updateQuery = @"
                        UPDATE Expenses
                        SET ExpenseNumber = @ExpenseNumber
                        WHERE ExpenseID = @ExpenseID;";


                    using SqlCommand updateCommand =
                        new SqlCommand(
                            updateQuery,
                            connection,
                            transaction);


                    updateCommand.Parameters.Add(
                        "@ExpenseNumber",
                        System.Data.SqlDbType.NVarChar,
                        50).Value =
                            expenseNumber;


                    updateCommand.Parameters.Add(
                        "@ExpenseID",
                        System.Data.SqlDbType.Int).Value =
                            expenseId;


                    updateCommand.ExecuteNonQuery();


                    transaction.Commit();


                    MessageBox.Show(
                        "تم تسجيل المصروف بنجاح.\n\n" +
                        "رقم المصروف: " +
                        expenseNumber,
                        "تم الحفظ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                    DialogResult = true;
                    Close();
                }
                catch
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ المصروف:\n\n" +
                    ex.Message,
                    "خطأ في حفظ المصروف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }


    public class ExpenseCategoryItem
    {
        public int ExpenseCategoryID { get; set; }

        public string CategoryName { get; set; } = "";

        public override string ToString()
        {
            return CategoryName;
        }
    }
}