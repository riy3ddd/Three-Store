using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Views
{
    public partial class SalesWindow : Window
    {
        // ==============================
        // Connection String
        // ==============================

        private const string ConnectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";


        // ==============================
        // سلة المبيعات
        // ==============================

        private ObservableCollection<CartItem> cartItems =
            new ObservableCollection<CartItem>();


        // ==============================
        // بيانات المنتجات
        // ==============================

        private DataTable productsTable =
            new DataTable();


        // ==============================
        // Placeholder
        // ==============================

        private const string SearchPlaceholder =
            "ابحث عن منتج بالاسم 🔍...";


        // ==============================
        // Event لتحديث Dashboard
        // ==============================

        public event Action? SaleCompleted;


        // ==============================
        // Constructor
        // ==============================

        public SalesWindow()
        {
            InitializeComponent();

            CartGrid.ItemsSource =
                cartItems;

            UpdateInvoiceNumber();

            SearchBox.Text =
                SearchPlaceholder;

            SearchBox.Foreground =
                new SolidColorBrush(
                    Colors.Gray);

            LoadProducts();

            ProductsGrid.MouseDoubleClick +=
                ProductsGrid_MouseDoubleClick;
        }


        // =========================================================
        // رقم الفاتورة
        // =========================================================

        private void UpdateInvoiceNumber()
        {
            int nextInvoiceNumber =
                DatabaseHelper.GetNextInvoiceNumber();

            InvoiceNumberText.Text =
                $"رقم الفاتورة: {nextInvoiceNumber}";
        }


        // =========================================================
        // الحدث القديم
        // =========================================================

        private void PaymentMethodComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // موجود فقط للحفاظ على توافق XAML القديم.
        }


        // =========================================================
        // تحميل المنتجات
        // =========================================================

        private void LoadProducts()
        {
            productsTable =
                DatabaseHelper.GetProductsForSales();

            ProductsGrid.ItemsSource =
                productsTable.DefaultView;
        }


        // =========================================================
        // ترقيم المنتجات
        // =========================================================

        private void ProductsGrid_LoadingRow(
            object sender,
            DataGridRowEventArgs e)
        {
            e.Row.Header =
                e.Row.GetIndex() + 1;
        }


        // =========================================================
        // البحث
        // =========================================================

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (SearchBox.Text ==
                SearchPlaceholder)
            {
                return;
            }

            string searchText =
                SearchBox.Text.Trim();

            if (ProductsGrid.ItemsSource
                is DataView view)
            {
                if (string.IsNullOrWhiteSpace(
                        searchText))
                {
                    view.RowFilter =
                        "";
                }
                else
                {
                    string safeSearch =
                        searchText.Replace(
                            "'",
                            "''");

                    view.RowFilter =
                        $"ProductName LIKE '%{safeSearch}%'";
                }
            }
        }


        // =========================================================
        // البحث Focus
        // =========================================================

        private void SearchBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (SearchBox.Text ==
                SearchPlaceholder)
            {
                SearchBox.Text =
                    "";

                SearchBox.Foreground =
                    new SolidColorBrush(
                        Colors.Black);
            }
        }


        // =========================================================
        // البحث Lost Focus
        // =========================================================

        private void SearchBox_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    SearchBox.Text))
            {
                SearchBox.Text =
                    SearchPlaceholder;

                SearchBox.Foreground =
                    new SolidColorBrush(
                        Colors.Gray);
            }
        }


        // =========================================================
        // Double Click المنتج
        // =========================================================

        private void ProductsGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (ProductsGrid.SelectedItem ==
                null)
            {
                return;
            }

            if (ProductsGrid.SelectedItem
                is not DataRowView row)
            {
                return;
            }

            int productId =
                Convert.ToInt32(
                    row["ProductID"]);

            string productName =
                row["ProductName"]?.ToString()
                ?? "";

            int availableQuantity =
                Convert.ToInt32(
                    row["Quantity"]);

            decimal sellingPrice =
                row["SellingPrice"] ==
                DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(
                        row["SellingPrice"]);

            AddProductToCart(
                productId,
                productName,
                availableQuantity,
                sellingPrice);
        }


        // =========================================================
        // إضافة للسلة
        // =========================================================

        private void AddProductToCart(
            int productId,
            string productName,
            int availableQuantity,
            decimal sellingPrice)
        {
            CartItem? existingItem =
                cartItems.FirstOrDefault(
                    x => x.ProductID ==
                         productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity >=
                    existingItem.AvailableQuantity)
                {
                    MessageBox.Show(
                        $"عذراً، لا يمكن إضافة الكمية.\n\nالمخزون المتوفر هو {existingItem.AvailableQuantity} فقط.",
                        "الكمية غير متاحة",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                existingItem.Quantity++;
            }
            else
            {
                cartItems.Add(
                    new CartItem
                    {
                        ProductID =
                            productId,

                        ProductName =
                            productName,

                        Quantity =
                            1,

                        AvailableQuantity =
                            availableQuantity,

                        SellingPrice =
                            sellingPrice
                    });
            }

            RenumberCart();

            CartGrid.Items.Refresh();

            UpdateGrandTotal();
        }


        // =========================================================
        // إعادة الترقيم
        // =========================================================

        private void RenumberCart()
        {
            int number = 1;

            foreach (CartItem item in cartItems)
            {
                item.CartNumber =
                    number;

                number++;
            }
        }


        // =========================================================
        // قراءة الخصم
        // =========================================================

        private decimal GetCurrentDiscount()
        {
            if (FindName("DiscountTextBox")
                is not TextBox discountBox)
            {
                return 0m;
            }

            if (!decimal.TryParse(
                    discountBox.Text.Trim(),
                    out decimal discount))
            {
                return 0m;
            }

            if (discount < 0)
                return 0m;

            return discount;
        }


        // =========================================================
        // حساب الإجمالي
        // =========================================================

        private void UpdateGrandTotal()
        {
            decimal subtotal =
                cartItems.Sum(
                    x => x.Total);

            decimal discount =
                GetCurrentDiscount();

            if (discount > subtotal)
            {
                discount =
                    subtotal;
            }

            decimal finalTotal =
                subtotal -
                discount;

            if (FindName("SubtotalText")
                is TextBlock subtotalText)
            {
                subtotalText.Text =
                    $"{subtotal:N2} جنيه";
            }

            if (FindName("GrandTotalText")
                is TextBlock grandTotalText)
            {
                grandTotalText.Text =
                    $"{finalTotal:N2} جنيه";
            }
        }


        // =========================================================
        // تغيير الخصم
        // =========================================================

        private void DiscountTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            UpdateGrandTotal();
        }


        // =========================================================
        // تعديل الكمية
        // =========================================================

        private void CartGrid_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            CartGrid.CommitEdit(
                DataGridEditingUnit.Cell,
                true);

            CartGrid.CommitEdit(
                DataGridEditingUnit.Row,
                true);

            if (CartGrid.SelectedItem
                is CartItem item)
            {
                if (item.Quantity >
                    item.AvailableQuantity)
                {
                    MessageBox.Show(
                        $"عذراً، لا يمكن إضافة الكمية.\n\nالمخزون المتوفر هو {item.AvailableQuantity} فقط.",
                        "الكمية غير متاحة",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    item.Quantity =
                        item.AvailableQuantity;
                }

                if (item.Quantity < 1)
                {
                    item.Quantity =
                        1;
                }
            }

            CartGrid.Items.Refresh();

            UpdateGrandTotal();

            e.Handled =
                true;
        }


        // =========================================================
        // مسح السلة
        // =========================================================

        private void ClearCartButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show(
                    "السلة فارغة بالفعل.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    "هل تريد مسح جميع المنتجات من السلة؟",
                    "تأكيد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result ==
                MessageBoxResult.Yes)
            {
                cartItems.Clear();

                CartGrid.Items.Refresh();

                ResetDiscount();

                UpdateGrandTotal();
            }
        }


        // =========================================================
        // حذف منتج من السلة
        // =========================================================

        private void DeleteCartItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext
                is CartItem item)
            {
                cartItems.Remove(item);

                RenumberCart();

                CartGrid.Items.Refresh();

                UpdateGrandTotal();
            }
        }


        // =========================================================
        // تحديث أسعار السلة من العروض - قبل نافذة الدفع
        // =========================================================

        private void RefreshCartPricesFromOffersPreview()
        {
            DataTable latestProducts =
                DatabaseHelper.GetProductsForSales();

            foreach (CartItem item in cartItems)
            {
                DataRow[] rows =
                    latestProducts.Select(
                        $"ProductID = {item.ProductID}");

                if (rows.Length == 0)
                {
                    throw new Exception(
                        $"المنتج \"{item.ProductName}\" لم يعد متاحًا للبيع.");
                }

                decimal price =
                    rows[0]["SellingPrice"] ==
                    DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(
                            rows[0]["SellingPrice"]);

                item.SellingPrice =
                    price;
            }

            CartGrid.Items.Refresh();

            UpdateGrandTotal();
        }


        // =========================================================
        // تحديث أسعار السلة داخل Transaction
        // =========================================================

        private void RefreshCartPricesFromOffers(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            foreach (CartItem item in cartItems)
            {
                string query = @"
                    SELECT
                        CASE
                            WHEN o.OfferID IS NOT NULL
                                THEN o.OfferPrice
                            ELSE p.SellingPrice
                        END AS EffectivePrice
                    FROM Products p
                    OUTER APPLY
                    (
                        SELECT TOP 1
                            o.OfferID,
                            o.OfferPrice
                        FROM Offers o
                        WHERE
                            o.ProductID =
                                p.ProductID
                            AND o.IsActive = 1
                            AND o.StartDate <=
                                GETDATE()
                            AND
                            (
                                o.EndDate IS NULL
                                OR GETDATE() <
                                   o.EndDate
                            )
                        ORDER BY
                            o.OfferID DESC
                    ) o
                    WHERE
                        p.ProductID =
                        @ProductID;";

                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection,
                        transaction);

                command.Parameters.Add(
                    "@ProductID",
                    SqlDbType.Int).Value =
                    item.ProductID;

                object result =
                    command.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    throw new Exception(
                        $"لم يتم العثور على سعر للمنتج \"{item.ProductName}\".");
                }

                decimal price =
                    Convert.ToDecimal(result);

                if (price < 0)
                {
                    throw new Exception(
                        $"سعر المنتج \"{item.ProductName}\" غير صالح.");
                }

                item.SellingPrice =
                    price;
            }

            CartGrid.Items.Refresh();

            UpdateGrandTotal();
        }


        // =========================================================
        // الحصول على الخصم والإجمالي النهائي
        // =========================================================

        private void CalculateInvoiceValues(
            out decimal subtotal,
            out decimal discount,
            out decimal total)
        {
            subtotal =
                cartItems.Sum(
                    x => x.Total);

            discount =
                GetCurrentDiscount();

            if (discount < 0)
                discount = 0;

            if (discount > subtotal)
                discount = subtotal;

            total =
                subtotal -
                discount;
        }


        // =========================================================
        // الحصول على حساب العميل الآجل
        // =========================================================

        private int GetOrCreateCreditAccount(
            SqlConnection connection,
            SqlTransaction transaction,
            string customerName,
            string customerPhone)
        {
            string selectQuery;

            if (!string.IsNullOrWhiteSpace(
                    customerPhone))
            {
                selectQuery = @"
                    SELECT TOP 1
                        CreditAccountID
                    FROM CreditAccounts
                    WHERE
                        IsActive = 1
                        AND CustomerPhone =
                            @CustomerPhone
                    ORDER BY
                        CreditAccountID DESC;";
            }
            else
            {
                selectQuery = @"
                    SELECT TOP 1
                        CreditAccountID
                    FROM CreditAccounts
                    WHERE
                        IsActive = 1
                        AND CustomerName =
                            @CustomerName
                        AND
                        (
                            CustomerPhone IS NULL
                            OR CustomerPhone = N''
                        )
                    ORDER BY
                        CreditAccountID DESC;";
            }

            using SqlCommand selectCommand =
                new SqlCommand(
                    selectQuery,
                    connection,
                    transaction);

            if (!string.IsNullOrWhiteSpace(
                    customerPhone))
            {
                selectCommand.Parameters.Add(
                    "@CustomerPhone",
                    SqlDbType.NVarChar,
                    30).Value =
                    customerPhone;
            }
            else
            {
                selectCommand.Parameters.Add(
                    "@CustomerName",
                    SqlDbType.NVarChar,
                    150).Value =
                    customerName;
            }

            object result =
                selectCommand.ExecuteScalar();

            if (result != null &&
                result != DBNull.Value)
            {
                int existingAccountId =
                    Convert.ToInt32(result);

                using SqlCommand updateCommand =
                    new SqlCommand(
                        @"
                        UPDATE CreditAccounts
                        SET
                            CustomerName =
                                @CustomerName,
                            CustomerPhone =
                                @CustomerPhone,
                            UpdatedAt =
                                GETDATE()
                        WHERE
                            CreditAccountID =
                            @CreditAccountID;",
                        connection,
                        transaction);

                updateCommand.Parameters.Add(
                    "@CustomerName",
                    SqlDbType.NVarChar,
                    150).Value =
                    customerName;

                updateCommand.Parameters.Add(
                    "@CustomerPhone",
                    SqlDbType.NVarChar,
                    30).Value =
                    string.IsNullOrWhiteSpace(
                        customerPhone)
                        ? DBNull.Value
                        : customerPhone;

                updateCommand.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    existingAccountId;

                updateCommand.ExecuteNonQuery();

                return existingAccountId;
            }

            string insertQuery = @"
                INSERT INTO CreditAccounts
                (
                    CustomerName,
                    CustomerPhone,
                    CreatedAt,
                    UpdatedAt,
                    IsActive
                )
                OUTPUT INSERTED.CreditAccountID
                VALUES
                (
                    @CustomerName,
                    @CustomerPhone,
                    GETDATE(),
                    GETDATE(),
                    1
                );";

            using SqlCommand insertCommand =
                new SqlCommand(
                    insertQuery,
                    connection,
                    transaction);

            insertCommand.Parameters.Add(
                "@CustomerName",
                SqlDbType.NVarChar,
                150).Value =
                customerName;

            insertCommand.Parameters.Add(
                "@CustomerPhone",
                SqlDbType.NVarChar,
                30).Value =
                string.IsNullOrWhiteSpace(
                    customerPhone)
                    ? DBNull.Value
                    : customerPhone;

            return Convert.ToInt32(
                insertCommand.ExecuteScalar());
        }


        // =========================================================
        // تنفيذ البيع
        // =========================================================
        private void CompleteSaleButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show(
                    "السلة فارغة، لا يمكن إتمام البيع.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // =====================================================
            // المتغيرات لازم تكون خارج الـ try
            // عشان تفضل متاحة في باقي الدالة
            // =====================================================

            int saleId = 0;

            decimal total = 0m;

            decimal creditAmount = 0m;

            decimal paidNow = 0m;


            try
            {
                // =================================================
                // تحديث الأسعار من العروض قبل فتح الدفع
                // =================================================

                RefreshCartPricesFromOffersPreview();


                CalculateInvoiceValues(
                    out decimal subtotalBeforePayment,
                    out decimal discountBeforePayment,
                    out decimal totalBeforePayment);


                if (totalBeforePayment < 0)
                {
                    MessageBox.Show(
                        "إجمالي الفاتورة غير صحيح.",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }


                // =================================================
                // نافذة بيانات الدفع
                // =================================================

                SaleDetailsWindow saleDetailsWindow =
                    new SaleDetailsWindow(
                        totalBeforePayment);


                saleDetailsWindow.Owner =
                    this;


                bool? result =
                    saleDetailsWindow.ShowDialog();


                if (result != true ||
                    !saleDetailsWindow.IsConfirmed)
                {
                    return;
                }


                // =================================================
                // استلام بيانات الدفع
                // =================================================

                string paymentMethod =
                    saleDetailsWindow.PaymentMethod;


                string customerName =
                    saleDetailsWindow.CustomerName;


                string customerPhone =
                    saleDetailsWindow.CustomerPhone;


                string vodafoneCashNumber =
                    saleDetailsWindow.VodafoneCashNumber;


                bool isCredit =
                    saleDetailsWindow.IsCredit;


                paidNow =
                    saleDetailsWindow.PaidAmount;


                // =================================================
                // فتح الاتصال
                // =================================================

                using SqlConnection connection =
                    new SqlConnection(
                        ConnectionString);


                connection.Open();


                // =================================================
                // Transaction
                // =================================================

                using SqlTransaction transaction =
                    connection.BeginTransaction();


                try
                {
                    // =============================================
                    // تأكيد الأسعار مرة أخرى داخل Transaction
                    // =============================================

                    RefreshCartPricesFromOffers(
                        connection,
                        transaction);


                    // =============================================
                    // إعادة حساب الفاتورة
                    // =============================================

                    decimal subtotal;
                    decimal discount;

                    CalculateInvoiceValues(
                        out subtotal,
                        out discount,
                        out total);


                    // =============================================
                    // التأكد أن السعر لم يتغير
                    // =============================================

                    if (Math.Abs(
                            total -
                            saleDetailsWindow.InvoiceTotal)
                        > 0.01m)
                    {
                        throw new Exception(
                            "تغير سعر أحد المنتجات أثناء عملية البيع. من فضلك أعد محاولة البيع.");
                    }


                    // =============================================
                    // تحديد المدفوع والآجل
                    // =============================================

                    creditAmount = 0m;


                    if (isCredit)
                    {
                        if (paidNow < 0 ||
                            paidNow >= total)
                        {
                            throw new Exception(
                                "قيمة المدفوع في البيع الآجل غير صحيحة.");
                        }


                        creditAmount =
                            total -
                            paidNow;


                        if (creditAmount <= 0)
                        {
                            throw new Exception(
                                "لا يوجد مبلغ آجل متبقي.");
                        }


                        if (string.IsNullOrWhiteSpace(
                                customerName))
                        {
                            throw new Exception(
                                "اسم العميل مطلوب في البيع الآجل.");
                        }
                    }
                    else
                    {
                        paidNow =
                            total;

                        creditAmount =
                            0m;
                    }


                    // =============================================
                    // إنشاء سجل البيع
                    // =============================================
                    string saleQuery = @"
    INSERT INTO Sales
    (
        SaleDate,
        Subtotal,
        Discount,
        Total,
        Paid,
        Change,
        PaymentMethod,
        VodafoneCashNumber,
        CustomerName,
        CustomerPhone,
        CreatedBy
    )
    OUTPUT INSERTED.SaleID
    VALUES
    (
        GETDATE(),
        @Subtotal,
        @Discount,
        @Total,
        @Paid,
        @Change,
        @PaymentMethod,
        @VodafoneCashNumber,
        @CustomerName,
        @CustomerPhone,
        @CreatedBy
    );";

                    using SqlCommand saleCommand =
                        new SqlCommand(
                            saleQuery,
                            connection,
                            transaction);


                    saleCommand.Parameters.Add(
                        "@Subtotal",
                        SqlDbType.Decimal).Value =
                        subtotal;


                    saleCommand.Parameters[
                        "@Subtotal"]
                        .Precision = 18;

                    saleCommand.Parameters[
                        "@Subtotal"]
                        .Scale = 2;


                    saleCommand.Parameters.Add(
                        "@Discount",
                        SqlDbType.Decimal).Value =
                        discount;


                    saleCommand.Parameters[
                        "@Discount"]
                        .Precision = 18;

                    saleCommand.Parameters[
                        "@Discount"]
                        .Scale = 2;


                    saleCommand.Parameters.Add(
                        "@Total",
                        SqlDbType.Decimal).Value =
                        total;


                    saleCommand.Parameters[
                        "@Total"]
                        .Precision = 18;

                    saleCommand.Parameters[
                        "@Total"]
                        .Scale = 2;


                    saleCommand.Parameters.Add(
                        "@Paid",
                        SqlDbType.Decimal).Value =
                        paidNow;


                    saleCommand.Parameters[
                        "@Paid"]
                        .Precision = 18;

                    saleCommand.Parameters[
                        "@Paid"]
                        .Scale = 2;


                    saleCommand.Parameters.Add(
                        "@Change",
                        SqlDbType.Decimal).Value =
                        0m;


                    saleCommand.Parameters.Add(
                        "@PaymentMethod",
                        SqlDbType.NVarChar,
                        50).Value =
                        paymentMethod;


                    saleCommand.Parameters.Add(
                        "@VodafoneCashNumber",
                        SqlDbType.NVarChar,
                        30).Value =
                        paymentMethod ==
                        "فودافون كاش"
                            ? (
                                string.IsNullOrWhiteSpace(
                                    vodafoneCashNumber)
                                    ? DBNull.Value
                                    : vodafoneCashNumber
                              )
                            : DBNull.Value;


                    saleCommand.Parameters.Add(
                        "@CustomerName",
                        SqlDbType.NVarChar,
                        150).Value =
                        string.IsNullOrWhiteSpace(
                            customerName)
                            ? DBNull.Value
                            : customerName;


                    saleCommand.Parameters.Add(
                        "@CustomerPhone",
                        SqlDbType.NVarChar,
                        30).Value =
                        string.IsNullOrWhiteSpace(
                            customerPhone)
                            ? DBNull.Value
                            : customerPhone;

                    saleCommand.Parameters.Add(
    "@CreatedBy",
    SqlDbType.Int).Value =
    SessionManager.CurrentUser?.UserID
    ?? throw new Exception(
        "لا يوجد مستخدم مسجل دخول حاليًا.");
                    // =================================================
                    // هنا بنحط القيمة في المتغير الخارجي saleId
                    // =================================================

                    saleId =
                        Convert.ToInt32(
                            saleCommand.ExecuteScalar());


                    // =============================================
                    // حفظ SaleItems
                    // =============================================

                    foreach (CartItem item
                             in cartItems)
                    {
                        string itemQuery = @"
                    INSERT INTO SaleItems
                    (
                        SaleID,
                        ProductID,
                        Quantity,
                        UnitPrice,
                        Total
                    )
                    VALUES
                    (
                        @SaleID,
                        @ProductID,
                        @Quantity,
                        @UnitPrice,
                        @Total
                    );";


                        using SqlCommand itemCommand =
                            new SqlCommand(
                                itemQuery,
                                connection,
                                transaction);


                        itemCommand.Parameters.Add(
                            "@SaleID",
                            SqlDbType.Int).Value =
                            saleId;


                        itemCommand.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;


                        itemCommand.Parameters.Add(
                            "@Quantity",
                            SqlDbType.Int).Value =
                            item.Quantity;


                        itemCommand.Parameters.Add(
                            "@UnitPrice",
                            SqlDbType.Decimal).Value =
                            item.SellingPrice;


                        itemCommand.Parameters[
                            "@UnitPrice"]
                            .Precision = 18;

                        itemCommand.Parameters[
                            "@UnitPrice"]
                            .Scale = 2;


                        itemCommand.Parameters.Add(
                            "@Total",
                            SqlDbType.Decimal).Value =
                            item.Total;


                        itemCommand.Parameters[
                            "@Total"]
                            .Precision = 18;

                        itemCommand.Parameters[
                            "@Total"]
                            .Scale = 2;


                        itemCommand.ExecuteNonQuery();


                        // =========================================
                        // خصم المخزون
                        // =========================================

                        string stockQuery = @"
                    UPDATE Products
                    SET
                        Quantity =
                            Quantity -
                            @Quantity
                    WHERE
                        ProductID =
                            @ProductID
                        AND Quantity >=
                            @Quantity;";


                        using SqlCommand stockCommand =
                            new SqlCommand(
                                stockQuery,
                                connection,
                                transaction);


                        stockCommand.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;


                        stockCommand.Parameters.Add(
                            "@Quantity",
                            SqlDbType.Int).Value =
                            item.Quantity;


                        int affectedRows =
                            stockCommand.ExecuteNonQuery();


                        if (affectedRows == 0)
                        {
                            throw new Exception(
                                $"الكمية المطلوبة من المنتج \"{item.ProductName}\" غير متوفرة في المخزون.");
                        }
                    }


                    // =============================================
                    // إنشاء حساب الآجل
                    // =============================================

                    if (creditAmount > 0)
                    {
                        int creditAccountId =
                            GetOrCreateCreditAccount(
                                connection,
                                transaction,
                                customerName,
                                customerPhone);


                        string creditQuery = @"
                    INSERT INTO CreditTransactions
                    (
                        CreditAccountID,
                        TransactionType,
                        Amount,
                        TransactionDate,
                        PaymentMethod,
                        Notes,
                        SaleID,
                        CreatedAt
                    )
                    VALUES
                    (
                        @CreditAccountID,
                        N'Credit',
                        @Amount,
                        GETDATE(),
                        @PaymentMethod,
                        @Notes,
                        @SaleID,
                        GETDATE()
                    );";


                        using SqlCommand creditCommand =
                            new SqlCommand(
                                creditQuery,
                                connection,
                                transaction);


                        creditCommand.Parameters.Add(
                            "@CreditAccountID",
                            SqlDbType.Int).Value =
                            creditAccountId;


                        creditCommand.Parameters.Add(
                            "@Amount",
                            SqlDbType.Decimal).Value =
                            creditAmount;


                        creditCommand.Parameters[
                            "@Amount"]
                            .Precision = 18;

                        creditCommand.Parameters[
                            "@Amount"]
                            .Scale = 2;


                        creditCommand.Parameters.Add(
                            "@PaymentMethod",
                            SqlDbType.NVarChar,
                            50).Value =
                            paymentMethod;


                        creditCommand.Parameters.Add(
                            "@Notes",
                            SqlDbType.NVarChar,
                            500).Value =
                            "آجل من فاتورة المبيعات";


                        creditCommand.Parameters.Add(
                            "@SaleID",
                            SqlDbType.Int).Value =
                            saleId;


                        creditCommand.ExecuteNonQuery();


                        using SqlCommand updateAccountCommand =
                            new SqlCommand(
                                @"
                        UPDATE CreditAccounts
                        SET
                            UpdatedAt =
                                GETDATE()
                        WHERE
                            CreditAccountID =
                                @CreditAccountID;",
                                connection,
                                transaction);


                        updateAccountCommand.Parameters.Add(
                            "@CreditAccountID",
                            SqlDbType.Int).Value =
                            creditAccountId;


                        updateAccountCommand.ExecuteNonQuery();
                    }


                    // =============================================
                    // Commit
                    // =============================================

                    transaction.Commit();
                }
                catch
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                    }

                    throw;
                }


                // =================================================
                // تحديث Dashboard
                // =================================================

                SaleCompleted?.Invoke();


                // =================================================
                // رسالة النجاح
                // =================================================

                string invoiceNumber =
                    $"INV-{saleId:D6}";


                string successMessage;


                if (creditAmount > 0)
                {
                    successMessage =
                        $"تم إتمام البيع بنجاح.\n\n" +
                        $"رقم الفاتورة: {invoiceNumber}\n" +
                        $"طريقة الدفع: {paymentMethod}\n" +
                        $"الإجمالي: {total:N2} جنيه\n" +
                        $"المدفوع الآن: {paidNow:N2} جنيه\n" +
                        $"الآجل: {creditAmount:N2} جنيه";
                }
                else
                {
                    successMessage =
                        $"تم إتمام البيع بنجاح.\n\n" +
                        $"رقم الفاتورة: {invoiceNumber}\n" +
                        $"طريقة الدفع: {paymentMethod}\n" +
                        $"الإجمالي: {total:N2} جنيه";
                }


                MessageBox.Show(
                    successMessage,
                    "تم البيع",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // =================================================
                // فتح الفاتورة
                // =================================================

                InvoiceWindow invoiceWindow =
                    new InvoiceWindow(
                        saleId);


                invoiceWindow.Owner =
                    this;


                invoiceWindow.ShowDialog();


                // =================================================
                // تنظيف السلة
                // =================================================

                cartItems.Clear();

                CartGrid.Items.Refresh();

                ResetDiscount();

                UpdateGrandTotal();


                // =================================================
                // إعادة تحميل المنتجات
                // =================================================

                LoadProducts();


                // =================================================
                // تحديث رقم الفاتورة
                // =================================================

                UpdateInvoiceNumber();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء إتمام البيع:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // Reset Discount
        // =========================================================

        private void ResetDiscount()
        {
            if (FindName("DiscountTextBox")
                is TextBox discountBox)
            {
                discountBox.Text =
                    "0";
            }
        }
    }
}