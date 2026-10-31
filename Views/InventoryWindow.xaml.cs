using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1.Views
{
    public partial class InventoryWindow : Window
    {
        private const string SearchPlaceholder =
            "🔍 ابحث عن منتج بالاسم...";


        // =========================================================
        // Constructor
        // =========================================================

        public InventoryWindow()
        {
            InitializeComponent();

            // إعداد الـ Placeholder
            SearchTextBox.Text = SearchPlaceholder;

            SearchTextBox.Foreground =
                new SolidColorBrush(Colors.Gray);

            try
            {
                LoadInventory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "خطأ أثناء فتح جرد المنتجات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تحميل الجرد
        // =========================================================

        private void LoadInventory()
        {
            DataTable products =
                DatabaseHelper.GetProducts();


            // إنشاء أعمدة الحسابات الخاصة بالجرد
            if (!products.Columns.Contains("TotalPurchase"))
            {
                products.Columns.Add(
                    "TotalPurchase",
                    typeof(decimal));
            }


            if (!products.Columns.Contains("TotalSelling"))
            {
                products.Columns.Add(
                    "TotalSelling",
                    typeof(decimal));
            }


            decimal totalSellingValue = 0;


            // =====================================================
            // حساب إجمالي الشراء والبيع لكل منتج
            // =====================================================

            foreach (DataRow row in products.Rows)
            {
                decimal purchasePrice = 0;

                decimal sellingPrice = 0;

                int quantity = 0;


                // الكمية
                if (row["Quantity"] != DBNull.Value)
                {
                    quantity =
                        Convert.ToInt32(row["Quantity"]);
                }


                // سعر الشراء
                if (row["PurchasePrice"] != DBNull.Value)
                {
                    purchasePrice =
                        Convert.ToDecimal(row["PurchasePrice"]);
                }


                // سعر البيع
                if (row["SellingPrice"] != DBNull.Value)
                {
                    sellingPrice =
                        Convert.ToDecimal(row["SellingPrice"]);
                }


                // إجمالي شراء المنتج
                decimal totalPurchase =
                    quantity * purchasePrice;


                // إجمالي بيع المنتج
                decimal totalSelling =
                    quantity * sellingPrice;


                row["TotalPurchase"] =
                    totalPurchase;


                row["TotalSelling"] =
                    totalSelling;


                // تجميع إجمالي قيمة البيع
                totalSellingValue +=
                    totalSelling;
            }


            // =====================================================
            // عرض إجمالي قيمة البيع أعلى الصفحة
            // =====================================================

            TotalSellingValueTextBlock.Text =
                $"{totalSellingValue:N2} ج.م";


            // =====================================================
            // عرض البيانات
            // =====================================================

            InventoryDataGrid.ItemsSource =
                products.DefaultView;
        }


        // =========================================================
        // ترقيم الصفوف
        // =========================================================

        private void InventoryDataGrid_LoadingRow(
            object sender,
            DataGridRowEventArgs e)
        {
            e.Row.Header =
                e.Row.GetIndex() + 1;
        }


        // =========================================================
        // البحث
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            // لو الـ Placeholder ظاهر
            if (SearchTextBox.Text ==
                SearchPlaceholder)
            {
                return;
            }


            string searchText =
                SearchTextBox.Text.Trim();


            if (InventoryDataGrid.ItemsSource
                is DataView view)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    view.RowFilter = "";
                }
                else
                {
                    view.RowFilter =
                        $"ProductName LIKE '%{searchText.Replace("'", "''")}%'";
                }
            }
        }


        // =========================================================
        // عند الضغط على البحث
        // =========================================================

        private void SearchTextBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (SearchTextBox.Text ==
                SearchPlaceholder)
            {
                SearchTextBox.Text = "";

                SearchTextBox.Foreground =
                    new SolidColorBrush(
                        Colors.Black);
            }
        }


        // =========================================================
        // عند الخروج من البحث
        // =========================================================

        private void SearchTextBox_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    SearchTextBox.Text))
            {
                SearchTextBox.Text =
                    SearchPlaceholder;

                SearchTextBox.Foreground =
                    new SolidColorBrush(
                        Colors.Gray);
            }
        }
    }
}
