using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp1.Views
{
    public partial class ProductsWindow : Window
    {
        private const string SearchPlaceholder =
            "🔍 ابحث عن منتج بالاسم...";


        // =========================================================
        // Constructor
        // =========================================================

        public ProductsWindow()
        {
            InitializeComponent();
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show(
                    "ليس لديك صلاحية للوصول إلى إدارة المنتجات.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

           
        // إعداد شكل الـ Placeholder
        SearchTextBox.Text = SearchPlaceholder;

            SearchTextBox.Foreground =
                new SolidColorBrush(
                    Colors.Gray);

            try
            {
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "خطأ أثناء فتح نافذة المنتجات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تحميل المنتجات
        // =========================================================

        private void LoadProducts()
        {
            ProductsDataGrid.ItemsSource =
                DatabaseHelper.GetProducts().DefaultView;
        }

        private void ProductsDataGrid_LoadingRow(
            object sender,
            DataGridRowEventArgs e)
        {
            e.Row.Header =
                e.Row.GetIndex() + 1;
        }


        // =========================================================
        // تحديث Dashboard
        // =========================================================

        private void RefreshMainWindow()
        {
            if (Owner is WpfApp1.MainWindow mainWindow)
            {
                mainWindow.LoadDashboardData();
            }
        }


        // =========================================================
        // زر إضافة منتج
        // =========================================================

        private void Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddProductWindow addProductWindow =
                new AddProductWindow();

            // نخلي ProductsWindow هي الـ Owner
            addProductWindow.Owner = this;

            addProductWindow.ShowDialog();


            // تحديث جدول المنتجات
            LoadProducts();


            // تحديث Dashboard فورًا
            RefreshMainWindow();
        }


        // =========================================================
        // البحث
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            // لو الـ Placeholder ظاهر، ما نعملش بحث
            if (SearchTextBox.Text == SearchPlaceholder)
            {
                return;
            }

            string searchText =
                SearchTextBox.Text.Trim();


            if (ProductsDataGrid.ItemsSource
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
        // عند الضغط على مربع البحث
        // =========================================================

        private void SearchTextBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (SearchTextBox.Text == SearchPlaceholder)
            {
                SearchTextBox.Text = "";

                SearchTextBox.Foreground =
                    new SolidColorBrush(
                        Colors.Black);
            }
        }


        // =========================================================
        // عند الخروج من مربع البحث
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


        // =========================================================
        // حذف منتج
        // =========================================================

        private void DeleteProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Button button =
                    (Button)sender;


                DataRowView row =
                    (DataRowView)button.DataContext;


                int productId =
                    Convert.ToInt32(
                        row["ProductID"]);


                string productName =
                    row["ProductName"]?.ToString() ?? "";


                MessageBoxResult result =
                    MessageBox.Show(
                        $"هل أنت متأكد من حذف المنتج:\n\n{productName}؟",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);


                if (result != MessageBoxResult.Yes)
                    return;


                // حذف المنتج من قاعدة البيانات
                DatabaseHelper.DeleteProduct(
                    productId);


                // تحديث جدول المنتجات
                LoadProducts();


                // تحديث Dashboard فورًا
                RefreshMainWindow();


                MessageBox.Show(
                    "تم حذف المنتج بنجاح.",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "خطأ أثناء حذف المنتج",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تعديل منتج
        // =========================================================

        private void EditProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Button button =
                    (Button)sender;


                DataRowView row =
                    (DataRowView)button.DataContext;


                int productId =
                    Convert.ToInt32(
                        row["ProductID"]);


                string productName =
                    row["ProductName"]?.ToString() ?? "";


                int? categoryId = null;

                if (row["CategoryID"] != DBNull.Value)
                {
                    categoryId =
                        Convert.ToInt32(
                            row["CategoryID"]);
                }


                int quantity =
                    Convert.ToInt32(
                        row["Quantity"]);


                decimal? purchasePrice = null;

                if (row["PurchasePrice"] != DBNull.Value)
                {
                    purchasePrice =
                        Convert.ToDecimal(
                            row["PurchasePrice"]);
                }


                decimal? sellingPrice = null;

                if (row["SellingPrice"] != DBNull.Value)
                {
                    sellingPrice =
                        Convert.ToDecimal(
                            row["SellingPrice"]);
                }


                AddProductWindow editWindow =
                    new AddProductWindow(
                        productId,
                        productName,
                        categoryId,
                        quantity,
                        purchasePrice,
                        sellingPrice);


                // نخلي ProductsWindow هي الـ Owner
                editWindow.Owner = this;

                editWindow.ShowDialog();


                // تحديث جدول المنتجات
                LoadProducts();


                // تحديث Dashboard فورًا
                RefreshMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح تعديل المنتج:\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}