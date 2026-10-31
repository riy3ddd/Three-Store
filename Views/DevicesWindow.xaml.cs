using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Views
{
    public partial class DevicesWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public DevicesWindow()
        {
            InitializeComponent();

            LoadDevices();
            LoadDeviceRevenue();
        }

        private void LoadDevices()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.GetDevices", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Filter",
                        GetCurrentFilter());

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        SearchTextBox?.Text?.Trim() ?? "");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    DevicesDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الأجهزة:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetCurrentFilter()
        {
            string type = "الكل";
            string status = "الكل";

            if (DeviceTypeComboBox?.SelectedItem is ComboBoxItem typeItem)
            {
                type = typeItem.Content?.ToString() ?? "الكل";
            }

            if (DeviceStatusComboBox?.SelectedItem is ComboBoxItem statusItem)
            {
                status = statusItem.Content?.ToString() ?? "الكل";
            }

            if (type != "الكل")
                return type;

            if (status != "الكل")
                return status;

            return "الكل";
        }
        private void LoadDeviceRevenue()
        {
            try
            {
                using (SqlConnection con =
                       new SqlConnection(connectionString))
                using (SqlCommand cmd =
                       new SqlCommand(
                           @"
                    SELECT
                        ISNULL(
                            (
                                SELECT SUM(ds.Total)
                                FROM dbo.DeviceSales ds
                            ),
                            0
                        )
                        +
                        ISNULL(
                            (
                                SELECT SUM(ct.Amount)
                                FROM dbo.CreditTransactions ct
                                WHERE
                                    ct.TransactionType = N'Payment'
                                    AND ct.DeviceSaleID IS NOT NULL
                            ),
                            0
                        );
                   ",
                           con))
                {
                    con.Open();

                    object result =
                        cmd.ExecuteScalar();

                    decimal revenue = 0;

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        decimal.TryParse(
                            result.ToString(),
                            out revenue);
                    }

                    if (TotalDeviceRevenueText != null)
                    {
                        TotalDeviceRevenueText.Text =
                            revenue.ToString("N2") +
                            " ج.م";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حساب إجمالي إيرادات الأجهزة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void AddDeviceButton_Click(
       object sender,
       RoutedEventArgs e)
        {
            AddDeviceWindow addDeviceWindow = new AddDeviceWindow();

            bool? result = addDeviceWindow.ShowDialog();

            if (result == true)
            {
                LoadDevices();
                LoadDeviceRevenue();
            }
        }
        private int? GetSelectedDeviceId()
        {
            if (DevicesDataGrid.SelectedItem is DataRowView row)
            {
                if (row["DeviceID"] != DBNull.Value)
                    return Convert.ToInt32(row["DeviceID"]);
            }

            return null;
        }

        private void EditDeviceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? deviceId = GetSelectedDeviceId();

            if (deviceId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر جهاز أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            EditDeviceWindow editWindow =
                new EditDeviceWindow(deviceId.Value);

            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                LoadDevices();
                LoadDeviceRevenue();
            }
        }

        private void DeleteDeviceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? deviceId = GetSelectedDeviceId();

            if (deviceId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر جهاز أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "هل أنت متأكد من حذف هذا الجهاز؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE dbo.Devices
              SET
                  IsDeleted = 1,
                  DeletedAt = GETDATE(),
                  DeletedBy = NULL,
                  UpdatedAt = GETDATE()
              WHERE DeviceID = @DeviceID
                AND ISNULL(IsDeleted, 0) = 0",
                    con))
                {
                    cmd.Parameters.AddWithValue("@DeviceID", deviceId.Value);

                    con.Open();

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        MessageBox.Show(
                            "الجهاز غير موجود أو تم حذفه بالفعل.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }

                MessageBox.Show(
                    "تم حذف الجهاز بنجاح.",
                    "تم الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadDevices();
                LoadDeviceRevenue();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف الجهاز:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SellDeviceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? deviceId = GetSelectedDeviceId();

            if (deviceId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر جهاز أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            SellDeviceWindow sellWindow =
                new SellDeviceWindow(deviceId.Value);

            bool? result = sellWindow.ShowDialog();

            if (result == true)
            {
                LoadDevices();
                LoadDeviceRevenue();
            }
        }
        private void DevicesDataGrid_MouseDoubleClick(
    object sender,
    MouseButtonEventArgs e)
        {
            int? deviceId = GetSelectedDeviceId();

            if (deviceId == null)
                return;

            DeviceDetailsWindow detailsWindow =
                new DeviceDetailsWindow(deviceId.Value);

            detailsWindow.ShowDialog();
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            LoadDevices();
        }

        private void Filter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            LoadDevices();
        }


     


    }
}