using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1.Views
{
    public partial class AddOfferWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private DataTable productsTable =
            new DataTable();

        private int selectedProductId;

        private decimal originalPrice;

        public AddOfferWindow()
        {
            InitializeComponent();

            Loaded += AddOfferWindow_Loaded;
        }


        // =========================================================
        // تحميل النافذة
        // =========================================================

        private void AddOfferWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadProducts();

            SearchPlaceholderText.Visibility =
                Visibility.Visible;

            SearchProductTextBox.Foreground =
                new SolidColorBrush(
                    Colors.Black);
        }


        // =========================================================
        // تحميل المنتجات
        // =========================================================

        private void LoadProducts()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    SELECT
                        ProductID,
                        ProductName,
                        Quantity,
                        ISNULL(SellingPrice, 0) AS SellingPrice
                    FROM Products
                    ORDER BY ProductName;";

                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);

                productsTable =
                    new DataTable();

                adapter.Fill(
                    productsTable);

                ProductsDataGrid.ItemsSource =
                    productsTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المنتجات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // البحث باسم المنتج
        // =========================================================

        private void SearchProductTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (productsTable == null)
                return;

            string searchText =
                SearchProductTextBox.Text.Trim();

            // إظهار / إخفاء الـ Placeholder
            SearchPlaceholderText.Visibility =
                string.IsNullOrWhiteSpace(searchText)
                    ? Visibility.Visible
                    : Visibility.Collapsed;


            // مسح البحث
            if (string.IsNullOrWhiteSpace(
                    searchText))
            {
                productsTable.DefaultView.RowFilter =
                    "";

                return;
            }


            // حماية النص من علامات SQL الخاصة
            string safeSearch =
                searchText.Replace(
                    "'",
                    "''");


            // البحث باسم المنتج فقط
            productsTable.DefaultView.RowFilter =
                $"ProductName LIKE '%{safeSearch}%'";
        }


        // =========================================================
        // اختيار المنتج
        // =========================================================

        private void ProductsDataGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem
                is not DataRowView row)
            {
                return;
            }

            try
            {
                selectedProductId =
                    Convert.ToInt32(
                        row["ProductID"]);

                SelectedProductTextBox.Text =
                    row["ProductName"]
                        ?.ToString()
                        ?? "";

                originalPrice =
                    Convert.ToDecimal(
                        row["SellingPrice"]);

                OriginalPriceTextBox.Text =
                    originalPrice.ToString("N2");

                OfferPriceTextBox.Text =
                    "";

                DiscountPreviewTextBlock.Text =
                    "الخصم: 0%";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء اختيار المنتج:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // حساب الخصم أثناء الكتابة
        // =========================================================

        private void OfferPriceTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (originalPrice <= 0)
            {
                DiscountPreviewTextBlock.Text =
                    "الخصم: 0%";

                return;
            }

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


        // =========================================================
        // حفظ العرض
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // -------------------------------------------------
                // التأكد من اختيار منتج
                // -------------------------------------------------

                if (selectedProductId <= 0)
                {
                    MessageBox.Show(
                        "من فضلك اختر منتجًا أولاً.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // -------------------------------------------------
                // التأكد من سعر العرض
                // -------------------------------------------------

                if (!decimal.TryParse(
                        OfferPriceTextBox.Text.Trim(),
                        out decimal offerPrice))
                {
                    MessageBox.Show(
                        "من فضلك أدخل سعر عرض صحيح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    OfferPriceTextBox.Focus();

                    return;
                }


                // -------------------------------------------------
                // التحقق من السعر
                // -------------------------------------------------

                if (offerPrice < 0)
                {
                    MessageBox.Show(
                        "سعر العرض لا يمكن أن يكون أقل من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (offerPrice >= originalPrice)
                {
                    MessageBox.Show(
                        "سعر العرض يجب أن يكون أقل من السعر الحالي.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                connection.Open();


                using SqlTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    // =================================================
                    // إيقاف أي عرض نشط سابق لنفس المنتج
                    // =================================================

                    string deactivateQuery = @"
                        UPDATE Offers
                        SET
                            IsActive = 0,
                            UpdatedAt = GETDATE()
                        WHERE
                            ProductID = @ProductID
                            AND IsActive = 1;";

                    using SqlCommand deactivateCommand =
                        new SqlCommand(
                            deactivateQuery,
                            connection,
                            transaction);

                    deactivateCommand.Parameters.Add(
                        "@ProductID",
                        SqlDbType.Int).Value =
                        selectedProductId;

                    deactivateCommand.ExecuteNonQuery();


                    // =================================================
                    // إضافة العرض الجديد
                    // =================================================

                    string insertQuery = @"
                        INSERT INTO Offers
                        (
                            ProductID,
                            OriginalPrice,
                            OfferPrice,
                            StartDate,
                            IsActive,
                            CreatedAt
                        )
                        VALUES
                        (
                            @ProductID,
                            @OriginalPrice,
                            @OfferPrice,
                            GETDATE(),
                            1,
                            GETDATE()
                        );";

                    using SqlCommand insertCommand =
                        new SqlCommand(
                            insertQuery,
                            connection,
                            transaction);

                    insertCommand.Parameters.Add(
                        "@ProductID",
                        SqlDbType.Int).Value =
                        selectedProductId;


                    SqlParameter originalPriceParameter =
                        insertCommand.Parameters.Add(
                            "@OriginalPrice",
                            SqlDbType.Decimal);

                    originalPriceParameter.Precision =
                        18;

                    originalPriceParameter.Scale =
                        2;

                    originalPriceParameter.Value =
                        originalPrice;


                    SqlParameter offerPriceParameter =
                        insertCommand.Parameters.Add(
                            "@OfferPrice",
                            SqlDbType.Decimal);

                    offerPriceParameter.Precision =
                        18;

                    offerPriceParameter.Scale =
                        2;

                    offerPriceParameter.Value =
                        offerPrice;


                    insertCommand.ExecuteNonQuery();


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }


                // =================================================
                // رسالة نجاح
                // =================================================

                MessageBox.Show(
                    "تمت إضافة المنتج إلى العروض بنجاح.",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ العرض:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إلغاء
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}