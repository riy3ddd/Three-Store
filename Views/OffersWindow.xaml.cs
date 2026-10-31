using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class OffersWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public OffersWindow()
        {
            InitializeComponent();

            Loaded += OffersWindow_Loaded;
        }

        private void OffersWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadOffers();
        }

        private void LoadOffers()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            o.OfferID,
                            o.ProductID,
                            p.ProductName,
                            o.OriginalPrice,
                            o.OfferPrice,
                            o.DiscountAmount,
                            o.DiscountPercent,
                            o.StartDate,
                            o.EndDate,
                            o.IsActive,

                            CASE
                                WHEN o.IsActive = 1
                                    THEN N'نشط'
                                ELSE N'متوقف'
                            END AS StatusText

                        FROM dbo.Offers o
                        INNER JOIN dbo.Products p
                            ON p.ProductID = o.ProductID

                        ORDER BY
                            o.IsActive DESC,
                            o.StartDate DESC,
                            o.OfferID DESC;
                        ",
                        connection);

                DataTable table =
                    new DataTable();

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);

                adapter.Fill(table);

                OffersDataGrid.ItemsSource =
                    table.DefaultView;

                UpdateSummary(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل العروض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateSummary(DataTable table)
        {
            if (table == null)
                return;

            int activeCount = 0;

            decimal totalDiscount = 0m;

            System.Collections.Generic.HashSet<int>
                productIds =
                new System.Collections.Generic.HashSet<int>();

            foreach (DataRow row in table.Rows)
            {
                if (row["IsActive"] != DBNull.Value &&
                    Convert.ToBoolean(row["IsActive"]))
                {
                    activeCount++;

                    if (row["DiscountAmount"] != DBNull.Value)
                    {
                        totalDiscount +=
                            Convert.ToDecimal(
                                row["DiscountAmount"]);
                    }

                    if (row["ProductID"] != DBNull.Value)
                    {
                        productIds.Add(
                            Convert.ToInt32(
                                row["ProductID"]));
                    }
                }
            }

            ActiveOffersTextBlock.Text =
                activeCount.ToString();

            TotalDiscountTextBlock.Text =
                $"{totalDiscount:N2} ج.م";

            ProductsWithOffersTextBlock.Text =
                productIds.Count.ToString();
        }


        private void AddOfferButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddOfferWindow window =
                new AddOfferWindow();

            window.Owner = this;

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadOffers();
            }
        }

        private void EditOfferButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? offerId =
                GetSelectedOfferId();

            if (offerId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عرضًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            EditOfferWindow window =
                new EditOfferWindow(
                    offerId.Value);

            window.Owner = this;

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                LoadOffers();
            }
        }

        private void DeactivateOfferButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? offerId =
                GetSelectedOfferId();

            if (offerId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عرضًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation =
                MessageBox.Show(
                    "هل أنت متأكد من إيقاف هذا العرض؟",
                    "تأكيد إيقاف العرض",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(
                        @"
                        UPDATE dbo.Offers
                        SET
                            IsActive = 0,
                            UpdatedAt = GETDATE()
                        WHERE OfferID = @OfferID;
                        ",
                        connection);

                command.Parameters.Add(
                    "@OfferID",
                    SqlDbType.Int).Value =
                    offerId.Value;

                connection.Open();

                command.ExecuteNonQuery();

                LoadOffers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء إيقاف العرض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private int? GetSelectedOfferId()
        {
            if (OffersDataGrid.SelectedItem
                is DataRowView row)
            {
                if (row["OfferID"] != DBNull.Value)
                {
                    return Convert.ToInt32(
                        row["OfferID"]);
                }
            }

            return null;
        }
        private void ToggleOfferButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? offerId =
                GetSelectedOfferId();

            if (offerId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عرضًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                string query = @"
            UPDATE Offers
            SET
                IsActive =
                    CASE
                        WHEN IsActive = 1 THEN 0
                        ELSE 1
                    END,
                UpdatedAt = GETDATE()
            WHERE OfferID = @OfferID;";

                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);

                command.Parameters.Add(
                    "@OfferID",
                    SqlDbType.Int).Value =
                    offerId.Value;

                connection.Open();

                command.ExecuteNonQuery();

                LoadOffers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تغيير حالة العرض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void DeleteOfferButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            int? offerId =
                GetSelectedOfferId();

            if (offerId == null)
            {
                MessageBox.Show(
                    "من فضلك اختر عرضًا أولاً.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    "هل أنت متأكد من حذف هذا العرض؟\n\n" +
                    "سيتم حذف سجل العرض نهائيًا.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                string query = @"
            DELETE FROM Offers
            WHERE OfferID = @OfferID;";

                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);

                command.Parameters.Add(
                    "@OfferID",
                    SqlDbType.Int).Value =
                    offerId.Value;

                connection.Open();

                int affectedRows =
                    command.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    MessageBox.Show(
                        "العرض غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                MessageBox.Show(
                    "تم حذف العرض بنجاح.",
                    "تم الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadOffers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف العرض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


    }
}