using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class AddUserWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";

            string username =
                UsernameTextBox.Text.Trim();

            string password =
                PasswordBox.Password;

            string confirmPassword =
                ConfirmPasswordBox.Password;

            // التأكد من اسم المستخدم
            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorTextBlock.Text =
                    "من فضلك أدخل اسم المستخدم.";

                UsernameTextBox.Focus();
                return;
            }

            // التأكد من كلمة المرور
            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text =
                    "من فضلك أدخل كلمة المرور.";

                PasswordBox.Focus();
                return;
            }

            // الحد الأدنى لكلمة المرور
            if (password.Length < 6)
            {
                ErrorTextBlock.Text =
                    "كلمة المرور يجب أن تكون 6 أحرف على الأقل.";

                PasswordBox.Focus();
                return;
            }

            // تأكيد كلمة المرور
            if (password != confirmPassword)
            {
                ErrorTextBlock.Text =
                    "كلمتا المرور غير متطابقتين.";

                ConfirmPasswordBox.Clear();
                ConfirmPasswordBox.Focus();
                return;
            }

            // الحصول على الصلاحية
            // الحصول على الصلاحية
            ComboBoxItem? selectedRole =
                RoleComboBox.SelectedItem as ComboBoxItem;

            if (selectedRole == null)
            {
                ErrorTextBlock.Text =
                    "من فضلك اختر الصلاحية.";

                return;
            }

            string role =
                selectedRole.Tag?.ToString() ?? "Employee";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                // التأكد أن Username غير مستخدم
                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Username = @Username";

                using SqlCommand checkCommand =
                    new SqlCommand(
                        checkQuery,
                        connection);

                checkCommand.Parameters.Add(
                    "@Username",
                    System.Data.SqlDbType.NVarChar,
                    50).Value = username;

                int existingUsers =
                    Convert.ToInt32(
                        checkCommand.ExecuteScalar());

                if (existingUsers > 0)
                {
                    ErrorTextBlock.Text =
                        "اسم المستخدم موجود بالفعل.";

                    UsernameTextBox.Focus();
                    return;
                }

                // إنشاء Hash لكلمة المرور
                string passwordHash =
                    PasswordHasher.HashPassword(password);

                // إضافة المستخدم
                string insertQuery = @"
                    INSERT INTO Users
                    (
                        Username,
                        PasswordHash,
                        Role,
                        IsActive
                    )
                    VALUES
                    (
                        @Username,
                        @PasswordHash,
                        @Role,
                        1
                    )";

                using SqlCommand insertCommand =
                    new SqlCommand(
                        insertQuery,
                        connection);

                insertCommand.Parameters.Add(
                    "@Username",
                    System.Data.SqlDbType.NVarChar,
                    50).Value = username;

                insertCommand.Parameters.Add(
                    "@PasswordHash",
                    System.Data.SqlDbType.NVarChar,
                    255).Value = passwordHash;

                insertCommand.Parameters.Add(
                    "@Role",
                    System.Data.SqlDbType.NVarChar,
                    20).Value = role;

                insertCommand.ExecuteNonQuery();

                MessageBox.Show(
                    "تمت إضافة العضو بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء إضافة العضو:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}