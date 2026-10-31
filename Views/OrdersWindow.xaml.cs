using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Views
{
    public partial class OrdersWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private string currentStatus = "الجميع";


        public OrdersWindow()
        {
            InitializeComponent();

            // مهم:
            // النافذة مستقلة وليست تابعة للرئيسية
            Owner = null;

            Loaded += OrdersWindow_Loaded;
        }


        // =========================================================
        // تحميل النافذة
        // =========================================================
        private void OrdersWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("الجميع");
        }


        // =========================================================
        // زر الرجوع
        // =========================================================
        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // =========================================================
        // تحميل الطلبات
        // =========================================================
        private void LoadOrders(string status)
        {
            try
            {
                currentStatus = status;

                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query;

                if (status == "الجميع")
                {
                    query = @"
                        SELECT
                            OrderID,
                            CustomerName,
                            CustomerPhone,
                            ProductID,
                            ProductName,
                            Quantity,
                            Note,
                            OrderDate,
                            Status,
                            IsManualProduct
                        FROM CustomerOrders
                        ORDER BY OrderID DESC";
                }
                else
                {
                    query = @"
                        SELECT
                            OrderID,
                            CustomerName,
                            CustomerPhone,
                            ProductID,
                            ProductName,
                            Quantity,
                            Note,
                            OrderDate,
                            Status,
                            IsManualProduct
                        FROM CustomerOrders
                        WHERE Status = @Status
                        ORDER BY OrderID DESC";
                }


                using SqlCommand command =
                    new SqlCommand(query, connection);


                if (status != "الجميع")
                {
                    command.Parameters.Add(
                        "@Status",
                        SqlDbType.NVarChar,
                        50).Value = status;
                }


                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);


                DataTable table =
                    new DataTable();


                adapter.Fill(table);


                OrdersGrid.ItemsSource =
                    table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الطلبيات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إضافة طلبية
        // =========================================================
        private void AddOrderButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AddOrderWindow window =
                    new AddOrderWindow();

                window.Owner = this;

                bool? result =
                    window.ShowDialog();

                if (result == true)
                {
                    LoadOrders(currentStatus);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح إضافة الطلبية:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // التبويبات
        // =========================================================
        private void AllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("الجميع");
        }


        private void ReadyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("الجاهزة");
        }


        private void CancelledButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("الملغية");
        }


        private void PendingButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("قيد الانتظار");
        }


        private void CompletedButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("تمت");
        }


        private void DeliveredButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadOrders("تم التسليم");
        }


        // =========================================================
        // ترقيم الصفوف
        // =========================================================
        private void OrdersGrid_LoadingRow(
            object sender,
            DataGridRowEventArgs e)
        {
            e.Row.Header =
                e.Row.GetIndex() + 1;
        }


        // =========================================================
        // دبل كليك على الطلبية
        // =========================================================
        private void OrdersGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem
                is not DataRowView row)
            {
                return;
            }

            try
            {
                OrderDetailsWindow detailsWindow =
                    new OrderDetailsWindow(row);

                detailsWindow.Owner = this;

                detailsWindow.ShowDialog();

                OrdersGrid.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح تفاصيل الطلبية:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // ComboBox الحالة
        // =========================================================
        private void StatusComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // لا يوجد حفظ تلقائي
        }


        // =========================================================
        // حفظ الحالة
        // =========================================================
        private void SaveStatusButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;


            if (button.DataContext
                is not DataRowView row)
            {
                return;
            }


            try
            {
                if (!int.TryParse(
                        row["OrderID"]?.ToString(),
                        out int orderId))
                {
                    MessageBox.Show(
                        "تعذر تحديد رقم الطلبية.",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }


                string newStatus =
                    row["Status"]?.ToString() ?? "";


                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    MessageBox.Show(
                        "من فضلك اختر حالة صحيحة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();


                string query = @"
                    UPDATE CustomerOrders
                    SET Status = @Status
                    WHERE OrderID = @OrderID";


                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);


                command.Parameters.Add(
                    "@Status",
                    SqlDbType.NVarChar,
                    50).Value =
                    newStatus;


                command.Parameters.Add(
                    "@OrderID",
                    SqlDbType.Int).Value =
                    orderId;


                int affectedRows =
                    command.ExecuteNonQuery();


                if (affectedRows > 0)
                {
                    MessageBox.Show(
                        "تم تحديث حالة الطلبية بنجاح.",
                        "تم الحفظ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadOrders(currentStatus);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحديث حالة الطلبية:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}