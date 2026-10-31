using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class AddDeviceWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddDeviceWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string deviceName = DeviceNameTextBox.Text.Trim();

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

                string serialNumber = SerialNumberTextBox.Text.Trim();

                string deviceType = "جديد";

                if (DeviceTypeComboBox.SelectedItem is ComboBoxItem typeItem)
                {
                    deviceType = typeItem.Content?.ToString() ?? "جديد";
                }

                string deviceStatus = "متاح";

                string ram = RAMTextBox.Text.Trim();

                string storage = StorageTextBox.Text.Trim();

                string accessories = AccessoriesTextBox.Text.Trim();

                string customerName = CustomerNameTextBox.Text.Trim();

                string customerPhone = CustomerPhoneTextBox.Text.Trim();

                string notes = NotesTextBox.Text.Trim();

                decimal purchasePrice = 0;

                decimal sellingPrice = 0;

                if (!string.IsNullOrWhiteSpace(PurchasePriceTextBox.Text))
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

                        PurchasePriceTextBox.Focus();
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(SellingPriceTextBox.Text))
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

                        SellingPriceTextBox.Focus();
                        return;
                    }
                }

                using (SqlConnection con =
                       new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd =
                           new SqlCommand(
                               @"INSERT INTO dbo.Devices
                               (
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
                                   Notes,
                                   AddedDate,
                                   IsDeleted
                               )
                               VALUES
                               (
                                   @DeviceName,
                                   @SerialNumber,
                                   @CustomerName,
                                   @CustomerPhone,
                                   @DeviceType,
                                   @DeviceStatus,
                                   @RAM,
                                   @Storage,
                                   @Accessories,
                                   @PurchasePrice,
                                   @SellingPrice,
                                   @Notes,
                                   GETDATE(),
                                   0
                               )",
                               con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@DeviceName",
                            deviceName);

                        cmd.Parameters.AddWithValue(
                            "@SerialNumber",
                            string.IsNullOrWhiteSpace(serialNumber)
                                ? (object)DBNull.Value
                                : serialNumber);

                        cmd.Parameters.AddWithValue(
                            "@CustomerName",
                            string.IsNullOrWhiteSpace(customerName)
                                ? (object)DBNull.Value
                                : customerName);

                        cmd.Parameters.AddWithValue(
                            "@CustomerPhone",
                            string.IsNullOrWhiteSpace(customerPhone)
                                ? (object)DBNull.Value
                                : customerPhone);

                        cmd.Parameters.AddWithValue(
                            "@DeviceType",
                            deviceType);

                        cmd.Parameters.AddWithValue(
                            "@DeviceStatus",
                            deviceStatus);

                        cmd.Parameters.AddWithValue(
                            "@RAM",
                            string.IsNullOrWhiteSpace(ram)
                                ? (object)DBNull.Value
                                : ram);

                        cmd.Parameters.AddWithValue(
                            "@Storage",
                            string.IsNullOrWhiteSpace(storage)
                                ? (object)DBNull.Value
                                : storage);

                        cmd.Parameters.AddWithValue(
                            "@Accessories",
                            string.IsNullOrWhiteSpace(accessories)
                                ? (object)DBNull.Value
                                : accessories);

                        cmd.Parameters.AddWithValue(
                            "@PurchasePrice",
                            purchasePrice);

                        cmd.Parameters.AddWithValue(
                            "@SellingPrice",
                            sellingPrice);

                        cmd.Parameters.AddWithValue(
                            "@Notes",
                            string.IsNullOrWhiteSpace(notes)
                                ? (object)DBNull.Value
                                : notes);

                        con.Open();

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "تمت إضافة الجهاز بنجاح.",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ الجهاز:\n\n" + ex.Message,
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