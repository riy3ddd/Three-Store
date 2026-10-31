using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1.Views
{
    public partial class AddProductWindow : Window
    {
        private int? productId = null;


        // =========================================================
        // Constructor الإضافة
        // =========================================================

        public AddProductWindow()
        {
            InitializeComponent();

            LoadCategories();
        }


        // =========================================================
        // Constructor التعديل
        // =========================================================

        public AddProductWindow(
            int productId,
            string productName,
            int? categoryId,
            int quantity,
            decimal? purchasePrice,
            decimal? sellingPrice)
        {
            InitializeComponent();

            this.productId = productId;

            LoadCategories();


            // -----------------------------------------------------
            // تعبئة البيانات القديمة
            // -----------------------------------------------------

            ProductNameTextBox.Text = productName;

            QuantityTextBox.Text =
                quantity.ToString();


            if (purchasePrice.HasValue)
            {
                PurchasePriceTextBox.Text =
                    purchasePrice.Value.ToString("0.##");
            }


            if (sellingPrice.HasValue)
            {
                SellingPriceTextBox.Text =
                    sellingPrice.Value.ToString("0.##");
            }


            if (categoryId.HasValue)
            {
                CategoryComboBox.SelectedValue =
                    categoryId.Value;
            }


            // -----------------------------------------------------
            // تغيير شكل النافذة إلى وضع التعديل
            // -----------------------------------------------------

            Title = "تعديل منتج";

            WindowTitleTextBlock.Text =
                "تعديل المنتج";

            InfoTextBlock.Text =
                "(يمكنك تعديل أي خانة واحدة أو أكثر)";

            InfoTextBlock.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(22, 163, 74));

            SaveProductButton.Content =
                "حفظ التعديل";
        }


        // =========================================================
        // تحميل الأقسام
        // =========================================================

        private void LoadCategories()
        {
            try
            {
                DataTable categories =
                    DatabaseHelper.GetCategories();

                CategoryComboBox.ItemsSource =
                    categories.DefaultView;

                CategoryComboBox.DisplayMemberPath =
                    "CategoryName";

                CategoryComboBox.SelectedValuePath =
                    "CategoryID";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الأقسام:\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // زر الحفظ
        // =========================================================

        private void SaveProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // =================================================
                // أولاً: حالة التعديل
                // =================================================

                if (productId != null)
                {
                    // ------------------------------------------------
                    // اسم المنتج
                    // ------------------------------------------------

                    string? productName = null;

                    if (!string.IsNullOrWhiteSpace(
                            ProductNameTextBox.Text))
                    {
                        productName =
                            ProductNameTextBox.Text.Trim();
                    }


                    // ------------------------------------------------
                    // القسم
                    // ------------------------------------------------

                    int? categoryId = null;

                    if (CategoryComboBox.SelectedValue != null)
                    {
                        categoryId =
                            Convert.ToInt32(
                                CategoryComboBox.SelectedValue);
                    }


                    // ------------------------------------------------
                    // الكمية
                    // ------------------------------------------------

                    int? quantity = null;

                    if (!string.IsNullOrWhiteSpace(
                            QuantityTextBox.Text))
                    {
                        if (!int.TryParse(
                                QuantityTextBox.Text.Trim(),
                                out int quantityValue))
                        {
                            MessageBox.Show(
                                "الكمية يجب أن تكون رقمًا صحيحًا.",
                                "تنبيه",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        quantity = quantityValue;
                    }


                    // ------------------------------------------------
                    // سعر الشراء
                    // ------------------------------------------------

                    decimal? purchasePrice = null;

                    if (!string.IsNullOrWhiteSpace(
                            PurchasePriceTextBox.Text))
                    {
                        if (!decimal.TryParse(
                                PurchasePriceTextBox.Text.Trim(),
                                out decimal purchaseValue))
                        {
                            MessageBox.Show(
                                "سعر الشراء غير صحيح.",
                                "تنبيه",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        purchasePrice = purchaseValue;
                    }


                    // ------------------------------------------------
                    // سعر البيع
                    // ------------------------------------------------

                    decimal? sellingPrice = null;

                    if (!string.IsNullOrWhiteSpace(
                            SellingPriceTextBox.Text))
                    {
                        if (!decimal.TryParse(
                                SellingPriceTextBox.Text.Trim(),
                                out decimal sellingValue))
                        {
                            MessageBox.Show(
                                "سعر البيع غير صحيح.",
                                "تنبيه",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        sellingPrice = sellingValue;
                    }


                    // ------------------------------------------------
                    // التأكد إن المستخدم عدل حاجة واحدة على الأقل
                    // ------------------------------------------------

                    if (productName == null &&
                        categoryId == null &&
                        quantity == null &&
                        purchasePrice == null &&
                        sellingPrice == null)
                    {
                        MessageBox.Show(
                            "من فضلك قم بتعديل خانة واحدة على الأقل.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }


                    // ------------------------------------------------
                    // تحديث المنتج في قاعدة البيانات
                    // ------------------------------------------------

                    DatabaseHelper.UpdateProduct(
                        productId.Value,
                        productName,
                        categoryId,
                        quantity,
                        purchasePrice,
                        sellingPrice);


                    // ------------------------------------------------
                    // رسالة النجاح
                    // ------------------------------------------------

                    MessageBox.Show(
                        "تم تعديل المنتج بنجاح!",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                    Close();

                    return;
                }


                // =================================================
                // ثانياً: حالة إضافة منتج جديد
                // =================================================

                string newProductName =
                    ProductNameTextBox.Text.Trim();


                // ------------------------------------------------
                // اسم المنتج إجباري
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                        newProductName))
                {
                    MessageBox.Show(
                        "الحد الأدنى لإضافة المنتج هو:\n\n" +
                        "اسم المنتج + القسم + الكمية",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // ------------------------------------------------
                // القسم إجباري
                // ------------------------------------------------

                if (CategoryComboBox.SelectedValue == null)
                {
                    MessageBox.Show(
                        "الحد الأدنى لإضافة المنتج هو:\n\n" +
                        "اسم المنتج + القسم + الكمية",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // ------------------------------------------------
                // الكمية إجباري
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                        QuantityTextBox.Text))
                {
                    MessageBox.Show(
                        "الحد الأدنى لإضافة المنتج هو:\n\n" +
                        "اسم المنتج + القسم + الكمية",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // ------------------------------------------------
                // الحصول على رقم القسم
                // ------------------------------------------------

                int newCategoryId =
                    Convert.ToInt32(
                        CategoryComboBox.SelectedValue);


                // ------------------------------------------------
                // التحقق من الكمية
                // ------------------------------------------------

                if (!int.TryParse(
                        QuantityTextBox.Text.Trim(),
                        out int newQuantity))
                {
                    MessageBox.Show(
                        "الكمية يجب أن تكون رقمًا صحيحًا.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // ------------------------------------------------
                // سعر الشراء اختياري
                // ------------------------------------------------

                decimal? newPurchasePrice = null;

                if (!string.IsNullOrWhiteSpace(
                        PurchasePriceTextBox.Text))
                {
                    if (!decimal.TryParse(
                            PurchasePriceTextBox.Text.Trim(),
                            out decimal purchaseValue))
                    {
                        MessageBox.Show(
                            "سعر الشراء غير صحيح.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    newPurchasePrice = purchaseValue;
                }


                // ------------------------------------------------
                // سعر البيع اختياري
                // ------------------------------------------------

                decimal? newSellingPrice = null;

                if (!string.IsNullOrWhiteSpace(
                        SellingPriceTextBox.Text))
                {
                    if (!decimal.TryParse(
                            SellingPriceTextBox.Text.Trim(),
                            out decimal sellingValue))
                    {
                        MessageBox.Show(
                            "سعر البيع غير صحيح.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    newSellingPrice = sellingValue;
                }


                // ------------------------------------------------
                // إضافة المنتج إلى قاعدة البيانات
                // ------------------------------------------------

                DatabaseHelper.AddProduct(
                    newProductName,
                    newCategoryId,
                    newQuantity,
                    newPurchasePrice,
                    newSellingPrice);


                // ------------------------------------------------
                // رسالة النجاح
                // ------------------------------------------------

                MessageBox.Show(
                    "تم إضافة المنتج بنجاح!",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ المنتج:\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}