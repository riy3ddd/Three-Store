using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class SellDeviceWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int deviceId;

        private decimal originalSellingPrice = 0;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public SellDeviceWindow(int id)
        {
            InitializeComponent();

            deviceId = id;

            LoadDevice();
        }


        // =========================================================
        // LOAD DEVICE
        // =========================================================

        private void LoadDevice()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            DeviceName,
                            SellingPrice,
                            CustomerName,
                            CustomerPhone,
                            DeviceStatus
                        FROM dbo.Devices
                        WHERE DeviceID = @DeviceID
                          AND ISNULL(IsDeleted, 0) = 0;
                        ",
                        connection);

                command.Parameters.Add(
                    "@DeviceID",
                    SqlDbType.Int).Value = deviceId;

                connection.Open();

                using SqlDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "الجهاز غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }


                // =====================================================
                // DEVICE NAME
                // =====================================================

                DeviceNameText.Text =
                    reader["DeviceName"] == DBNull.Value
                        ? ""
                        : reader["DeviceName"].ToString();


                // =====================================================
                // SELLING PRICE
                // =====================================================

                if (reader["SellingPrice"] != DBNull.Value)
                {
                    originalSellingPrice =
                        Convert.ToDecimal(
                            reader["SellingPrice"]);

                    SellingPriceTextBox.Text =
                        originalSellingPrice.ToString("0.##");

                    CollectedAmountTextBox.Text =
                        originalSellingPrice.ToString("0.##");
                }


                // =====================================================
                // CUSTOMER NAME
                // =====================================================

                if (reader["CustomerName"] != DBNull.Value)
                {
                    CustomerNameTextBox.Text =
                        reader["CustomerName"].ToString();
                }


                // =====================================================
                // CUSTOMER PHONE
                // =====================================================

                if (reader["CustomerPhone"] != DBNull.Value)
                {
                    CustomerPhoneTextBox.Text =
                        reader["CustomerPhone"].ToString();
                }


                // =====================================================
                // DEVICE STATUS
                // =====================================================

                string status =
                    reader["DeviceStatus"] == DBNull.Value
                        ? ""
                        : reader["DeviceStatus"].ToString();

                if (status == "مباع")
                {
                    MessageBox.Show(
                        "هذا الجهاز مباع بالفعل.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }


                UpdateRemainingAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات الجهاز:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // PAYMENT TYPE CHANGED
        // =========================================================

        private void PaymentType_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (FullPaymentRadio.IsChecked == true)
            {
                // دفعة كاملة
                CollectedAmountTextBox.Text =
                    SellingPriceTextBox.Text;

                CollectedAmountTextBox.IsReadOnly = true;

                RemainingCreditBorder.Visibility =
                    Visibility.Collapsed;
            }
            else if (CreditPaymentRadio.IsChecked == true)
            {
                // دفعة آجل
                CollectedAmountTextBox.IsReadOnly = false;

                RemainingCreditBorder.Visibility =
                    Visibility.Visible;

                UpdateRemainingAmount();
            }
        }


        // =========================================================
        // SELLING PRICE CHANGED
        // =========================================================

        private void SellingPriceTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (FullPaymentRadio.IsChecked == true)
            {
                CollectedAmountTextBox.Text =
                    SellingPriceTextBox.Text;
            }

            UpdateRemainingAmount();
        }


        // =========================================================
        // COLLECTED AMOUNT CHANGED
        // =========================================================

        private void CollectedAmountTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            UpdateRemainingAmount();
        }


        // =========================================================
        // UPDATE REMAINING
        // =========================================================

        private void UpdateRemainingAmount()
        {
            if (RemainingAmountText == null ||
                SellingPriceTextBox == null ||
                CollectedAmountTextBox == null)
            {
                return;
            }


            if (!decimal.TryParse(
                    SellingPriceTextBox.Text.Trim(),
                    out decimal sellingPrice))
            {
                RemainingAmountText.Text =
                    "0.00 ج.م";

                return;
            }


            if (!decimal.TryParse(
                    CollectedAmountTextBox.Text.Trim(),
                    out decimal collectedAmount))
            {
                RemainingAmountText.Text =
                    $"{sellingPrice:N2} ج.م";

                return;
            }


            decimal remaining =
                sellingPrice - collectedAmount;


            if (remaining < 0)
                remaining = 0;


            RemainingAmountText.Text =
                $"{remaining:N2} ج.م";
        }


        // =========================================================
        // SELL BUTTON
        // =========================================================

        private void SellButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // =====================================================
                // SELLING PRICE
                // =====================================================

                if (!decimal.TryParse(
                        SellingPriceTextBox.Text.Trim(),
                        out decimal sellingPrice))
                {
                    MessageBox.Show(
                        "من فضلك أدخل سعر بيع صحيح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    SellingPriceTextBox.Focus();

                    return;
                }


                if (sellingPrice <= 0)
                {
                    MessageBox.Show(
                        "سعر البيع يجب أن يكون أكبر من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // =====================================================
                // COLLECTED AMOUNT
                // =====================================================

                if (!decimal.TryParse(
                        CollectedAmountTextBox.Text.Trim(),
                        out decimal collectedAmount))
                {
                    MessageBox.Show(
                        "من فضلك أدخل المبلغ المحصل بشكل صحيح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CollectedAmountTextBox.Focus();

                    return;
                }


                if (collectedAmount < 0)
                {
                    MessageBox.Show(
                        "المبلغ المحصل لا يمكن أن يكون أقل من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (collectedAmount > sellingPrice)
                {
                    MessageBox.Show(
                        "المبلغ المحصل لا يمكن أن يكون أكبر من سعر البيع.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CollectedAmountTextBox.Focus();

                    return;
                }


                // =====================================================
                // CALCULATE CREDIT
                // =====================================================

                decimal creditAmount =
                    sellingPrice - collectedAmount;


                // =====================================================
                // PAYMENT TYPE
                // =====================================================

                bool isFullPayment =
                    FullPaymentRadio.IsChecked == true;


                if (isFullPayment)
                {
                    collectedAmount = sellingPrice;
                    creditAmount = 0;
                }


                // =====================================================
                // CUSTOMER
                // =====================================================

                string customerName =
                    CustomerNameTextBox.Text.Trim();

                string customerPhone =
                    CustomerPhoneTextBox.Text.Trim();


                // الاسم مطلوب في البيع الآجل
                if (creditAmount > 0 &&
                    string.IsNullOrWhiteSpace(customerName))
                {
                    MessageBox.Show(
                        "لأن هناك مبلغًا آجلًا، يجب إدخال اسم العميل.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CustomerNameTextBox.Focus();

                    return;
                }


                // =====================================================
                // PAYMENT METHOD
                // =====================================================

                string paymentMethod = "نقدي";


                if (PaymentMethodComboBox.SelectedItem
                    is ComboBoxItem selectedItem)
                {
                    paymentMethod =
                        selectedItem.Content?.ToString()
                        ?? "نقدي";
                }


                // =====================================================
                // NOTES
                // =====================================================

                string notes =
                    NotesTextBox.Text.Trim();


                // =====================================================
                // CONFIRMATION
                // =====================================================

                string paymentTypeText =
                    creditAmount > 0
                        ? "دفعة آجل"
                        : "دفعة كاملة";


                string message =
                    "هل أنت متأكد من بيع الجهاز؟\n\n" +
                    $"الجهاز: {DeviceNameText.Text}\n" +
                    $"العميل: {customerName}\n" +
                    $"رقم العميل: {customerPhone}\n" +
                    $"نوع السداد: {paymentTypeText}\n" +
                    $"سعر البيع: {sellingPrice:N2} ج.م\n" +
                    $"المحصل الآن: {collectedAmount:N2} ج.م\n" +
                    $"المتبقي آجل: {creditAmount:N2} ج.م\n" +
                    $"طريقة الدفع: {paymentMethod}";


                MessageBoxResult result =
                    MessageBox.Show(
                        message,
                        "تأكيد البيع",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);


                if (result != MessageBoxResult.Yes)
                    return;


                // =====================================================
                // DATABASE CONNECTION
                // =====================================================

                using SqlConnection connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using SqlTransaction transaction =
                    connection.BeginTransaction();


                try
                {
                    // =================================================
                    // 1 - GET PURCHASE PRICE
                    // =================================================

                    decimal purchasePrice;


                    using SqlCommand purchaseCommand =
                        new SqlCommand(
                            @"
                            SELECT
                                ISNULL(PurchasePrice, 0)
                            FROM dbo.Devices
                            WHERE DeviceID = @DeviceID
                              AND ISNULL(IsDeleted, 0) = 0
                              AND DeviceStatus = N'متاح';
                            ",
                            connection,
                            transaction);


                    purchaseCommand.Parameters.Add(
                        "@DeviceID",
                        SqlDbType.Int).Value = deviceId;


                    object purchaseResult =
                        purchaseCommand.ExecuteScalar();


                    if (purchaseResult == null ||
                        purchaseResult == DBNull.Value)
                    {
                        throw new Exception(
                            "الجهاز غير موجود أو تم بيعه بالفعل.");
                    }


                    purchasePrice =
                        Convert.ToDecimal(
                            purchaseResult);


                    // =================================================
                    // 2 - GENERATE INVOICE NUMBER
                    // =================================================

                    string invoiceNumber;


                    using SqlCommand invoiceCommand =
                        new SqlCommand(
                            @"
                            DECLARE @NextNumber INT;

                            SELECT
                                @NextNumber =
                                    ISNULL(
                                        MAX(DeviceSaleID),
                                        0
                                    ) + 1
                            FROM dbo.DeviceSales;

                            SELECT
                                'DEV-' +
                                RIGHT(
                                    '000000' +
                                    CAST(
                                        @NextNumber
                                        AS NVARCHAR(20)
                                    ),
                                    6
                                );
                            ",
                            connection,
                            transaction);


                    invoiceNumber =
                        invoiceCommand
                            .ExecuteScalar()
                            ?.ToString();


                    if (string.IsNullOrWhiteSpace(
                        invoiceNumber))
                    {
                        throw new Exception(
                            "تعذر إنشاء رقم الفاتورة.");
                    }


                    // =================================================
                    // 3 - INSERT DEVICE SALE
                    //
                    // Total = المبلغ المحصل فعليًا
                    // Profit = المحصل - سعر الشراء
                    //
                    // المتبقي الآجل لا يدخل الإيراد الآن.
                    // =================================================

                    int deviceSaleId;


                    string insertSaleQuery =
                        @"
                        INSERT INTO dbo.DeviceSales
                        (
                            InvoiceNumber,
                            DeviceID,
                            CustomerName,
                            CustomerPhone,
                            SaleDate,
                            PurchasePrice,
                            SellingPrice,
                            Discount,
                            Total,
                            Change,
                            Profit,
                            PaymentMethod,
                            VodafoneCashNumber,
                            Notes,
                            CreatedBy
                        )
                        VALUES
                        (
                            @InvoiceNumber,
                            @DeviceID,
                            @CustomerName,
                            @CustomerPhone,
                            GETDATE(),
                            @PurchasePrice,
                            @SellingPrice,
                            0,
                            @CollectedAmount,
                            0,
                            @CollectedAmount - @PurchasePrice,
                            @PaymentMethod,
                            NULL,
                            @Notes,
                            NULL
                        );

                        SELECT CAST(
                            SCOPE_IDENTITY()
                            AS INT
                        );
                        ";


                    using SqlCommand insertSaleCommand =
                        new SqlCommand(
                            insertSaleQuery,
                            connection,
                            transaction);


                    insertSaleCommand.Parameters.Add(
                        "@InvoiceNumber",
                        SqlDbType.NVarChar,
                        50).Value =
                        invoiceNumber;


                    insertSaleCommand.Parameters.Add(
                        "@DeviceID",
                        SqlDbType.Int).Value =
                        deviceId;


                    insertSaleCommand.Parameters.Add(
                        "@CustomerName",
                        SqlDbType.NVarChar,
                        200).Value =
                        string.IsNullOrWhiteSpace(customerName)
                            ? DBNull.Value
                            : customerName;


                    insertSaleCommand.Parameters.Add(
                        "@CustomerPhone",
                        SqlDbType.NVarChar,
                        50).Value =
                        string.IsNullOrWhiteSpace(customerPhone)
                            ? DBNull.Value
                            : customerPhone;


                    AddDecimalParameter(
                        insertSaleCommand,
                        "@PurchasePrice",
                        purchasePrice);


                    AddDecimalParameter(
                        insertSaleCommand,
                        "@SellingPrice",
                        sellingPrice);


                    AddDecimalParameter(
                        insertSaleCommand,
                        "@CollectedAmount",
                        collectedAmount);


                    insertSaleCommand.Parameters.Add(
                        "@PaymentMethod",
                        SqlDbType.NVarChar,
                        50).Value =
                        paymentMethod;


                    insertSaleCommand.Parameters.Add(
                        "@Notes",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(notes)
                            ? DBNull.Value
                            : notes;


                    deviceSaleId =
                        Convert.ToInt32(
                            insertSaleCommand.ExecuteScalar());


                    // =================================================
                    // 4 - UPDATE DEVICE
                    // =================================================

                    using SqlCommand updateDeviceCommand =
                        new SqlCommand(
                            @"
                            UPDATE dbo.Devices
                            SET
                                DeviceStatus = N'مباع',
                                SoldDate = GETDATE(),
                                CustomerName = @CustomerName,
                                CustomerPhone = @CustomerPhone,
                                SellingPrice = @SellingPrice
                            WHERE DeviceID = @DeviceID;
                            ",
                            connection,
                            transaction);


                    updateDeviceCommand.Parameters.Add(
                        "@DeviceID",
                        SqlDbType.Int).Value =
                        deviceId;


                    updateDeviceCommand.Parameters.Add(
                        "@CustomerName",
                        SqlDbType.NVarChar,
                        200).Value =
                        string.IsNullOrWhiteSpace(customerName)
                            ? DBNull.Value
                            : customerName;


                    updateDeviceCommand.Parameters.Add(
                        "@CustomerPhone",
                        SqlDbType.NVarChar,
                        50).Value =
                        string.IsNullOrWhiteSpace(customerPhone)
                            ? DBNull.Value
                            : customerPhone;


                    AddDecimalParameter(
                        updateDeviceCommand,
                        "@SellingPrice",
                        sellingPrice);


                    updateDeviceCommand.ExecuteNonQuery();


                    // =================================================
                    // 5 - CREATE CREDIT
                    // =================================================

                    if (creditAmount > 0)
                    {
                        int creditAccountId;


                        // =============================================
                        // SEARCH EXISTING ACCOUNT
                        // =============================================

                        string searchAccountQuery =
                            @"
                            SELECT TOP 1
                                CreditAccountID
                            FROM dbo.CreditAccounts
                            WHERE IsActive = 1
                              AND
                              (
                                  (
                                      @CustomerPhone IS NOT NULL
                                      AND CustomerPhone =
                                          @CustomerPhone
                                  )
                                  OR
                                  (
                                      @CustomerPhone IS NULL
                                      AND CustomerName =
                                          @CustomerName
                                  )
                              )
                            ORDER BY CreditAccountID;
                            ";


                        using SqlCommand searchAccountCommand =
                            new SqlCommand(
                                searchAccountQuery,
                                connection,
                                transaction);


                        searchAccountCommand.Parameters.Add(
                            "@CustomerName",
                            SqlDbType.NVarChar,
                            200).Value =
                            customerName;


                        searchAccountCommand.Parameters.Add(
                            "@CustomerPhone",
                            SqlDbType.NVarChar,
                            50).Value =
                            string.IsNullOrWhiteSpace(customerPhone)
                                ? DBNull.Value
                                : customerPhone;


                        object accountResult =
                            searchAccountCommand.ExecuteScalar();


                        // =============================================
                        // EXISTING ACCOUNT
                        // =============================================

                        if (accountResult != null &&
                            accountResult != DBNull.Value)
                        {
                            creditAccountId =
                                Convert.ToInt32(accountResult);


                            using SqlCommand updateAccountCommand =
                                new SqlCommand(
                                    @"
                                    UPDATE dbo.CreditAccounts
                                    SET
                                        CustomerName =
                                            @CustomerName,
                                        CustomerPhone =
                                            @CustomerPhone,
                                        UpdatedAt =
                                            GETDATE()
                                    WHERE CreditAccountID =
                                          @CreditAccountID;
                                    ",
                                    connection,
                                    transaction);


                            updateAccountCommand.Parameters.Add(
                                "@CustomerName",
                                SqlDbType.NVarChar,
                                200).Value =
                                customerName;


                            updateAccountCommand.Parameters.Add(
                                "@CustomerPhone",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(customerPhone)
                                    ? DBNull.Value
                                    : customerPhone;


                            updateAccountCommand.Parameters.Add(
                                "@CreditAccountID",
                                SqlDbType.Int).Value =
                                creditAccountId;


                            updateAccountCommand.ExecuteNonQuery();
                        }

                        // =============================================
                        // NEW ACCOUNT
                        // =============================================

                        else
                        {
                            using SqlCommand createAccountCommand =
                                new SqlCommand(
                                    @"
                                    INSERT INTO dbo.CreditAccounts
                                    (
                                        CustomerName,
                                        CustomerPhone,
                                        IsActive,
                                        CreatedAt,
                                        UpdatedAt
                                    )
                                    VALUES
                                    (
                                        @CustomerName,
                                        @CustomerPhone,
                                        1,
                                        GETDATE(),
                                        GETDATE()
                                    );

                                    SELECT CAST(
                                        SCOPE_IDENTITY()
                                        AS INT
                                    );
                                    ",
                                    connection,
                                    transaction);


                            createAccountCommand.Parameters.Add(
                                "@CustomerName",
                                SqlDbType.NVarChar,
                                200).Value =
                                customerName;


                            createAccountCommand.Parameters.Add(
                                "@CustomerPhone",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(customerPhone)
                                    ? DBNull.Value
                                    : customerPhone;


                            creditAccountId =
                                Convert.ToInt32(
                                    createAccountCommand
                                        .ExecuteScalar());
                        }


                        // =============================================
                        // INSERT CREDIT TRANSACTION
                        // =============================================

                        using SqlCommand creditCommand =
                            new SqlCommand(
                                @"
                                INSERT INTO dbo.CreditTransactions
                                (
                                    CreditAccountID,
                                    TransactionType,
                                    Amount,
                                    TransactionDate,
                                    PaymentMethod,
                                    Notes,
                                    SaleID,
                                    DeviceSaleID,
                                    CreatedAt
                                )
                                VALUES
                                (
                                    @CreditAccountID,
                                    N'Credit',
                                    @Amount,
                                    GETDATE(),
                                    NULL,
                                    @Notes,
                                    NULL,
                                    @DeviceSaleID,
                                    GETDATE()
                                );
                                ",
                                connection,
                                transaction);


                        creditCommand.Parameters.Add(
                            "@CreditAccountID",
                            SqlDbType.Int).Value =
                            creditAccountId;


                        AddDecimalParameter(
                            creditCommand,
                            "@Amount",
                            creditAmount);


                        creditCommand.Parameters.Add(
                            "@Notes",
                            SqlDbType.NVarChar,
                            500).Value =
                            string.IsNullOrWhiteSpace(notes)
                                ? DBNull.Value
                                : notes;


                        creditCommand.Parameters.Add(
                            "@DeviceSaleID",
                            SqlDbType.Int).Value =
                            deviceSaleId;


                        creditCommand.ExecuteNonQuery();
                    }


                    // =================================================
                    // 6 - COMMIT
                    // =================================================

                    transaction.Commit();


                    // =================================================
                    // SUCCESS MESSAGE
                    // =================================================

                    MessageBox.Show(
                        "تم بيع الجهاز بنجاح.\n\n" +
                        $"رقم الفاتورة: {invoiceNumber}\n" +
                        $"سعر البيع: {sellingPrice:N2} ج.م\n" +
                        $"المحصل الآن: {collectedAmount:N2} ج.م\n" +
                        $"المتبقي آجل: {creditAmount:N2} ج.م",
                        "تم البيع",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                    DialogResult = true;

                    Close();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء بيع الجهاز:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // DECIMAL PARAMETER
        // =========================================================

        private static void AddDecimalParameter(
            SqlCommand command,
            string parameterName,
            decimal value)
        {
            SqlParameter parameter =
                command.Parameters.Add(
                    parameterName,
                    SqlDbType.Decimal);

            parameter.Precision = 18;
            parameter.Scale = 2;
            parameter.Value = value;
        }


        // =========================================================
        // CANCEL
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


    }
}