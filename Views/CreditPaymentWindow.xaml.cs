using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class CreditPaymentWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int accountId;

        private decimal remainingAmount;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public CreditPaymentWindow(int id)
        {
            InitializeComponent();

            accountId = id;

            LoadAccount();
        }


        // =========================================================
        // تحميل الحساب
        // =========================================================

        private void LoadAccount()
        {
            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                string query = @"
                    SELECT
                        ca.CustomerName,

                        ISNULL
                        (
                            (
                                SELECT SUM
                                (
                                    CASE
                                        WHEN ct.TransactionType = N'Credit'
                                            THEN ct.Amount

                                        WHEN ct.TransactionType = N'Payment'
                                            THEN -ct.Amount

                                        ELSE 0
                                    END
                                )
                                FROM CreditTransactions ct
                                WHERE
                                    ct.CreditAccountID =
                                    ca.CreditAccountID
                            ),
                            0
                        ) AS Remaining

                    FROM CreditAccounts ca

                    WHERE
                        ca.CreditAccountID =
                        @CreditAccountID

                        AND ca.IsActive = 1;";

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@CreditAccountID",
                    SqlDbType.Int).Value =
                    accountId;

                connection.Open();

                using SqlDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "حساب العميل غير موجود.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }

                CustomerNameText.Text =
                    reader["CustomerName"]
                        ?.ToString()
                        ?? "";

                remainingAmount =
                    reader["Remaining"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(
                            reader["Remaining"]);

                if (remainingAmount <= 0)
                {
                    MessageBox.Show(
                        "لا يوجد مبلغ مستحق على هذا العميل.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    Close();

                    return;
                }

                RemainingText.Text =
                    $"{remainingAmount:N2} ج.م";

                PaymentAmountTextBox.Text =
                    remainingAmount.ToString("0.##");

                UpdateNewRemaining();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل حساب العميل:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // تغيير نوع التسديد
        // =========================================================

        private void PaymentType_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (PaymentAmountTextBox == null)
                return;

            if (FullPaymentRadioButton.IsChecked == true)
            {
                PaymentAmountTextBox.Text =
                    remainingAmount.ToString("0.##");

                PaymentAmountTextBox.IsReadOnly =
                    true;
            }
            else
            {
                PaymentAmountTextBox.IsReadOnly =
                    false;

                PaymentAmountTextBox.SelectAll();

                PaymentAmountTextBox.Focus();
            }

            UpdateNewRemaining();
        }


        // =========================================================
        // تغير المبلغ
        // =========================================================

        private void PaymentAmountTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            UpdateNewRemaining();
        }


        // =========================================================
        // حساب المتبقي الجديد
        // =========================================================

        private void UpdateNewRemaining()
        {
            if (PaymentAmountTextBox == null ||
                NewRemainingText == null)
            {
                return;
            }

            if (!decimal.TryParse(
                    PaymentAmountTextBox.Text.Trim(),
                    out decimal payment))
            {
                NewRemainingText.Text =
                    $"المتبقي بعد التسديد: {remainingAmount:N2} ج.م";

                return;
            }

            decimal newRemaining =
                remainingAmount - payment;

            if (newRemaining < 0)
                newRemaining = 0;

            NewRemainingText.Text =
                $"المتبقي بعد التسديد: {newRemaining:N2} ج.م";
        }


        // =========================================================
        // تأكيد التسديد
        // =========================================================

        private void ConfirmPaymentButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(
                        PaymentAmountTextBox.Text.Trim(),
                        out decimal paymentAmount))
                {
                    MessageBox.Show(
                        "من فضلك أدخل مبلغًا صحيحًا.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    PaymentAmountTextBox.Focus();

                    return;
                }

                if (paymentAmount <= 0)
                {
                    MessageBox.Show(
                        "المبلغ يجب أن يكون أكبر من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (paymentAmount > remainingAmount)
                {
                    MessageBox.Show(
                        "المبلغ المدفوع أكبر من المبلغ المستحق.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string paymentMethod = "نقدي";

                if (PaymentMethodComboBox.SelectedItem
                    is ComboBoxItem item)
                {
                    paymentMethod =
                        item.Content?.ToString()
                        ?? "نقدي";
                }

                decimal newRemaining =
                    remainingAmount -
                    paymentAmount;

                MessageBoxResult confirmation =
                    MessageBox.Show(
                        "هل أنت متأكد من تسجيل التسديد؟\n\n" +
                        $"العميل: {CustomerNameText.Text}\n" +
                        $"المبلغ: {paymentAmount:N2} ج.م\n" +
                        $"المتبقي بعد التسديد: {newRemaining:N2} ج.م",
                        "تأكيد التسديد",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (confirmation !=
                    MessageBoxResult.Yes)
                {
                    return;
                }


                // =================================================
                // بدء الاتصال
                // =================================================

                using SqlConnection connection =
                    new SqlConnection(connectionString);

                connection.Open();

                using SqlTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    // =================================================
                    // 1. الحصول على حركات الـ Credit الأصلية
                    //
                    // بنجيب:
                    // SaleID       = لو الأجل من بيع عادي
                    // DeviceSaleID = لو الأجل من بيع جهاز
                    //
                    // ونبدأ بالأقدم.
                    // =================================================

                    string creditsQuery = @"
                        SELECT
                            ct.CreditTransactionID,
                            ct.Amount,

                            ISNULL(
                                ct.SaleID,
                                0
                            ) AS SaleID,

                            ISNULL(
                                ct.DeviceSaleID,
                                0
                            ) AS DeviceSaleID,

                            ISNULL
                            (
                                (
                                    SELECT SUM(p.Amount)
                                    FROM CreditTransactions p
                                    WHERE
                                        p.TransactionType =
                                            N'Payment'

                                        AND
                                        p.ParentCreditTransactionID =
                                            ct.CreditTransactionID
                                ),
                                0
                            ) AS PaidAmount

                        FROM CreditTransactions ct
                            WITH (UPDLOCK, ROWLOCK)

                        WHERE
                            ct.CreditAccountID =
                                @CreditAccountID

                            AND
                            ct.TransactionType =
                                N'Credit'

                        ORDER BY
                            ct.TransactionDate ASC,
                            ct.CreditTransactionID ASC;";


                    List<CreditSource> creditSources =
                        new List<CreditSource>();


                    using (SqlCommand creditsCommand =
                        new SqlCommand(
                            creditsQuery,
                            connection,
                            transaction))
                    {
                        creditsCommand.Parameters.Add(
                            "@CreditAccountID",
                            SqlDbType.Int).Value =
                            accountId;


                        using SqlDataReader reader =
                            creditsCommand.ExecuteReader();


                        while (reader.Read())
                        {
                            int creditTransactionId =
                                Convert.ToInt32(
                                    reader[
                                        "CreditTransactionID"]);


                            decimal creditAmount =
                                Convert.ToDecimal(
                                    reader["Amount"]);


                            decimal paidAmount =
                                Convert.ToDecimal(
                                    reader["PaidAmount"]);


                            int saleId =
                                Convert.ToInt32(
                                    reader["SaleID"]);


                            int deviceSaleId =
                                Convert.ToInt32(
                                    reader["DeviceSaleID"]);


                            decimal outstanding =
                                creditAmount -
                                paidAmount;


                            if (outstanding > 0)
                            {
                                creditSources.Add(
                                    new CreditSource
                                    {
                                        CreditTransactionID =
                                            creditTransactionId,

                                        SaleID =
                                            saleId == 0
                                                ? (int?)null
                                                : saleId,

                                        DeviceSaleID =
                                            deviceSaleId == 0
                                                ? (int?)null
                                                : deviceSaleId,

                                        OutstandingAmount =
                                            outstanding
                                    });
                            }
                        }
                    }


                    // =================================================
                    // 2. التأكد أن عندنا حركات كافية
                    // =================================================

                    decimal availableOutstanding =
                        0m;

                    foreach (CreditSource source
                             in creditSources)
                    {
                        availableOutstanding +=
                            source.OutstandingAmount;
                    }


                    if (availableOutstanding <
                        paymentAmount)
                    {
                        throw new Exception(
                            "تعذر تحديد الحركات الأصلية التي سيتم تسديدها.\n" +
                            "يرجى تحديث شاشة الأجل والمحاولة مرة أخرى.");
                    }


                    // =================================================
                    // 3. توزيع الدفعة على الـ Credit Transactions
                    // =================================================

                    decimal remainingToAllocate =
                        paymentAmount;


                    foreach (CreditSource source
                             in creditSources)
                    {
                        if (remainingToAllocate <= 0)
                            break;


                        decimal allocation =
                            Math.Min(
                                remainingToAllocate,
                                source.OutstandingAmount);


                        if (allocation <= 0)
                            continue;


                        // =================================================
                        // تحديد الـ Notes تلقائيًا
                        // =================================================

                        string noteText =
                            string.IsNullOrWhiteSpace(
                                NotesTextBox.Text)
                                ? null
                                : NotesTextBox.Text.Trim();


                        if (source.DeviceSaleID.HasValue)
                        {
                            string deviceNote =
                                $"تسديد أجل بيع جهاز - " +
                                $"DEV-{source.DeviceSaleID.Value:000000}";


                            if (!string.IsNullOrWhiteSpace(
                                    noteText))
                            {
                                deviceNote +=
                                    $" - {noteText}";
                            }


                            noteText =
                                deviceNote;
                        }
                        else if (source.SaleID.HasValue)
                        {
                            string saleNote =
                                $"تسديد أجل فاتورة مبيعات - " +
                                $"INV-{source.SaleID.Value:000000}";


                            if (!string.IsNullOrWhiteSpace(
                                    noteText))
                            {
                                saleNote +=
                                    $" - {noteText}";
                            }


                            noteText =
                                saleNote;
                        }


                        // =================================================
                        // 4. إدخال حركة Payment
                        //
                        // هنا أهم تعديل:
                        //
                        // SaleID       ← نفس SaleID الأصلي
                        // DeviceSaleID ← نفس DeviceSaleID الأصلي
                        //
                        // وبالتالي النظام يعرف مصدر الإيراد
                        // تلقائيًا بدون أي اختيار من المستخدم.
                        // =================================================

                        string insertPaymentQuery = @"
                            INSERT INTO CreditTransactions
                            (
                                CreditAccountID,
                                TransactionType,
                                Amount,
                                TransactionDate,
                                PaymentMethod,
                                Notes,
                                SaleID,
                                CreatedAt,
                                DeviceSaleID,
                                ParentCreditTransactionID
                            )
                            VALUES
                            (
                                @CreditAccountID,
                                N'Payment',
                                @Amount,
                                GETDATE(),
                                @PaymentMethod,
                                @Notes,
                                @SaleID,
                                GETDATE(),
                                @DeviceSaleID,
                                @ParentCreditTransactionID
                            );";


                        using SqlCommand insertCommand =
                            new SqlCommand(
                                insertPaymentQuery,
                                connection,
                                transaction);


                        insertCommand.Parameters.Add(
                            "@CreditAccountID",
                            SqlDbType.Int).Value =
                            accountId;


                        SqlParameter amountParameter =
                            insertCommand.Parameters.Add(
                                "@Amount",
                                SqlDbType.Decimal);

                        amountParameter.Precision =
                            18;

                        amountParameter.Scale =
                            2;

                        amountParameter.Value =
                            allocation;


                        insertCommand.Parameters.Add(
                            "@PaymentMethod",
                            SqlDbType.NVarChar,
                            50).Value =
                            paymentMethod;


                        insertCommand.Parameters.Add(
                            "@Notes",
                            SqlDbType.NVarChar,
                            500).Value =
                            string.IsNullOrWhiteSpace(
                                noteText)
                                ? DBNull.Value
                                : noteText;


                        // =================================================
                        // SaleID
                        // =================================================

                        insertCommand.Parameters.Add(
                            "@SaleID",
                            SqlDbType.Int).Value =
                            source.SaleID.HasValue
                                ? source.SaleID.Value
                                : DBNull.Value;


                        // =================================================
                        // DeviceSaleID
                        // =================================================

                        insertCommand.Parameters.Add(
                            "@DeviceSaleID",
                            SqlDbType.Int).Value =
                            source.DeviceSaleID.HasValue
                                ? source.DeviceSaleID.Value
                                : DBNull.Value;


                        // =================================================
                        // ربط الدفعة بالأجل الأصلي
                        // =================================================

                        insertCommand.Parameters.Add(
                            "@ParentCreditTransactionID",
                            SqlDbType.Int).Value =
                            source.CreditTransactionID;


                        insertCommand.ExecuteNonQuery();


                        remainingToAllocate -=
                            allocation;
                    }


                    // =================================================
                    // 5. تحديث حساب العميل
                    // =================================================

                    using SqlCommand updateCommand =
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


                    updateCommand.Parameters.Add(
                        "@CreditAccountID",
                        SqlDbType.Int).Value =
                        accountId;


                    updateCommand.ExecuteNonQuery();


                    // =================================================
                    // 6. إنهاء Transaction
                    // =================================================

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }


                // =================================================
                // نجاح
                // =================================================

                MessageBox.Show(
                    newRemaining <= 0
                        ? "تم تسديد الآجل بالكامل."
                        : "تم تسجيل التسديد بنجاح.",
                    "تم التسديد",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                DialogResult =
                    true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تسجيل التسديد:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إلغاء
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // =========================================================
        // MODEL داخلي لحركة الأجل الأصلية
        // =========================================================

        private class CreditSource
        {
            public int CreditTransactionID
            {
                get;
                set;
            }

            public int? SaleID
            {
                get;
                set;
            }

            public int? DeviceSaleID
            {
                get;
                set;
            }

            public decimal OutstandingAmount
            {
                get;
                set;
            }
        }
    }
}