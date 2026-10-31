using System;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class DeviceDetailsWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int deviceId;

        public DeviceDetailsWindow(int id)
        {
            InitializeComponent();

            deviceId = id;

            LoadDeviceDetails();
        }

        private void LoadDeviceDetails()
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
                                DeviceType,
                                DeviceStatus,
                                RAM,
                                Storage,
                                Accessories,
                                CustomerName,
                                CustomerPhone,
                                PurchasePrice,
                                SellingPrice,
                                AddedDate,
                                SoldDate,
                                Notes
                             FROM dbo.Devices
                             WHERE DeviceID = @DeviceID
                               AND ISNULL(IsDeleted, 0) = 0",
                           con))
                {
                    cmd.Parameters.AddWithValue(
                        "@DeviceID",
                        deviceId);

                    con.Open();

                    using (SqlDataReader reader =
                           cmd.ExecuteReader())
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

                        DeviceNameText.Text =
                            GetValue(reader, "DeviceName");

                        SerialNumberText.Text =
                            GetValue(reader, "SerialNumber");

                        DeviceTypeText.Text =
                            GetValue(reader, "DeviceType");

                        DeviceStatusText.Text =
                            GetValue(reader, "DeviceStatus");

                        RAMText.Text =
                            GetValue(reader, "RAM");

                        StorageText.Text =
                            GetValue(reader, "Storage");

                        AccessoriesText.Text =
                            GetValue(reader, "Accessories");

                        CustomerNameText.Text =
                            GetValue(reader, "CustomerName");

                        CustomerPhoneText.Text =
                            GetValue(reader, "CustomerPhone");

                        PurchasePriceText.Text =
                            FormatMoney(reader, "PurchasePrice");

                        SellingPriceText.Text =
                            FormatMoney(reader, "SellingPrice");

                        AddedDateText.Text =
                            FormatDate(reader, "AddedDate");

                        SoldDateText.Text =
                            FormatDate(reader, "SoldDate");

                        NotesText.Text =
                            GetValue(reader, "Notes");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل تفاصيل الجهاز:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetValue(
            SqlDataReader reader,
            string column)
        {
            if (reader[column] == DBNull.Value)
                return "غير متوفر";

            string value =
                reader[column].ToString();

            return string.IsNullOrWhiteSpace(value)
                ? "غير متوفر"
                : value;
        }

        private string FormatMoney(
            SqlDataReader reader,
            string column)
        {
            if (reader[column] == DBNull.Value)
                return "غير متوفر";

            decimal value =
                Convert.ToDecimal(reader[column]);

            return value.ToString("N2") + " ج.م";
        }

        private string FormatDate(
            SqlDataReader reader,
            string column)
        {
            if (reader[column] == DBNull.Value)
                return "غير متوفر";

            DateTime value =
                Convert.ToDateTime(reader[column]);

            return value.ToString("dd/MM/yyyy HH:mm");
        }

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}