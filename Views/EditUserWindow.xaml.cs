using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class EditUserWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int userId;

        public EditUserWindow(
            int userId,
            string username,
            string role,
            bool isActive)
        {
            InitializeComponent();

            this.userId = userId;

            // اسم المستخدم
            UsernameTextBox.Text = username;

            // الصلاحية الحالية
            if (role == "Admin")
            {
                RoleComboBox.SelectedIndex = 0;
            }
            else
            {
                RoleComboBox.SelectedIndex = 1;
            }

            // حالة الحساب
            StatusComboBox.SelectedIndex =
                isActive ? 0 : 1;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";

            string username =
                UsernameTextBox.Text.Trim();

            string newPassword =
                PasswordBox.Password;

            ComboBoxItem? selectedRole =
                RoleComboBox.SelectedItem as ComboBoxItem;

            ComboBoxItem? selectedStatus =
                StatusComboBox.SelectedItem as ComboBoxItem;


            // التأكد من اسم المستخدم
            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorTextBlock.Text =
                    "من فضلك أدخل اسم المستخدم.";

                UsernameTextBox.Focus();

                return;
            }


            // التأكد من اختيار الصلاحية
            if (selectedRole == null)
            {
                ErrorTextBlock.Text =
                    "من فضلك اختر الصلاحية.";

                return;
            }


            // التأكد من اختيار حالة الحساب
            if (selectedStatus == null)
            {
                ErrorTextBlock.Text =
                    "من فضلك اختر حالة الحساب.";

                return;
            }


            // مهم جداً:
            // نأخذ القيمة الحقيقية من Tag
            // وليس النص الظاهر في Content

            string role =
                selectedRole.Tag?.ToString()
                ?? "Employee";


            // تحديد حالة الحساب

            bool isActive =
                selectedStatus.Content?.ToString()
                == "نشط";


            // التحقق من كلمة المرور

            if (!string.IsNullOrWhiteSpace(newPassword)
                && newPassword.Length < 6)
            {
                ErrorTextBlock.Text =
                    "كلمة المرور يجب أن تكون 6 أحرف على الأقل.";

                PasswordBox.Focus();

                return;
            }


            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();


                // التأكد أن اسم المستخدم
                // غير مستخدم لحساب آخر

                string checkUsernameQuery = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Username = @Username
                    AND UserID <> @UserID";


                using SqlCommand checkCommand =
                    new SqlCommand(
                        checkUsernameQuery,
                        connection);


                checkCommand.Parameters.Add(
                    "@Username",
                    System.Data.SqlDbType.NVarChar,
                    50).Value = username;


                checkCommand.Parameters.Add(
                    "@UserID",
                    System.Data.SqlDbType.Int).Value = userId;


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


                // منع تعطيل آخر Admin نشط

                if (role == "Admin" && !isActive)
                {
                    string countAdminQuery = @"
                        SELECT COUNT(*)
                        FROM Users
                        WHERE Role = 'Admin'
                        AND IsActive = 1
                        AND UserID <> @UserID";


                    using SqlCommand adminCommand =
                        new SqlCommand(
                            countAdminQuery,
                            connection);


                    adminCommand.Parameters.Add(
                        "@UserID",
                        System.Data.SqlDbType.Int).Value = userId;


                    int activeAdmins =
                        Convert.ToInt32(
                            adminCommand.ExecuteScalar());


                    if (activeAdmins == 0)
                    {
                        ErrorTextBlock.Text =
                            "لا يمكن تعطيل آخر Admin نشط في النظام.";

                        return;
                    }
                }


                // =========================================
                // تحديث البيانات بدون تغيير كلمة المرور
                // =========================================

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    string updateQuery = @"
                        UPDATE Users
                        SET
                            Username = @Username,
                            Role = @Role,
                            IsActive = @IsActive
                        WHERE UserID = @UserID";


                    using SqlCommand updateCommand =
                        new SqlCommand(
                            updateQuery,
                            connection);


                    updateCommand.Parameters.Add(
                        "@Username",
                        System.Data.SqlDbType.NVarChar,
                        50).Value = username;


                    updateCommand.Parameters.Add(
                        "@Role",
                        System.Data.SqlDbType.NVarChar,
                        20).Value = role;


                    updateCommand.Parameters.Add(
                        "@IsActive",
                        System.Data.SqlDbType.Bit).Value = isActive;


                    updateCommand.Parameters.Add(
                        "@UserID",
                        System.Data.SqlDbType.Int).Value = userId;


                    updateCommand.ExecuteNonQuery();
                }


                // =========================================
                // تحديث البيانات مع تغيير كلمة المرور
                // =========================================

                else
                {
                    string passwordHash =
                        PasswordHasher.HashPassword(
                            newPassword);


                    string updateQuery = @"
                        UPDATE Users
                        SET
                            Username = @Username,
                            PasswordHash = @PasswordHash,
                            Role = @Role,
                            IsActive = @IsActive
                        WHERE UserID = @UserID";


                    using SqlCommand updateCommand =
                        new SqlCommand(
                            updateQuery,
                            connection);


                    updateCommand.Parameters.Add(
                        "@Username",
                        System.Data.SqlDbType.NVarChar,
                        50).Value = username;


                    updateCommand.Parameters.Add(
                        "@PasswordHash",
                        System.Data.SqlDbType.NVarChar,
                        255).Value = passwordHash;


                    updateCommand.Parameters.Add(
                        "@Role",
                        System.Data.SqlDbType.NVarChar,
                        20).Value = role;


                    updateCommand.Parameters.Add(
                        "@IsActive",
                        System.Data.SqlDbType.Bit).Value = isActive;


                    updateCommand.Parameters.Add(
                        "@UserID",
                        System.Data.SqlDbType.Int).Value = userId;


                    updateCommand.ExecuteNonQuery();
                }


                // =========================================
                // تحديث Session الحالية لو بنعدل
                // الحساب الذي سجل الدخول حالياً
                // =========================================

                if (SessionManager.CurrentUser != null &&
                    userId == SessionManager.CurrentUser.UserID)
                {
                    SessionManager.CurrentUser.Username =
                        username;

                    SessionManager.CurrentUser.Role =
                        role;

                    SessionManager.CurrentUser.IsActive =
                        isActive;
                }


                MessageBox.Show(
                    "تم حفظ تعديلات العضو بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ التعديلات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}