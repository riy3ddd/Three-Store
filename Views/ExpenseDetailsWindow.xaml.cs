using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class ExpenseDetailsWindow : Window
    {
        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int expenseID;

        public ExpenseDetailsWindow(int expenseID)
        {
            InitializeComponent();

            this.expenseID = expenseID;

            Loaded += ExpenseDetailsWindow_Loaded;
        }


        // =========================================================
        // تحميل التفاصيل
        // =========================================================

        private void ExpenseDetailsWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadExpense();
        }


        private void LoadExpense()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    SELECT
                        E.ExpenseID,
                        E.ExpenseNumber,
                        E.ExpenseType,
                        E.Description,
                        E.Amount,
                        E.ExpenseDate,
                        E.Notes,
                        E.CreatedBy,
                        E.CreatedAt,
                        EC.CategoryName,
                        U.Username
                    FROM Expenses E
                    LEFT JOIN ExpenseCategories EC
                        ON E.ExpenseCategoryID = EC.ExpenseCategoryID
                    LEFT JOIN Users U
                        ON E.CreatedBy = U.UserID
                    WHERE E.ExpenseID = @ExpenseID
                      AND ISNULL(E.IsDeleted, 0) = 0;";


                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@ExpenseID",
                    System.Data.SqlDbType.Int).Value =
                    expenseID;


                using SqlDataReader reader =
                    command.ExecuteReader();


                if (!reader.Read())
                {
                    MessageBox.Show(
                        "المصروف غير موجود أو تم حذفه.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    DialogResult = true;

                    return;
                }


                // =====================================================
                // رقم المصروف
                // =====================================================

                ExpenseNumberText.Text =
                    reader["ExpenseNumber"]?.ToString()
                    ?? "غير محدد";


                // =====================================================
                // التصنيف
                // =====================================================

                CategoryText.Text =
                    reader["CategoryName"] == DBNull.Value
                        ? reader["ExpenseType"]?.ToString() ?? "غير محدد"
                        : reader["CategoryName"]?.ToString();


                // =====================================================
                // الوصف
                // =====================================================

                DescriptionText.Text =
                    reader["Description"] == DBNull.Value
                        ? "بدون بيان"
                        : reader["Description"]?.ToString();


                // =====================================================
                // المبلغ
                // =====================================================

                decimal amount =
                    reader["Amount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["Amount"]);


                AmountText.Text =
                    $"{amount:N2} ج.م";


                // =====================================================
                // التاريخ
                // =====================================================

                if (reader["ExpenseDate"] != DBNull.Value)
                {
                    DateTime date =
                        Convert.ToDateTime(reader["ExpenseDate"]);

                    DateText.Text =
                        date.ToString("dd/MM/yyyy - hh:mm tt");
                }
                else
                {
                    DateText.Text = "غير محدد";
                }


                // =====================================================
                // المستخدم الذي سجل المصروف
                // =====================================================

                if (reader["Username"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(
                        reader["Username"]?.ToString()))
                {
                    CreatedByText.Text =
                        reader["Username"]?.ToString();
                }
                else
                {
                    CreatedByText.Text =
                        "غير محدد";
                }


                // =====================================================
                // الملاحظات
                // =====================================================

                NotesText.Text =
                    reader["Notes"] == DBNull.Value
                        ? "لا توجد ملاحظات"
                        : reader["Notes"]?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل تفاصيل المصروف:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تعديل
        // =========================================================

        private void EditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            EditExpenseWindow window =
                new EditExpenseWindow(expenseID);

            window.Owner = this;

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadExpense();

                DialogResult = true;
            }
        }


        // =========================================================
        // حذف
        // =========================================================

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult confirmation =
                MessageBox.Show(
                    "هل أنت متأكد من حذف هذا المصروف؟\n\n" +
                    "سيتم إخفاؤه من النظام ولن يتم احتسابه ضمن المصروفات.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
                return;


            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();


                string query = @"
                    UPDATE Expenses
                    SET
                        IsDeleted = 1,
                        DeletedAt = GETDATE()
                    WHERE ExpenseID = @ExpenseID
                      AND ISNULL(IsDeleted, 0) = 0;";


                using SqlCommand command =
                    new SqlCommand(query, connection);


                command.Parameters.Add(
                    "@ExpenseID",
                    System.Data.SqlDbType.Int).Value =
                    expenseID;


                int affectedRows =
                    command.ExecuteNonQuery();


                if (affectedRows > 0)
                {
                    MessageBox.Show(
                        "تم حذف المصروف بنجاح.",
                        "تم الحذف",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show(
                        "المصروف غير موجود أو تم حذفه بالفعل.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف المصروف:\n\n" +
                    ex.Message,
                    "خطأ في حذف المصروف",
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