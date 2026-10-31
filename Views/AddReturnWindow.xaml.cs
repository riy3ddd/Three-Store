using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp1.Views
{
    public partial class AddReturnWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private int selectedSaleID = 0;

        private readonly List<ReturnItemModel> returnItems =
            new List<ReturnItemModel>();

        public AddReturnWindow()
        {
            InitializeComponent();

            // =========================================================
            // الضغط على Enter داخل رقم الفاتورة = تحميل الفاتورة
            // =========================================================

            SaleIDTextBox.KeyDown += SaleIDTextBox_KeyDown;
        }

        // =========================================================
        // Enter لتحميل الفاتورة
        // =========================================================

        private void SaleIDTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (!int.TryParse(
                        SaleIDTextBox.Text.Trim(),
                        out int saleID))
                {
                    MessageBox.Show(
                        "من فضلك أدخل رقم فاتورة صحيح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    SaleIDTextBox.SelectAll();
                    SaleIDTextBox.Focus();

                    return;
                }

                LoadSaleItems(saleID);
            }
        }


        // =========================================================
        // زر تحميل الفاتورة
        // =========================================================

        private void LoadSaleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string saleText =
                SaleIDTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(saleText))
            {
                MessageBox.Show(
                    "من فضلك أدخل رقم الفاتورة.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                SaleIDTextBox.Focus();

                return;
            }

            if (!int.TryParse(
                    saleText,
                    out int saleID))
            {
                MessageBox.Show(
                    "من فضلك أدخل رقم فاتورة صحيح.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                SaleIDTextBox.SelectAll();
                SaleIDTextBox.Focus();

                return;
            }

            LoadSaleItems(saleID);
        }

        // =========================================================
        // تحميل منتجات الفاتورة
        // =========================================================

        private void LoadSaleItems(int saleID)
        {
            try
            {
                // تنظيف البيانات القديمة أولًا
                selectedSaleID = 0;

                returnItems.Clear();

                SaleItemsDataGrid.ItemsSource = null;
                SaleItemsDataGrid.Items.Clear();

                SaleInfoTextBlock.Text =
                    "جاري تحميل الفاتورة...";

                using SqlConnection connection =
                    new SqlConnection(connectionString);

                connection.Open();

                // =====================================================
                // جلب المنتجات الموجودة في الفاتورة
                // =====================================================

                string query = @"
                    SELECT
                        si.ProductID,
                        p.ProductName,
                        SUM(si.Quantity) AS SoldQuantity,
                        si.UnitPrice,

                        ISNULL(
                            (
                                SELECT SUM(ri.Quantity)
                                FROM ReturnItems ri
                                INNER JOIN Returns r
                                    ON r.ReturnID = ri.ReturnID
                                WHERE r.SaleID = si.SaleID
                                  AND ri.ProductID = si.ProductID
                            ),
                            0
                        ) AS AlreadyReturned

                    FROM SaleItems si

                    INNER JOIN Products p
                        ON p.ProductID = si.ProductID

                    WHERE si.SaleID = @SaleID

                    GROUP BY
                        si.ProductID,
                        p.ProductName,
                        si.UnitPrice,
                        si.SaleID

                    ORDER BY
                        si.ProductID;
                ";

                using SqlCommand command =
                    new SqlCommand(
                        query,
                        connection);

                command.Parameters.Add(
                    "@SaleID",
                    SqlDbType.Int).Value = saleID;

                using SqlDataReader reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    int soldQuantity =
                        reader["SoldQuantity"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["SoldQuantity"]);

                    int alreadyReturned =
                        reader["AlreadyReturned"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["AlreadyReturned"]);

                    int availableToReturn =
                        soldQuantity -
                        alreadyReturned;

                    if (availableToReturn < 0)
                        availableToReturn = 0;

                    decimal unitPrice =
                        reader["UnitPrice"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(
                                reader["UnitPrice"]);

                    ReturnItemModel item =
                        new ReturnItemModel
                        {
                            ProductID =
                                Convert.ToInt32(
                                    reader["ProductID"]),

                            ProductName =
                                reader["ProductName"]?.ToString()
                                ?? "",

                            SoldQuantity =
                                soldQuantity,

                            AlreadyReturned =
                                alreadyReturned,

                            AvailableToReturn =
                                availableToReturn,

                            UnitPrice =
                                unitPrice,

                            ReturnQuantity = 0
                        };

                    item.PropertyChanged +=
                        ReturnItem_PropertyChanged;

                    returnItems.Add(item);
                }

                // =====================================================
                // الفاتورة غير موجودة
                // =====================================================

                if (returnItems.Count == 0)
                {
                    SaleInfoTextBlock.Text = "";

                    MessageBox.Show(
                        "الفاتورة غير موجودة أو لا تحتوي على منتجات.",
                        "الفاتورة غير موجودة",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    SaleIDTextBox.Focus();
                    SaleIDTextBox.SelectAll();

                    return;
                }

                // =====================================================
                // الفاتورة موجودة
                // =====================================================

                selectedSaleID = saleID;

                // ربط المنتجات بالـ DataGrid
                SaleItemsDataGrid.ItemsSource =
                    returnItems;

                SaleInfoTextBlock.Text =
                    $"الفاتورة رقم {saleID} تحتوي على {returnItems.Count} منتج";

                CalculateTotal();

                // نخلي المؤشر في أول خانة كمية
                SaleItemsDataGrid.Focus();
            }
            catch (Exception ex)
            {
                SaleInfoTextBlock.Text = "";

                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // عند تغيير كمية الاسترجاع
        // =========================================================

        private void ReturnItem_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==
                nameof(ReturnItemModel.ReturnQuantity))
            {
                CalculateTotal();
            }
        }

        // =========================================================
        // حفظ الاسترجاع
        // =========================================================

        private void SaveReturnButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (selectedSaleID <= 0)
            {
                MessageBox.Show(
                    "من فضلك قم بتحميل الفاتورة أولًا.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                SaleIDTextBox.Focus();

                return;
            }

            // إنهاء تعديل الخلية الحالية
            SaleItemsDataGrid.CommitEdit(
                DataGridEditingUnit.Cell,
                true);

            SaleItemsDataGrid.CommitEdit(
                DataGridEditingUnit.Row,
                true);

            int totalItems = 0;

            // =====================================================
            // التحقق من الكميات
            // =====================================================

            foreach (ReturnItemModel item in returnItems)
            {
                if (item.ReturnQuantity < 0)
                {
                    MessageBox.Show(
                        $"كمية الاسترجاع للمنتج {item.ProductName} غير صحيحة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (item.ReturnQuantity >
                    item.AvailableToReturn)
                {
                    MessageBox.Show(
                        $"المنتج: {item.ProductName}\n\n" +
                        $"الكمية المسموح باسترجاعها: " +
                        $"{item.AvailableToReturn}\n\n" +
                        $"الكمية المطلوبة: " +
                        $"{item.ReturnQuantity}",
                        "الكمية غير صحيحة",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                totalItems +=
                    item.ReturnQuantity;
            }

            // =====================================================
            // لا يوجد أي منتج
            // =====================================================

            if (totalItems == 0)
            {
                MessageBox.Show(
                    "من فضلك أدخل كمية استرجاع لمنتج واحد على الأقل.",
                    "تنبيه",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                SaveCompleteReturn();

                MessageBox.Show(
                    "تم تسجيل الاسترجاع بنجاح.\n\n" +
                    "تم إنشاء مرتجع واحد يحتوي على المنتجات المختارة، " +
                    "وتمت إعادة الكميات إلى المخزون.",
                    "تم بنجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ الاسترجاع:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // حفظ المرتجع بالكامل داخل Transaction واحدة
        // =========================================================

        private void SaveCompleteReturn()
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                // =================================================
                // 1. التأكد من وجود الفاتورة
                // =================================================

                string saleExistsQuery = @"
                    SELECT COUNT(*)
                    FROM Sales
                    WHERE SaleID = @SaleID;
                ";

                using (SqlCommand command =
                    new SqlCommand(
                        saleExistsQuery,
                        connection,
                        transaction))
                {
                    command.Parameters.Add(
                        "@SaleID",
                        SqlDbType.Int).Value =
                        selectedSaleID;

                    int count =
                        Convert.ToInt32(
                            command.ExecuteScalar());

                    if (count == 0)
                    {
                        throw new Exception(
                            "الفاتورة غير موجودة.");
                    }
                }

                // =================================================
                // 2. حساب إجمالي المرتجع
                // =================================================

                decimal totalReturnAmount = 0;

                foreach (ReturnItemModel item in returnItems)
                {
                    if (item.ReturnQuantity <= 0)
                        continue;

                    totalReturnAmount +=
                        item.ReturnQuantity *
                        item.UnitPrice;
                }

                if (totalReturnAmount <= 0)
                {
                    throw new Exception(
                        "قيمة الاسترجاع غير صحيحة.");
                }

                // =================================================
                // 3. إنشاء Return واحد
                // =================================================

                string insertReturnQuery = @"
                    INSERT INTO Returns
                    (
                        SaleID,
                        ReturnDate,
                        TotalAmount,
                        Notes,
                        CreatedBy,
                        CreatedAt
                    )
                    VALUES
                    (
                        @SaleID,
                        GETDATE(),
                        @TotalAmount,
                        @Notes,
                        NULL,
                        GETDATE()
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                ";

                int returnID;

                using (SqlCommand command =
                    new SqlCommand(
                        insertReturnQuery,
                        connection,
                        transaction))
                {
                    command.Parameters.Add(
                        "@SaleID",
                        SqlDbType.Int).Value =
                        selectedSaleID;

                    SqlParameter totalParameter =
                        command.Parameters.Add(
                            "@TotalAmount",
                            SqlDbType.Decimal);

                    totalParameter.Precision = 18;
                    totalParameter.Scale = 2;
                    totalParameter.Value =
                        totalReturnAmount;

                    if (string.IsNullOrWhiteSpace(
                        NotesTextBox.Text))
                    {
                        command.Parameters.Add(
                            "@Notes",
                            SqlDbType.NVarChar,
                            1000).Value =
                            DBNull.Value;
                    }
                    else
                    {
                        command.Parameters.Add(
                            "@Notes",
                            SqlDbType.NVarChar,
                            1000).Value =
                            NotesTextBox.Text.Trim();
                    }

                    returnID =
                        Convert.ToInt32(
                            command.ExecuteScalar());
                }

                // =================================================
                // 4. إدخال المنتجات المسترجعة
                // =================================================

                foreach (ReturnItemModel item in returnItems)
                {
                    if (item.ReturnQuantity <= 0)
                        continue;

                    // ---------------------------------------------
                    // الكمية الأصلية المباعة
                    // ---------------------------------------------

                    string soldQuery = @"
                        SELECT ISNULL(SUM(Quantity), 0)
                        FROM SaleItems
                        WHERE SaleID = @SaleID
                          AND ProductID = @ProductID;
                    ";

                    int soldQuantity;

                    using (SqlCommand command =
                        new SqlCommand(
                            soldQuery,
                            connection,
                            transaction))
                    {
                        command.Parameters.Add(
                            "@SaleID",
                            SqlDbType.Int).Value =
                            selectedSaleID;

                        command.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;

                        soldQuantity =
                            Convert.ToInt32(
                                command.ExecuteScalar());
                    }

                    if (soldQuantity <= 0)
                    {
                        throw new Exception(
                            $"المنتج {item.ProductName} غير موجود في الفاتورة.");
                    }

                    // ---------------------------------------------
                    // الكمية التي تم استرجاعها سابقًا
                    // ---------------------------------------------

                    string returnedQuery = @"
                        SELECT ISNULL(SUM(ri.Quantity), 0)
                        FROM ReturnItems ri
                        INNER JOIN Returns r
                            ON r.ReturnID = ri.ReturnID
                        WHERE r.SaleID = @SaleID
                          AND ri.ProductID = @ProductID;
                    ";

                    int alreadyReturned;

                    using (SqlCommand command =
                        new SqlCommand(
                            returnedQuery,
                            connection,
                            transaction))
                    {
                        command.Parameters.Add(
                            "@SaleID",
                            SqlDbType.Int).Value =
                            selectedSaleID;

                        command.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;

                        alreadyReturned =
                            Convert.ToInt32(
                                command.ExecuteScalar());
                    }

                    int availableToReturn =
                        soldQuantity -
                        alreadyReturned;

                    if (item.ReturnQuantity >
                        availableToReturn)
                    {
                        throw new Exception(
                            $"المنتج: {item.ProductName}\n\n" +
                            $"الكمية المتاحة للاسترجاع: {availableToReturn}\n" +
                            $"الكمية المطلوبة: {item.ReturnQuantity}");
                    }

                    // ---------------------------------------------
                    // إضافة ReturnItem
                    // ---------------------------------------------

                    decimal itemTotal =
                        item.ReturnQuantity *
                        item.UnitPrice;

                    string insertItemQuery = @"
                        INSERT INTO ReturnItems
                        (
                            ReturnID,
                            ProductID,
                            Quantity,
                            UnitPrice,
                            Total
                        )
                        VALUES
                        (
                            @ReturnID,
                            @ProductID,
                            @Quantity,
                            @UnitPrice,
                            @Total
                        );
                    ";

                    using (SqlCommand command =
                        new SqlCommand(
                            insertItemQuery,
                            connection,
                            transaction))
                    {
                        command.Parameters.Add(
                            "@ReturnID",
                            SqlDbType.Int).Value =
                            returnID;

                        command.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;

                        command.Parameters.Add(
                            "@Quantity",
                            SqlDbType.Int).Value =
                            item.ReturnQuantity;

                        SqlParameter unitPriceParameter =
                            command.Parameters.Add(
                                "@UnitPrice",
                                SqlDbType.Decimal);

                        unitPriceParameter.Precision = 18;
                        unitPriceParameter.Scale = 2;
                        unitPriceParameter.Value =
                            item.UnitPrice;

                        SqlParameter totalParameter =
                            command.Parameters.Add(
                                "@Total",
                                SqlDbType.Decimal);

                        totalParameter.Precision = 18;
                        totalParameter.Scale = 2;
                        totalParameter.Value =
                            itemTotal;

                        command.ExecuteNonQuery();
                    }

                    // ---------------------------------------------
                    // إعادة الكمية للمخزون
                    // ---------------------------------------------

                    string updateProductQuery = @"
                        UPDATE Products
                        SET Quantity = Quantity + @Quantity
                        WHERE ProductID = @ProductID;
                    ";

                    using (SqlCommand command =
                        new SqlCommand(
                            updateProductQuery,
                            connection,
                            transaction))
                    {
                        command.Parameters.Add(
                            "@Quantity",
                            SqlDbType.Int).Value =
                            item.ReturnQuantity;

                        command.Parameters.Add(
                            "@ProductID",
                            SqlDbType.Int).Value =
                            item.ProductID;

                        int affected =
                            command.ExecuteNonQuery();

                        if (affected == 0)
                        {
                            throw new Exception(
                                $"تعذر تحديث مخزون المنتج {item.ProductName}.");
                        }
                    }
                }

                // =================================================
                // 5. اعتماد العملية
                // =================================================

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // =========================================================
        // حساب إجمالي المرتجع
        // =========================================================

        private void CalculateTotal()
        {
            decimal total = 0;

            foreach (ReturnItemModel item in returnItems)
            {
                if (item.ReturnQuantity <= 0)
                    continue;

                total +=
                    item.ReturnQuantity *
                    item.UnitPrice;
            }

            TotalReturnTextBlock.Text =
                $"{total:N2} ج";
        }
    }

    // =============================================================
    // Model
    // =============================================================

    public class ReturnItemModel : INotifyPropertyChanged
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; } = "";

        public int SoldQuantity { get; set; }

        public int AlreadyReturned { get; set; }

        public int AvailableToReturn { get; set; }

        public decimal UnitPrice { get; set; }

        private int returnQuantity;

        public int ReturnQuantity
        {
            get => returnQuantity;

            set
            {
                if (returnQuantity == value)
                    return;

                returnQuantity = value;

                OnPropertyChanged(
                    nameof(ReturnQuantity));

                OnPropertyChanged(
                    nameof(ReturnTotal));
            }
        }

        public decimal ReturnTotal
        {
            get => ReturnQuantity * UnitPrice;
        }

        public event PropertyChangedEventHandler?
            PropertyChanged;

        protected virtual void OnPropertyChanged(
            string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    propertyName));
        }
    }
}
