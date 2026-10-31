using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class EditOfferWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int offerId;

        private decimal originalPrice;

        public EditOfferWindow(int id)
        {
            InitializeComponent();

            offerId = id;

            LoadOffer();
        }

        private void LoadOffer()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            o.OriginalPrice,
                            o.OfferPrice,
                            p.ProductName
                        FROM dbo.Offers o
                        INNER JOIN dbo.Products p
                            ON p.ProductID = o.ProductID
                        WHERE o.OfferID = @OfferID;
                        ",
                        connection);

                command.Parameters.Add(
                    "@OfferID",
                    SqlDbType.Int).Value =
                    offerId;

                connection.Open();

                using SqlDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "العرض غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }

                ProductNameTextBox.Text =
                    reader["ProductName"]?.ToString() ?? "";

                originalPrice =
                    Convert.ToDecimal(
                        reader["OriginalPrice"]);

                OriginalPriceTextBox.Text =
                    originalPrice.ToString("N2");

                OfferPriceTextBox.Text =
                    Convert.ToDecimal(
                        reader["OfferPrice"])
                    .ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل العرض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OfferPriceTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (originalPrice <= 0)
                return;

            if (!decimal.TryParse(
                    OfferPriceTextBox.Text.Trim(),
                    out decimal offerPrice))
            {
                DiscountPreviewTextBlock.Text =
                    "الخصم: 0%";

                return;
            }

            if (offerPrice < 0 ||
                offerPrice > originalPrice)
            {
                DiscountPreviewTextBlock.Text =
                    "سعر العرض غير صحيح";

                return;
            }

            decimal discountPercent =
                ((originalPrice - offerPrice)
                / originalPrice) * 100m;

            DiscountPreviewTextBlock.Text =
                $"الخصم: {discountPercent:N1}%";
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(
                        OfferPriceTextBox.Text.Trim(),
                        out decimal offerPrice))
                {
                    MessageBox.Show(
                        "من فضلك أدخل سعر عرض صحيح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (offerPrice < 0 ||
                    offerPrice >= originalPrice)
                {
                    MessageBox.Show(
                        "سعر العرض يجب أن يكون أقل من السعر الأصلي.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(
                        @"
                        UPDATE dbo.Offers
                        SET
                            OfferPrice = @OfferPrice,
                            UpdatedAt = GETDATE()
                        WHERE OfferID = @OfferID
                          AND IsActive = 1;
                        ",
                        connection);

                command.Parameters.Add(
                    "@OfferPrice",
                    SqlDbType.Decimal).Value =
                    offerPrice;

                command.Parameters[
                    "@OfferPrice"]
                    .Precision = 18;

                command.Parameters[
                    "@OfferPrice"]
                    .Scale = 2;

                command.Parameters.Add(
                    "@OfferID",
                    SqlDbType.Int).Value =
                    offerId;

                connection.Open();

                command.ExecuteNonQuery();

                MessageBox.Show(
                    "تم تعديل العرض بنجاح.",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تعديل العرض:\n\n" +
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