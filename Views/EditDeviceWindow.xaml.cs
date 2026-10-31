using System;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class EditDeviceWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int deviceId;

        public EditDeviceWindow(int id)
        {
            InitializeComponent();

            deviceId = id;

            LoadDevice();
        }

        private void LoadDevice()
        {
            try
            {
                using (SqlConnection con =
                       new SqlConnection(connectionString))
                using (SqlCommand cmd =
                       new SqlCommand(
                           @"SELECT
                               DeviceName,
                               SerialNumber,
                               CustomerName,
                               CustomerPhone,
                               DeviceType,
                               DeviceStatus,
                               RAM,
                               Storage,
                               Accessories,
                               PurchasePrice,
                               SellingPrice,
                               Notes
                             FROM dbo.Devices
                             WHERE DeviceID = @DeviceID
                               AND ISNULL(IsDeleted, 0) = 0",
                           con))
                {
                    cmd.Parameters.AddWithValue("@DeviceID", deviceId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show(
                                "الجهاز غير موجود.",
                                "تنبيه",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            Close();
                            return;
                        }

                        DeviceNameTextBox.Text =
                            reader["DeviceName"]?.ToString() ?? "";

                        SerialNumberTextBox.Text =
                            reader["SerialNumber"] == DBNull.Value
                                ? ""
                                : reader["SerialNumber"].ToString();

                        CustomerNameTextBox.Text =
                            reader["CustomerName"] == DBNull.Value
                                ? ""
                                : reader["CustomerName"].ToString();

                        CustomerPhoneTextBox.Text =
                            reader["CustomerPhone"] == DBNull.Value
                                ? ""
                                : reader["CustomerPhone"].ToString();

                        RAMTextBox.Text =
                            reader["RAM"] == DBNull.Value
                                ? ""
                                : reader["RAM"].ToString();

                        StorageTextBox.Text =
                            reader["Storage"] == DBNull.Value
                                ? ""
                                : reader["Storage"].ToString();

                        AccessoriesTextBox.Text =
                            reader["Accessories"] == DBNull.Value
                                ? ""
                                : reader["Accessories"].ToString();

                        NotesTextBox.Text =
                            reader["Notes"] == DBNull.Value
                                ? ""
                                : reader["Notes"].ToString();

                        PurchasePriceTextBox.Text =
                            reader["PurchasePrice"] == DBNull.Value
                                ? ""
                                : reader["PurchasePrice"].ToString();

                        SellingPriceTextBox.Text =
                            reader["SellingPrice"] == DBNull.Value
                                ? ""
                                : reader["SellingPrice"].ToString();

                        SelectComboBoxItem(
                            DeviceTypeComboBox,
                            reader["DeviceType"]?.ToString());

                        SelectComboBoxItem(
                            DeviceStatusComboBox,
                            reader["DeviceStatus"]?.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات الجهاز:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SelectComboBoxItem(
            ComboBox comboBox,
            string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string deviceName =
                    DeviceNameTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    MessageBox.Show(
                        "من فضلك أدخل اسم الجهاز.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    DeviceNameTextBox.Focus();
                    return;
                }

                decimal purchasePrice = 0;
                decimal sellingPrice = 0;

                if (!string.IsNullOrWhiteSpace(
                        PurchasePriceTextBox.Text))
                {
                    if (!decimal.TryParse(
                            PurchasePriceTextBox.Text.Trim(),
                            out purchasePrice))
                    {
                        MessageBox.Show(
                            "سعر الشراء غير صحيح.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(
                        SellingPriceTextBox.Text))
                {
                    if (!decimal.TryParse(
                            SellingPriceTextBox.Text.Trim(),
                            out sellingPrice))
                    {
                        MessageBox.Show(
                            "سعر البيع غير صحيح.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }

                string deviceType = "جديد";

                if (DeviceTypeComboBox.SelectedItem
                    is ComboBoxItem typeItem)
                {
                    deviceType =
                        typeItem.Content?.ToString() ?? "جديد";
                }

                string deviceStatus = "متاح";

                if (DeviceStatusComboBox.SelectedItem
                    is ComboBoxItem statusItem)
                {
                    deviceStatus =
                        statusItem.Content?.ToString() ?? "متاح";
                }

                using (SqlConnection con =
                       new SqlConnection(connectionString))
                using (SqlCommand cmd =
                       new SqlCommand(
                           @"UPDATE dbo.Devices
                             SET
                                 DeviceName = @DeviceName,
                                 SerialNumber = @SerialNumber,
                                 CustomerName = @CustomerName,
                                 CustomerPhone = @CustomerPhone,
                                 DeviceType = @DeviceType,
                                 DeviceStatus = @DeviceStatus,
                                 RAM = @RAM,
                                 Storage = @Storage,
                                 Accessories = @Accessories,
                                 PurchasePrice = @PurchasePrice,
                                 SellingPrice = @SellingPrice,
                                 Notes = @Notes,
                                 UpdatedAt = GETDATE()
                             WHERE DeviceID = @DeviceID
                               AND ISNULL(IsDeleted, 0) = 0",
                           con))
                {
                    cmd.Parameters.AddWithValue(
                        "@DeviceID", deviceId);

                    cmd.Parameters.AddWithValue(
                        "@DeviceName", deviceName);

                    cmd.Parameters.AddWithValue(
                        "@SerialNumber",
                        string.IsNullOrWhiteSpace(
                            SerialNumberTextBox.Text)
                            ? (object)DBNull.Value
                            : SerialNumberTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@CustomerName",
                        string.IsNullOrWhiteSpace(
                            CustomerNameTextBox.Text)
                            ? (object)DBNull.Value
                            : CustomerNameTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@CustomerPhone",
                        string.IsNullOrWhiteSpace(
                            CustomerPhoneTextBox.Text)
                            ? (object)DBNull.Value
                            : CustomerPhoneTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@DeviceType", deviceType);

                    cmd.Parameters.AddWithValue(
                        "@DeviceStatus", deviceStatus);

                    cmd.Parameters.AddWithValue(
                        "@RAM",
                        string.IsNullOrWhiteSpace(
                            RAMTextBox.Text)
                            ? (object)DBNull.Value
                            : RAMTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@Storage",
                        string.IsNullOrWhiteSpace(
                            StorageTextBox.Text)
                            ? (object)DBNull.Value
                            : StorageTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@Accessories",
                        string.IsNullOrWhiteSpace(
                            AccessoriesTextBox.Text)
                            ? (object)DBNull.Value
                            : AccessoriesTextBox.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@PurchasePrice",
                        purchasePrice);

                    cmd.Parameters.AddWithValue(
                        "@SellingPrice",
                        sellingPrice);

                    cmd.Parameters.AddWithValue(
                        "@Notes",
                        string.IsNullOrWhiteSpace(
                            NotesTextBox.Text)
                            ? (object)DBNull.Value
                            : NotesTextBox.Text.Trim());

                    con.Open();

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "تم تعديل بيانات الجهاز بنجاح.",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تعديل الجهاز:\n\n" +
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