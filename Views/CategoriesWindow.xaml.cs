using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class CategoriesWindow : Window
    {
        private const string SearchPlaceholder =
            "🔍 ابحث عن تصنيف...";


        // =========================================================
        // Constructor
        // =========================================================

        public CategoriesWindow()
        {
            InitializeComponent();
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show(
                    "ليس لديك صلاحية للوصول إلى إدارة التصنيفات.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            SearchTextBox.Text =
                SearchPlaceholder;

            LoadCategories();
        }
        // =========================================================
        // تحميل التصنيفات
        // =========================================================

        private void LoadCategories()
        {
            try
            {
                DataTable table =
                    DatabaseHelper.GetCategories();


                // -------------------------------------------------
                // رقم عرض فقط
                // لا يغير CategoryID الحقيقي
                // -------------------------------------------------

                table.Columns.Add(
                    "DisplayNumber",
                    typeof(int));

                int number = 1;

                foreach (DataRow row in table.Rows)
                {
                    row["DisplayNumber"] =
                        number++;

                }


                // -------------------------------------------------
                // عرض البيانات
                // ProductCount جاي من Products
                // -------------------------------------------------

                CategoriesDataGrid.ItemsSource =
                    table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل التصنيفات:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // البحث - عند الضغط داخل مربع البحث
        // =========================================================

        private void SearchTextBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (SearchTextBox.Text ==
                SearchPlaceholder)
            {
                SearchTextBox.Text = "";
            }
        }


        // =========================================================
        // البحث - عند الخروج من مربع البحث
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
            }
        }


        // =========================================================
        // البحث
        // =========================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (CategoriesDataGrid == null)
                return;

            if (SearchTextBox.Text ==
                SearchPlaceholder)
                return;


            string searchText =
                SearchTextBox.Text.Trim();


            if (CategoriesDataGrid.ItemsSource
                is DataView view)
            {
                if (string.IsNullOrWhiteSpace(
                        searchText))
                {
                    view.RowFilter = "";
                }
                else
                {
                    searchText =
                        searchText.Replace(
                            "'",
                            "''");

                    view.RowFilter =
                        $"CategoryName LIKE '%{searchText}%'";
                }
            }
        }


        // =========================================================
        // إضافة تصنيف
        // =========================================================

        private void AddCategoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddCategoryWindow addCategoryWindow =
                new AddCategoryWindow();

            addCategoryWindow.Owner =
                this;


            bool? result =
                addCategoryWindow.ShowDialog();


            if (result == true)
            {
                LoadCategories();
            }
        }


        // =========================================================
        // تعديل تصنيف
        // =========================================================

        private void EditCategoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;


            if (button.DataContext
                is not DataRowView row)
                return;


            // -------------------------------------------------
            // البيانات الحالية
            // -------------------------------------------------

            int categoryId =
                Convert.ToInt32(
                    row["CategoryID"]);


            string categoryName =
                row["CategoryName"]
                    .ToString() ?? "";


            int productCount =
                Convert.ToInt32(
                    row["ProductCount"]);


            // -------------------------------------------------
            // فتح نافذة التعديل
            // -------------------------------------------------

            AddCategoryWindow editWindow =
                new AddCategoryWindow(
                    categoryId,
                    categoryName,
                    productCount);


            editWindow.Owner =
                this;


            bool? result =
                editWindow.ShowDialog();


            if (result == true)
            {
                LoadCategories();
            }
        }


        // =========================================================
        // حذف تصنيف
        // =========================================================

        private void DeleteCategoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;


            if (button.DataContext
                is not DataRowView row)
                return;


            int categoryId =
                Convert.ToInt32(
                    row["CategoryID"]);


            string categoryName =
                row["CategoryName"]
                    .ToString() ?? "";


            // -------------------------------------------------
            // عدد المنتجات الحقيقي من Products
            // -------------------------------------------------

            int productCount =
                Convert.ToInt32(
                    row["ProductCount"]);


            // -------------------------------------------------
            // منع حذف التصنيف لو عليه منتجات
            // -------------------------------------------------

            if (productCount > 0)
            {
                MessageBox.Show(
                    $"لا يمكن حذف التصنيف \"{categoryName}\" لأنه يحتوي على {productCount} منتج.",
                    "لا يمكن الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            // -------------------------------------------------
            // تأكيد الحذف
            // -------------------------------------------------

            MessageBoxResult result =
                MessageBox.Show(
                    $"هل أنت متأكد من حذف التصنيف:\n\n{categoryName} ؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);


            if (result !=
                MessageBoxResult.Yes)
                return;


            try
            {
                DatabaseHelper.DeleteCategory(
                    categoryId);


                LoadCategories();


                MessageBox.Show(
                    "تم حذف التصنيف بنجاح.",
                    "تم الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف التصنيف:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}