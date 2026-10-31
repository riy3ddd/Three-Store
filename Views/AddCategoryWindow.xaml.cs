using System;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class AddCategoryWindow : Window
    {
        private int? categoryId = null;


        // =========================================================
        // Constructor - إضافة تصنيف جديد
        // =========================================================

        public AddCategoryWindow()
        {
            InitializeComponent();

            // العدد الافتراضي عند الإضافة
            ProductCountTextBox.Text = "0";
        }


        // =========================================================
        // Constructor - تعديل تصنيف موجود
        // =========================================================

        public AddCategoryWindow(
            int categoryId,
            string categoryName,
            int productCount)
        {
            InitializeComponent();

            this.categoryId = categoryId;

            // تحميل البيانات الحالية
            CategoryNameTextBox.Text =
                categoryName;

            ProductCountTextBox.Text =
                productCount.ToString();

            // تغيير شكل النافذة إلى وضع التعديل
            Title = "تعديل تصنيف";

            WindowTitleTextBlock.Text =
                "تعديل التصنيف";

            InfoTextBlock.Text =
                "يمكنك تعديل اسم التصنيف أو عدد المنتجات";

            SaveCategoryButton.Content =
                "حفظ التعديل";
        }


        // =========================================================
        // زر الحفظ
        // =========================================================

        private void SaveCategoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // -----------------------------------------------------
            // اسم التصنيف
            // -----------------------------------------------------

            string categoryName =
                CategoryNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show(
                    "من فضلك أدخل اسم التصنيف.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CategoryNameTextBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // عدد المنتجات
            // -----------------------------------------------------

            if (!int.TryParse(
                    ProductCountTextBox.Text.Trim(),
                    out int productCount))
            {
                MessageBox.Show(
                    "عدد المنتجات يجب أن يكون رقمًا صحيحًا.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ProductCountTextBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // منع الرقم السالب
            // -----------------------------------------------------

            if (productCount < 0)
            {
                MessageBox.Show(
                    "عدد المنتجات لا يمكن أن يكون رقمًا سالبًا.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ProductCountTextBox.Focus();

                return;
            }


            try
            {
                // =================================================
                // حالة التعديل
                // =================================================

                if (categoryId.HasValue)
                {
                    DatabaseHelper.UpdateCategory(
                        categoryId.Value,
                        categoryName,
                        productCount);

                    MessageBox.Show(
                        "تم تعديل التصنيف وعدد المنتجات بنجاح.",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }


                // =================================================
                // حالة الإضافة
                // =================================================

                else
                {
                    DatabaseHelper.AddCategory(
                        categoryName,
                        productCount);

                    MessageBox.Show(
                        "تم إضافة التصنيف والمنتجات بنجاح.",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }


                // إغلاق النافذة وإبلاغ النافذة السابقة بنجاح العملية
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ التصنيف:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // زر إلغاء
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}

