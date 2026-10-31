using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class UsersWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private DataTable allUsers = new DataTable();

        public UsersWindow()
        {
            InitializeComponent();

            // حماية النافذة نفسها
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show(
                    "ليس لديك صلاحية للوصول إلى إدارة الأعضاء.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                DataTable table = new DataTable();

                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    SELECT
                        UserID,
                        Username,
                        Role,
                        IsActive,
                        CreatedAt
                    FROM Users
                    ORDER BY UserID DESC";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);

                adapter.Fill(table);

                allUsers = table;

                UsersDataGrid.ItemsSource =
                    allUsers.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الأعضاء:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            string search =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(search))
            {
                allUsers.DefaultView.RowFilter = "";
                return;
            }

            allUsers.DefaultView.RowFilter =
                $"Username LIKE '%{search.Replace("'", "''")}%'";
        }

        private void AddUserButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddUserWindow window =
                new AddUserWindow();

            window.Owner = this;

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadUsers();
            }
        }

        private void EditUserButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext is not DataRowView row)
                return;

            int userId =
                Convert.ToInt32(row["UserID"]);

            string username =
                row["Username"]?.ToString() ?? "";

            string role =
                row["Role"]?.ToString() ?? "Employee";

            bool isActive =
                Convert.ToBoolean(row["IsActive"]);

            EditUserWindow window =
                new EditUserWindow(
                    userId,
                    username,
                    role,
                    isActive);

            window.Owner = this;

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadUsers();
            }
        }

        private void DeleteUserButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext is not DataRowView row)
                return;

            int userId =
                Convert.ToInt32(row["UserID"]);

            string username =
                row["Username"]?.ToString() ?? "";

            string role =
                row["Role"]?.ToString() ?? "";

            // منع حذف الحساب الحالي
            if (SessionManager.CurrentUser != null &&
                userId == SessionManager.CurrentUser.UserID)
            {
                MessageBox.Show(
                    "لا يمكنك حذف الحساب الذي تستخدمه حاليًا.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // منع حذف آخر Admin نشط
            if (role == "Admin" &&
                IsLastActiveAdmin(userId))
            {
                MessageBox.Show(
                    "لا يمكن حذف آخر حساب Admin نشط في النظام.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    $"هل أنت متأكد من حذف العضو:\n\n{username} ؟\n\n" +
                    "سيتم حذف الحساب نهائيًا.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    DELETE FROM Users
                    WHERE UserID = @UserID";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@UserID",
                    System.Data.SqlDbType.Int).Value = userId;

                command.ExecuteNonQuery();

                MessageBox.Show(
                    "تم حذف العضو بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف العضو:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool IsLastActiveAdmin(int userId)
        {
            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            string query = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Role = 'Admin'
                AND IsActive = 1
                AND UserID <> @UserID";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@UserID",
                System.Data.SqlDbType.Int).Value = userId;

            int count =
                Convert.ToInt32(
                    command.ExecuteScalar());

            return count == 0;
        }
    }
}