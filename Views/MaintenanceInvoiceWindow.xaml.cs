using System;
using System.Data;
using System.Globalization;
using System.Printing;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MaintenanceInvoiceWindow : Window
    {
        private readonly int maintenanceId;

        private bool _isLoading = false;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MaintenanceInvoiceWindow(int maintenanceId)
        {
            InitializeComponent();

            this.maintenanceId = maintenanceId;

            LoadInvoice();
        }


        // =========================================================
        // LOAD INVOICE
        // =========================================================

        private void LoadInvoice()
        {
            try
            {
                _isLoading = true;


                DataTable table =
                    DatabaseHelper.GetMaintenanceInvoice(maintenanceId);


                if (table == null || table.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "الفاتورة غير موجودة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();

                    return;
                }


                DataRow row = table.Rows[0];


                // =================================================
                // BASIC DATA
                // =================================================

                InvoiceNumberTextBox.Text =
                    GetColumnValue(row, "InvoiceNumber");


                HeaderInvoiceNumberText.Text =
                    GetColumnValue(row, "InvoiceNumber");


                CustomerNameTextBox.Text =
                    GetColumnValue(row, "CustomerName");


                CustomerPhoneTextBox.Text =
                    GetColumnValue(row, "CustomerPhone");


                DeviceNameTextBox.Text =
                    GetColumnValue(row, "DeviceName");


                SerialNumberTextBox.Text =
                    GetColumnValue(row, "SerialNumber");


                DeviceConditionTextBox.Text =
                    GetColumnValue(row, "DeviceCondition");


                DeviceColorTextBox.Text =
                    GetColumnValue(row, "DeviceColor");


                ProblemDescriptionTextBox.Text =
                    GetColumnValue(row, "ProblemDescription");


                AccessoriesTextBox.Text =
                    GetColumnValue(row, "Accessories");


                NotesTextBox.Text =
                    GetColumnValue(row, "Notes");


                UsernameTextBox.Text =
                    GetColumnValue(row, "Username");


                // =================================================
                // RECEIVE DATE
                // =================================================

                ReceiveDateTextBox.Text =
                    FormatDate(row["ReceiveDate"]);


                // =================================================
                // MAINTENANCE COST
                // =================================================

                decimal maintenanceCost = 0;


                if (HasColumn(row, "MaintenanceCost"))
                {
                    maintenanceCost =
                        GetDecimalValue(
                            row,
                            "MaintenanceCost");
                }


                // =================================================
                // SPARE PART
                // =================================================

                string sparePartName = "";

                decimal sparePartsCost = 0;


                // -------------------------------------------------
                // اسم قطعة الغيار
                // -------------------------------------------------

                if (HasColumn(row, "SparePartsName"))
                {
                    sparePartName =
                        GetColumnValue(
                            row,
                            "SparePartsName");
                }
                else if (HasColumn(row, "SparePartName"))
                {
                    sparePartName =
                        GetColumnValue(
                            row,
                            "SparePartName");
                }


                // -------------------------------------------------
                // سعر قطعة الغيار
                // -------------------------------------------------

                if (HasColumn(row, "SparePartsCost"))
                {
                    sparePartsCost =
                        GetDecimalValue(
                            row,
                            "SparePartsCost");
                }
                else if (HasColumn(row, "SparePartCost"))
                {
                    sparePartsCost =
                        GetDecimalValue(
                            row,
                            "SparePartCost");
                }


                // =================================================
                // FALLBACK
                // =================================================

                // لو MaintenanceCost مش موجود في نتيجة الاستعلام
                // نستخدم Price ونطرح منه سعر قطعة الغيار.

                if (!HasColumn(row, "MaintenanceCost"))
                {
                    decimal oldPrice =
                        GetDecimalValue(
                            row,
                            "Price");


                    maintenanceCost =
                        oldPrice - sparePartsCost;


                    if (maintenanceCost < 0)
                    {
                        maintenanceCost = 0;
                    }
                }


                // =================================================
                // SHOW MAINTENANCE COST
                // =================================================

                MaintenanceCostTextBox.Text =
                    maintenanceCost.ToString("0.00");


                // =================================================
                // SHOW SPARE PART COST
                // =================================================

                SparePartsCostSummaryText.Text =
                    sparePartsCost.ToString("0.00") +
                    " جنيه";


                // =================================================
                // CALCULATE TOTAL
                // =================================================

                decimal total =
                    maintenanceCost +
                    sparePartsCost;


                TotalMaintenanceCostText.Text =
                    total.ToString("0.00") +
                    " جنيه";


                PriceTextBox.Text =
                    total.ToString("0.00");


                // =================================================
                // SPARE PART UI
                // =================================================

                // لو فيه قطعة غيار محفوظة بالفعل
                if (!string.IsNullOrWhiteSpace(sparePartName) ||
                    sparePartsCost > 0)
                {
                    AddSparePartCheckBox.IsChecked =
                        true;


                    SparePartPanel.Visibility =
                        Visibility.Visible;


                    SparePartsNameTextBox.Text =
                        sparePartName;


                    SparePartsCostTextBox.Text =
                        sparePartsCost.ToString("0.00");
                }
                else
                {
                    // مفيش قطعة غيار

                    AddSparePartCheckBox.IsChecked =
                        false;


                    SparePartPanel.Visibility =
                        Visibility.Collapsed;


                    SparePartsNameTextBox.Text =
                        "";


                    SparePartsCostTextBox.Text =
                        "0";
                }


                // =================================================
                // COMBOBOXES
                // =================================================

                SetComboBoxValue(
                    MaintenanceTypeComboBox,
                    GetColumnValue(
                        row,
                        "MaintenanceType"));


                SetComboBoxValue(
                    StatusComboBox,
                    GetColumnValue(
                        row,
                        "Status"));


                // =================================================
                // DELIVERY DATE
                // =================================================

                DeliveryDatePicker.SelectedDate =
                    null;


                if (HasColumn(row, "DeliveryDate") &&
                    row["DeliveryDate"] != DBNull.Value)
                {
                    if (DateTime.TryParse(
                        row["DeliveryDate"].ToString(),
                        out DateTime deliveryDate))
                    {
                        DeliveryDatePicker.SelectedDate =
                            deliveryDate;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل الفاتورة:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;

                UpdateMaintenanceTotal();
            }
        }


        // =========================================================
        // UPDATE TOTAL
        // =========================================================

        private void UpdateMaintenanceTotal()
        {
            if (MaintenanceCostTextBox == null ||
                SparePartsCostSummaryText == null ||
                TotalMaintenanceCostText == null ||
                PriceTextBox == null)
            {
                return;
            }


            decimal maintenanceCost =
                GetMaintenanceCost();


            decimal sparePartsCost =
                GetSparePartsCost();


            decimal total =
                maintenanceCost +
                sparePartsCost;


            // =====================================================
            // MAINTENANCE COST
            // =====================================================

            // هنا لا نعيد كتابة MaintenanceCostTextBox
            // لأنها أصبحت حقل إدخال قابل للتعديل.


            // =====================================================
            // SPARE PART COST
            // =====================================================

            SparePartsCostSummaryText.Text =
                sparePartsCost.ToString("0.00") +
                " جنيه";


            // =====================================================
            // TOTAL
            // =====================================================

            TotalMaintenanceCostText.Text =
                total.ToString("0.00") +
                " جنيه";


            // السعر النهائي = التكلفة التقديرية + قطعة الغيار

            PriceTextBox.Text =
                total.ToString("0.00");
        }


        // =========================================================
        // GET MAINTENANCE COST
        // =========================================================

        private decimal GetMaintenanceCost()
        {
            if (MaintenanceCostTextBox == null)
            {
                return 0;
            }


            return TryParseDecimal(
                MaintenanceCostTextBox.Text,
                out decimal value)
                ? value
                : 0;
        }


        // =========================================================
        // GET SPARE PART COST
        // =========================================================

        private decimal GetSparePartsCost()
        {
            if (AddSparePartCheckBox == null ||
                SparePartsCostTextBox == null)
            {
                return 0;
            }


            // لو القطعة مش مفعلة
            // تعتبر قيمتها صفر.

            if (AddSparePartCheckBox.IsChecked != true)
            {
                return 0;
            }


            return TryParseDecimal(
                SparePartsCostTextBox.Text,
                out decimal value)
                ? value
                : 0;
        }


        // =========================================================
        // MAINTENANCE COST CHANGED
        // =========================================================

        private void MaintenanceCostTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }


            UpdateMaintenanceTotal();
        }


        // =========================================================
        // CHECKBOX - ADD SPARE PART
        // =========================================================

        private void AddSparePartCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (SparePartPanel == null)
            {
                return;
            }


            SparePartPanel.Visibility =
                Visibility.Visible;


            UpdateMaintenanceTotal();
        }


        // =========================================================
        // CHECKBOX - REMOVE SPARE PART
        // =========================================================

        private void AddSparePartCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            if (SparePartPanel == null)
            {
                return;
            }


            SparePartPanel.Visibility =
                Visibility.Collapsed;


            // أثناء تحميل الفاتورة لا نمسح البيانات.

            if (!_isLoading)
            {
                SparePartsNameTextBox.Text =
                    "";

                SparePartsCostTextBox.Text =
                    "0";
            }


            // بمجرد إزالة العلامة
            // سعر قطعة الغيار يصبح صفر.

            UpdateMaintenanceTotal();
        }


        // =========================================================
        // SPARE PART NAME CHANGED
        // =========================================================

        private void SparePartsNameTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }


            UpdateMaintenanceTotal();
        }


        // =========================================================
        // SPARE PART COST CHANGED
        // =========================================================

        private void SparePartsCostTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }


            UpdateMaintenanceTotal();
        }


        // =========================================================
        // SAVE
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // =================================================
                // VALIDATION
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    CustomerNameTextBox.Text))
                {
                    MessageBox.Show(
                        "اسم العميل مطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (string.IsNullOrWhiteSpace(
                    DeviceNameTextBox.Text))
                {
                    MessageBox.Show(
                        "اسم الجهاز مطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (MaintenanceTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "اختر نوع الصيانة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                if (StatusComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "اختر حالة الصيانة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // =================================================
                // MAINTENANCE COST
                // =================================================

                if (!TryParseDecimal(
                    MaintenanceCostTextBox.Text,
                    out decimal maintenanceCost))
                {
                    MessageBox.Show(
                        "أدخل تكلفة صيانة صحيحة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    MaintenanceCostTextBox.Focus();

                    return;
                }


                if (maintenanceCost < 0)
                {
                    MessageBox.Show(
                        "تكلفة الصيانة لا يمكن أن تكون سالبة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    MaintenanceCostTextBox.Focus();

                    return;
                }


                // =================================================
                // SPARE PART
                // =================================================

                string sparePartsName = "";

                decimal sparePartsCost = 0;


                // لو قطعة الغيار مفعلة

                if (AddSparePartCheckBox.IsChecked == true)
                {
                    sparePartsName =
                        SparePartsNameTextBox.Text.Trim();


                    if (string.IsNullOrWhiteSpace(
                        sparePartsName))
                    {
                        MessageBox.Show(
                            "اكتب اسم قطعة الغيار.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        SparePartsNameTextBox.Focus();

                        return;
                    }


                    if (!TryParseDecimal(
                        SparePartsCostTextBox.Text,
                        out sparePartsCost))
                    {
                        MessageBox.Show(
                            "سعر قطعة الغيار غير صحيح.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        SparePartsCostTextBox.Focus();

                        return;
                    }


                    if (sparePartsCost < 0)
                    {
                        MessageBox.Show(
                            "سعر قطعة الغيار لا يمكن أن يكون سالبًا.",
                            "تنبيه",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        SparePartsCostTextBox.Focus();

                        return;
                    }
                }
                else
                {
                    // =================================================
                    // NO SPARE PART
                    // =================================================

                    sparePartsName = "";

                    sparePartsCost = 0;
                }


                // =================================================
                // FINAL TOTAL
                // =================================================

                decimal totalPrice =
                    maintenanceCost +
                    sparePartsCost;


                // =================================================
                // COMBOBOX VALUES
                // =================================================

                ComboBoxItem typeItem =
                    (ComboBoxItem)
                    MaintenanceTypeComboBox.SelectedItem;


                ComboBoxItem statusItem =
                    (ComboBoxItem)
                    StatusComboBox.SelectedItem;


                string maintenanceType =
                    typeItem.Content?.ToString() ?? "";


                string status =
                    statusItem.Content?.ToString() ?? "";


                // =================================================
                // UPDATE MAIN INVOICE
                // =================================================

                DatabaseHelper.UpdateMaintenanceInvoice(
                    maintenanceId,

                    CustomerNameTextBox.Text.Trim(),

                    CustomerPhoneTextBox.Text.Trim(),

                    DeviceNameTextBox.Text.Trim(),

                    SerialNumberTextBox.Text.Trim(),

                    DeviceConditionTextBox.Text.Trim(),

                    DeviceColorTextBox.Text.Trim(),

                    ProblemDescriptionTextBox.Text.Trim(),

                    AccessoriesTextBox.Text.Trim(),

                    NotesTextBox.Text.Trim(),

                    maintenanceType,

                    totalPrice,

                    DeliveryDatePicker.SelectedDate,

                    status);


                // =================================================
                // UPDATE COST DETAILS
                // =================================================

                DatabaseHelper.UpdateMaintenanceCostDetails(
                    maintenanceId,

                    maintenanceCost,

                    sparePartsName,

                    sparePartsCost);


                // =================================================
                // SUCCESS
                // =================================================

                MessageBox.Show(
                    "تم حفظ التعديلات بنجاح.\n\n" +

                    "تكلفة الصيانة: " +
                    maintenanceCost.ToString("0.00") +
                    " جنيه\n" +

                    "قطعة الغيار: " +
                    sparePartsCost.ToString("0.00") +
                    " جنيه\n" +

                    "الإجمالي النهائي: " +
                    totalPrice.ToString("0.00") +
                    " جنيه",

                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // =================================================
                // RELOAD
                // =================================================

                LoadInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ التعديلات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // PRINT
        // =========================================================

        private void PrintButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog =
                    new PrintDialog();


                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(
                        this,
                        "Maintenance Invoice - " +
                        InvoiceNumberTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء الطباعة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // CLOSE
        // =========================================================

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // =========================================================
        // PARSE DECIMAL
        // =========================================================

        private bool TryParseDecimal(
            string? text,
            out decimal value)
        {
            value = 0;


            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }


            text = text
                .Replace("جنيه", "")
                .Replace("EGP", "")
                .Trim();


            // Invariant Culture

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value))
            {
                return true;
            }


            // Current Culture

            return decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out value);
        }


        // =========================================================
        // GET DECIMAL FROM DATAROW
        // =========================================================

        private decimal GetDecimalValue(
            DataRow row,
            string columnName)
        {
            if (!HasColumn(
                row,
                columnName))
            {
                return 0;
            }


            if (row[columnName] == DBNull.Value)
            {
                return 0;
            }


            string text =
                row[columnName]?.ToString() ?? "";


            return TryParseDecimal(
                text,
                out decimal value)
                ? value
                : 0;
        }


        // =========================================================
        // GET COLUMN VALUE SAFELY
        // =========================================================

        private string GetColumnValue(
            DataRow row,
            string columnName)
        {
            if (!HasColumn(
                row,
                columnName))
            {
                return "";
            }


            if (row[columnName] == DBNull.Value)
            {
                return "";
            }


            return row[columnName]?.ToString() ?? "";
        }


        // =========================================================
        // CHECK COLUMN
        // =========================================================

        private bool HasColumn(
            DataRow row,
            string columnName)
        {
            return row.Table.Columns.Contains(
                columnName);
        }


        // =========================================================
        // FORMAT DATE
        // =========================================================

        private string FormatDate(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "-";
            }


            if (DateTime.TryParse(
                value.ToString(),
                out DateTime date))
            {
                return date.ToString(
                    "yyyy/MM/dd HH:mm");
            }


            return value.ToString() ?? "-";
        }


        // =========================================================
        // COMBOBOX VALUE
        // =========================================================

        private void SetComboBoxValue(
            ComboBox comboBox,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                comboBox.SelectedIndex = -1;

                return;
            }


            foreach (ComboBoxItem item in
                     comboBox.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    comboBox.SelectedItem =
                        item;

                    return;
                }
            }


            comboBox.SelectedIndex = -1;
        }
    }
}