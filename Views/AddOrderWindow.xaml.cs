using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class AddOrderWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddOrderWindow()
        {
            InitializeComponent();

            Loaded += AddOrderWindow_Loaded;
        }

        private void AddOrderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        // ============================================
        // تحميل المنتجات الحقيقية من Products
        // ============================================

        private void LoadProducts()
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();

                string query = @"
                    SELECT
                        ProductID,
                        ProductName
                    FROM Products
                    ORDER BY ProductName ASC";

                using SqlCommand command = new SqlCommand(query, connection);

                using SqlDataAdapter adapter = new SqlDataAdapter(command);

                DataTable table = new DataTable();

                adapter.Fill(table);

                ProductComboBox.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل المنتجات:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================
        // اختيار المنتج
        // ============================================

        private void ProductComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ManualProductCheckBox.IsChecked == true)
                return;

            if (ProductComboBox.SelectedItem != null)
            {
                ManualProductTextBox.Text = "";
            }
        }

        // ============================================
        // المنتج اليدوي
        // ============================================

        private void ManualProductCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            ProductComboBox.SelectedIndex = -1;

            ProductComboBox.IsEnabled = false;

            ManualProductTextBox.IsEnabled = true;

            ManualProductTextBox.Focus();
        }

        private void ManualProductCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            ProductComboBox.IsEnabled = true;

            ManualProductTextBox.IsEnabled = false;

            ManualProductTextBox.Text = "";
        }

        // ============================================
        // منع الحروف في الكمية
        // ============================================

        private void QuantityTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // ============================================
        // حفظ الطلبية
        // ============================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string customerName = CustomerNameTextBox.Text.Trim();
            string customerPhone = CustomerPhoneTextBox.Text.Trim();
            string note = NoteTextBox.Text.Trim();

            bool isManualProduct =
                ManualProductCheckBox.IsChecked == true;

            // -----------------------------
            // Customer validation
            // -----------------------------

            if (string.IsNullOrWhiteSpace(customerName))
            {
                MessageBox.Show(
                    "من فضلك اكتب اسم العميل.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CustomerNameTextBox.Focus();
                return;
            }

            // -----------------------------
            // Quantity validation
            // -----------------------------

            if (!int.TryParse(
                    QuantityTextBox.Text.Trim(),
                    out int quantity) ||
                quantity <= 0)
            {
                MessageBox.Show(
                    "من فضلك أدخل كمية صحيحة أكبر من صفر.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                QuantityTextBox.Focus();
                return;
            }

            // -----------------------------
            // Product validation
            // -----------------------------

            int? productId = null;
            string productName = "";

            if (isManualProduct)
            {
                productName = ManualProductTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(productName))
                {
                    MessageBox.Show(
                        "من فضلك اكتب اسم المنتج المطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    ManualProductTextBox.Focus();
                    return;
                }
            }
            else
            {
                if (ProductComboBox.SelectedValue == null)
                {
                    MessageBox.Show(
                        "من فضلك اختر منتجًا من المنتجات الموجودة أو فعّل الإدخال اليدوي.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    ProductComboBox.Focus();
                    return;
                }

                if (!int.TryParse(
                        ProductComboBox.SelectedValue.ToString(),
                        out int selectedProductId))
                {
                    MessageBox.Show(
                        "تعذر تحديد المنتج.",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                productId = selectedProductId;

                // نجيب الاسم من Products مباشرة
                productName = GetProductName(selectedProductId);

                if (string.IsNullOrWhiteSpace(productName))
                {
                    MessageBox.Show(
                        "المنتج لم يعد موجودًا في قاعدة البيانات.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    LoadProducts();
                    return;
                }
            }

            // -----------------------------
            // Insert
            // -----------------------------

            try
            {
                using SqlConnection connection =
                    new SqlConnection(ConnectionString);

                connection.Open();

                string query = @"
                    INSERT INTO CustomerOrders
                    (
                        CustomerName,
                        CustomerPhone,
                        ProductID,
                        ProductName,
                        Quantity,
                        Note,
                        OrderDate,
                        Status,
                        IsManualProduct
                    )
                    VALUES
                    (
                        @CustomerName,
                        @CustomerPhone,
                        @ProductID,
                        @ProductName,
                        @Quantity,
                        @Note,
                        GETDATE(),
                        N'قيد الانتظار',
                        @IsManualProduct
                    )";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@CustomerName",
                    customerName);

                command.Parameters.AddWithValue(
                    "@CustomerPhone",
                    string.IsNullOrWhiteSpace(customerPhone)
                        ? DBNull.Value
                        : customerPhone);

                command.Parameters.AddWithValue(
                    "@ProductID",
                    productId.HasValue
                        ? productId.Value
                        : DBNull.Value);

                command.Parameters.AddWithValue(
                    "@ProductName",
                    productName);

                command.Parameters.AddWithValue(
                    "@Quantity",
                    quantity);

                command.Parameters.AddWithValue(
                    "@Note",
                    string.IsNullOrWhiteSpace(note)
                        ? DBNull.Value
                        : note);

                command.Parameters.AddWithValue(
                    "@IsManualProduct",
                    isManualProduct);

                command.ExecuteNonQuery();

                MessageBox.Show(
                    "تم حفظ الطلبية بنجاح.\n\nالحالة الحالية: قيد الانتظار",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ الطلبية:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================
        // جلب اسم المنتج الحقيقي
        // ============================================

        private string GetProductName(int productId)
        {
            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            string query = @"
                SELECT ProductName
                FROM Products
                WHERE ProductID = @ProductID";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@ProductID",
                productId);

            object result = command.ExecuteScalar();

            return result == null || result == DBNull.Value
                ? ""
                : result.ToString()!;
        }

        // ============================================
        // إلغاء
        // ============================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}