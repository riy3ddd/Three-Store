using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class LowStockWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // =========================================
        // LOW STOCK RULE
        // =========================================
        // أي منتج كميته أقل من 5 يعتبر منخفض المخزون.
        //
        // 0 = منخفض
        // 1 = منخفض
        // 2 = منخفض
        // 3 = منخفض
        // 4 = منخفض
        // 5 = طبيعي
        private const int LOW_STOCK_LIMIT = 5;


        // =========================================
        // CONSTRUCTOR
        // =========================================

        public LowStockWindow()
        {
            InitializeComponent();

            RuleTextBlock.Text =
                $"الحد: أقل من {LOW_STOCK_LIMIT}";

            Loaded += LowStockWindow_Loaded;
        }


        // =========================================
        // WINDOW LOADED
        // =========================================

        private void LowStockWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadLowStockProducts();
        }


        // =========================================
        // LOAD LOW STOCK PRODUCTS
        // =========================================

        private void LoadLowStockProducts()
        {
            try
            {
                using (SqlConnection connection =
                       new SqlConnection(ConnectionString))
                {
                    connection.Open();


                    // =========================================
                    // SQL QUERY
                    // =========================================

                    string query = @"
                        SELECT
                            ROW_NUMBER() OVER (
                                ORDER BY
                                    P.Quantity ASC,
                                    P.ProductName ASC
                            ) AS Number,

                            P.ProductID,

                            P.ProductName,

                            ISNULL(
                                C.CategoryName,
                                N'بدون تصنيف'
                            ) AS CategoryName,

                            P.Quantity

                        FROM Products AS P

                        LEFT JOIN Categories AS C
                            ON P.CategoryID = C.CategoryID

                        WHERE P.Quantity < @LowStockLimit

                        ORDER BY
                            P.Quantity ASC,
                            P.ProductName ASC;
                    ";


                    using (SqlCommand command =
                           new SqlCommand(
                               query,
                               connection))
                    {
                        command.Parameters.Add(
                            "@LowStockLimit",
                            System.Data.SqlDbType.Int);

                        command.Parameters[
                            "@LowStockLimit"].Value =
                            LOW_STOCK_LIMIT;


                        // =========================================
                        // GET DATA
                        // =========================================

                        using (SqlDataAdapter adapter =
                               new SqlDataAdapter(command))
                        {
                            DataTable table =
                                new DataTable();

                            adapter.Fill(table);


                            // =========================================
                            // IMPORTANT
                            // =========================================
                            // نفصل أي ItemsSource قديم
                            // ثم نمسح الـ Items
                            // وبعدها نضع ItemsSource الجديد.
                            // =========================================

                            LowStockDataGrid.ItemsSource = null;

                            LowStockDataGrid.Items.Clear();


                            // =========================================
                            // SET NEW DATA
                            // =========================================

                            LowStockDataGrid.ItemsSource =
                                table.DefaultView;


                            // =========================================
                            // UPDATE COUNT
                            // =========================================

                            LowStockCountTextBlock.Text =
                                table.Rows.Count.ToString();


                            // =========================================
                            // IF NO PRODUCTS
                            // =========================================

                            if (table.Rows.Count == 0)
                            {
                                MessageBox.Show(
                                    "لا يوجد حاليًا أي منتج كميته أقل من 5.",
                                    "المخزون المنخفض",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المنتجات منخفضة المخزون:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================
        // REFRESH BUTTON
        // =========================================

        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadLowStockProducts();
        }
    }
}