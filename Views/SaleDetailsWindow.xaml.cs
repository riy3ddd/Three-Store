using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class SaleDetailsWindow : Window
    {
        // =========================================================
        // بيانات الفاتورة
        // =========================================================

        public decimal InvoiceTotal { get; private set; }

        public bool IsConfirmed { get; private set; }

        public bool IsCredit { get; private set; }

        public string PaymentType =>
            IsCredit ? "آجل" : "كامل";

        public decimal PaidAmount { get; private set; }

        public decimal CreditAmount { get; private set; }

        public string PaymentMethod { get; private set; }
            = "نقدي";

        public string CustomerName { get; private set; }
            = "";

        public string CustomerPhone { get; private set; }
            = "";

        public string VodafoneCashNumber { get; private set; }
            = "";


        // =========================================================
        // Constructor
        // =========================================================

        public SaleDetailsWindow()
            : this(0m)
        {
        }


        public SaleDetailsWindow(
            decimal invoiceTotal)
        {
            InitializeComponent();

            InvoiceTotal =
                invoiceTotal;

            InvoiceTotalTextBlock.Text =
                $"{invoiceTotal:N2} ج.م";

            PaidAmountTextBox.Text =
                invoiceTotal.ToString("0.##");

            UpdateCreditUI();
        }


        // =========================================================
        // نوع السداد
        // =========================================================

        private void PaymentType_Checked(
            object sender,
            RoutedEventArgs e)
        {
            UpdateCreditUI();
        }


        private void UpdateCreditUI()
        {
            if (CreditPaymentRadioButton == null ||
                FullPaymentRadioButton == null)
            {
                return;
            }


            if (CreditPaymentRadioButton.IsChecked == true)
            {
                CreditPanel.Visibility =
                    Visibility.Visible;

                PaidAmountTextBox.IsReadOnly =
                    false;

                PaidAmountTextBox.SelectAll();

                UpdateCreditAmount();
            }
            else
            {
                CreditPanel.Visibility =
                    Visibility.Collapsed;

                PaidAmountTextBox.IsReadOnly =
                    true;

                PaidAmountTextBox.Text =
                    InvoiceTotal.ToString("0.##");

                CreditRemainingTextBlock.Text =
                    "الآجل: 0.00 ج.م";

                CustomerNameRequiredText.Visibility =
                    Visibility.Collapsed;
            }
        }


        // =========================================================
        // تغيير المدفوع
        // =========================================================

        private void PaidAmountTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            UpdateCreditAmount();
        }


        private void UpdateCreditAmount()
        {
            if (!decimal.TryParse(
                    PaidAmountTextBox.Text.Trim(),
                    out decimal paid))
            {
                CreditRemainingTextBlock.Text =
                    $"الآجل: {InvoiceTotal:N2} ج.م";

                return;
            }


            if (paid < 0)
                paid = 0;


            decimal remaining =
                InvoiceTotal - paid;


            if (remaining < 0)
                remaining = 0;


            CreditRemainingTextBlock.Text =
                $"الآجل: {remaining:N2} ج.م";
        }


        // =========================================================
        // طريقة الدفع
        // =========================================================

        private void PaymentMethodComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;


            string paymentMethod =
                "نقدي";


            if (PaymentMethodComboBox.SelectedItem
                is ComboBoxItem item)
            {
                paymentMethod =
                    item.Content?.ToString()
                    ?? "نقدي";
            }


            if (paymentMethod ==
                "فودافون كاش")
            {
                VodafoneCashNumberTextBox.Visibility =
                    Visibility.Visible;

                VodafoneCashNumberTextBox.ToolTip =
                    "رقم فودافون كاش";

                VodafoneCashNumberTextBox.Focus();
            }
            else
            {
                VodafoneCashNumberTextBox.Visibility =
                    Visibility.Collapsed;

                VodafoneCashNumberTextBox.Text =
                    "";
            }
        }


        // =========================================================
        // تأكيد
        // =========================================================

        private void ConfirmButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                IsCredit =
                    CreditPaymentRadioButton.IsChecked
                    == true;


                // =================================================
                // اسم العميل في الآجل إجباري
                // =================================================

                CustomerName =
                    CustomerNameTextBox.Text.Trim();


                CustomerPhone =
                    CustomerPhoneTextBox.Text.Trim();


                if (IsCredit &&
                    string.IsNullOrWhiteSpace(
                        CustomerName))
                {
                    CustomerNameRequiredText.Visibility =
                        Visibility.Visible;

                    CustomerNameTextBox.Focus();

                    return;
                }


                CustomerNameRequiredText.Visibility =
                    Visibility.Collapsed;


                // =================================================
                // طريقة الدفع
                // =================================================

                PaymentMethod =
                    "نقدي";


                if (PaymentMethodComboBox.SelectedItem
                    is ComboBoxItem paymentItem)
                {
                    PaymentMethod =
                        paymentItem.Content?.ToString()
                        ?? "نقدي";
                }


                // =================================================
                // Vodafone Cash
                // =================================================

                VodafoneCashNumber =
                    VodafoneCashNumberTextBox.Text.Trim();


                if (PaymentMethod ==
                    "فودافون كاش" &&
                    string.IsNullOrWhiteSpace(
                        VodafoneCashNumber))
                {
                    MessageBox.Show(
                        "من فضلك أدخل رقم فودافون كاش.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    VodafoneCashNumberTextBox.Focus();

                    return;
                }


                // =================================================
                // الدفع الكامل
                // =================================================

                if (!IsCredit)
                {
                    PaidAmount =
                        InvoiceTotal;

                    CreditAmount =
                        0m;

                    IsConfirmed =
                        true;

                    DialogResult =
                        true;

                    Close();

                    return;
                }


                // =================================================
                // الدفع الآجل
                // =================================================

                if (!decimal.TryParse(
                        PaidAmountTextBox.Text.Trim(),
                        out decimal paidAmount))
                {
                    MessageBox.Show(
                        "من فضلك أدخل قيمة المدفوع الآن.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    PaidAmountTextBox.Focus();

                    return;
                }


                if (paidAmount < 0)
                {
                    MessageBox.Show(
                        "المبلغ لا يمكن أن يكون أقل من صفر.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (paidAmount >= InvoiceTotal)
                {
                    MessageBox.Show(
                        "عند اختيار الآجل يجب أن يكون المدفوع أقل من إجمالي الفاتورة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                PaidAmount =
                    paidAmount;


                CreditAmount =
                    InvoiceTotal -
                    paidAmount;


                if (CreditAmount <= 0)
                {
                    MessageBox.Show(
                        "لا يوجد مبلغ آجل متبقي.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                MessageBoxResult confirmation =
                    MessageBox.Show(
                        "هل أنت متأكد من إتمام البيع؟\n\n" +
                        $"الإجمالي: {InvoiceTotal:N2} ج.م\n" +
                        $"المدفوع الآن: {PaidAmount:N2} ج.م\n" +
                        $"الآجل: {CreditAmount:N2} ج.م",
                        "تأكيد البيع",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);


                if (confirmation !=
                    MessageBoxResult.Yes)
                {
                    return;
                }


                IsConfirmed =
                    true;

                DialogResult =
                    true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تجهيز بيانات الدفع:\n\n" +
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
            IsConfirmed =
                false;

            DialogResult =
                false;

            Close();
        }
    }
}