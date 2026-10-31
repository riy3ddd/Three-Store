using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MaintenanceWindow : Window
    {
        // =========================================================
        // Initialization Flag
        // =========================================================

        private bool _isInitialized = false;


        // =========================================================
        // Constructor
        // =========================================================

        public MaintenanceWindow()
        {
            InitializeComponent();

            // مهم جدًا:
            // نخليها true بعد InitializeComponent
            // لأن TextChanged ممكن يشتغل أثناء إنشاء عناصر XAML
            _isInitialized = true;

            LoadCurrentUser();
            LoadMaintenanceInvoices();
            LoadCompletedMaintenanceInvoices();
            LoadMaintenanceRevenue();

            UpdateMaintenanceTotal();
        }


        // =========================================================
        // Current User
        // =========================================================

        private void LoadCurrentUser()
        {
            try
            {
                if (CurrentUserText == null)
                    return;

                CurrentUserText.Text =
                    SessionManager.CurrentUser?.Username
                    ?? "غير معروف";
            }
            catch
            {
                if (CurrentUserText != null)
                    CurrentUserText.Text = "غير معروف";
            }
        }


        // =========================================================
        // Maintenance Revenue
        // =========================================================

        private void LoadMaintenanceRevenue()
        {
            try
            {
                decimal revenue =
                    DatabaseHelper.GetMaintenanceRevenue(
                        null,
                        null);

                if (MaintenanceRevenueText != null)
                {
                    MaintenanceRevenueText.Text =
                        revenue.ToString(
                            "N2",
                            CultureInfo.CurrentCulture)
                        + " جنيه";
                }
            }
            catch
            {
                if (MaintenanceRevenueText != null)
                    MaintenanceRevenueText.Text =
                        "0.00 جنيه";
            }
        }


        // =========================================================
        // Load Active Maintenance Invoices
        // =========================================================

        private void LoadMaintenanceInvoices()
        {
            try
            {
                DataTable table =
                    DatabaseHelper.GetAllMaintenanceInvoices();

                if (table != null &&
                    table.Columns.Contains("Status"))
                {
                    DataView view =
                        table.DefaultView;

                    view.RowFilter =
                        "Status IS NULL OR Status <> 'تم التسليم'";

                    if (MaintenanceInvoicesGrid != null)
                    {
                        MaintenanceInvoicesGrid.ItemsSource =
                            view;
                    }
                }
                else
                {
                    if (MaintenanceInvoicesGrid != null)
                    {
                        MaintenanceInvoicesGrid.ItemsSource =
                            table?.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل فواتير الصيانة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Load Completed Maintenance Invoices
        // =========================================================

        private void LoadCompletedMaintenanceInvoices()
        {
            try
            {
                DataTable table =
                    DatabaseHelper.GetCompletedMaintenanceInvoices();

                if (table != null &&
                    table.Columns.Contains("Status"))
                {
                    DataView view =
                        table.DefaultView;

                    view.RowFilter =
                        "Status = 'تم التسليم'";

                    if (CompletedMaintenanceInvoicesGrid != null)
                    {
                        CompletedMaintenanceInvoicesGrid.ItemsSource =
                            view;
                    }
                }
                else
                {
                    if (CompletedMaintenanceInvoicesGrid != null)
                    {
                        CompletedMaintenanceInvoicesGrid.ItemsSource =
                            table?.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل سجل الصيانة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Decimal Parser
        // =========================================================

        private bool TryParseDecimal(
            string text,
            out decimal value)
        {
            value = 0m;

            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            bool parsed =
                decimal.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out value);

            if (!parsed)
            {
                parsed =
                    decimal.TryParse(
                        text,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out value);
            }

            return parsed;
        }


        // =========================================================
        // Get Maintenance Cost
        // =========================================================

        private decimal GetMaintenanceCost()
        {
            if (MaintenanceCostTextBox == null)
                return 0m;

            if (!TryParseDecimal(
                    MaintenanceCostTextBox.Text,
                    out decimal value))
            {
                return 0m;
            }

            return value < 0 ? 0m : value;
        }


        // =========================================================
        // Get Spare Parts Cost
        // =========================================================

        private decimal GetSparePartsCost()
        {
            if (AddSparePartCheckBox == null)
                return 0m;

            if (!AddSparePartCheckBox.IsChecked.GetValueOrDefault())
            {
                return 0m;
            }

            if (SparePartsCostTextBox == null)
                return 0m;

            if (!TryParseDecimal(
                    SparePartsCostTextBox.Text,
                    out decimal value))
            {
                return 0m;
            }

            return value < 0 ? 0m : value;
        }


        // =========================================================
        // Update Total
        // =========================================================

        private void UpdateMaintenanceTotal()
        {
            try
            {
                // =================================================
                // مهم:
                // أثناء InitializeComponent بعض العناصر تكون null
                // لذلك لا ننفذ الحساب في هذه المرحلة
                // =================================================

                if (!_isInitialized)
                    return;

                if (MaintenanceCostTextBox == null ||
                    AddSparePartCheckBox == null ||
                    SparePartsCostTextBox == null ||
                    MaintenanceCostSummaryText == null ||
                    SparePartsCostSummaryText == null ||
                    TotalMaintenanceCostText == null)
                {
                    return;
                }


                // =================================================
                // Maintenance Cost
                // =================================================

                decimal maintenanceCost =
                    GetMaintenanceCost();


                // =================================================
                // Spare Parts Cost
                // =================================================

                decimal sparePartsCost =
                    GetSparePartsCost();


                // =================================================
                // Final Total
                // =================================================

                decimal total =
                    maintenanceCost +
                    sparePartsCost;


                // =================================================
                // Display Maintenance Cost
                // =================================================

                MaintenanceCostSummaryText.Text =
                    maintenanceCost.ToString(
                        "N2",
                        CultureInfo.CurrentCulture)
                    + " جنيه";


                // =================================================
                // Display Spare Parts Cost
                // =================================================

                SparePartsCostSummaryText.Text =
                    sparePartsCost.ToString(
                        "N2",
                        CultureInfo.CurrentCulture)
                    + " جنيه";


                // =================================================
                // Display Final Total
                // =================================================

                TotalMaintenanceCostText.Text =
                    total.ToString(
                        "N2",
                        CultureInfo.CurrentCulture)
                    + " جنيه";
            }
            catch
            {
                if (MaintenanceCostSummaryText != null)
                {
                    MaintenanceCostSummaryText.Text =
                        "0.00 جنيه";
                }

                if (SparePartsCostSummaryText != null)
                {
                    SparePartsCostSummaryText.Text =
                        "0.00 جنيه";
                }

                if (TotalMaintenanceCostText != null)
                {
                    TotalMaintenanceCostText.Text =
                        "0.00 جنيه";
                }
            }
        }


        // =========================================================
        // Maintenance Cost Changed
        // =========================================================

        private void MaintenanceCostTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            UpdateMaintenanceTotal();
        }


        // =========================================================
        // Spare Parts Name Changed
        // =========================================================

        private void SparePartsNameTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            UpdateMaintenanceTotal();
        }


        // =========================================================
        // Spare Parts Cost Changed
        // =========================================================

        private void SparePartsCostTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            UpdateMaintenanceTotal();
        }


        // =========================================================
        // Add Spare Part Checked
        // =========================================================

        private void AddSparePartCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (SparePartPanel != null)
            {
                SparePartPanel.Visibility =
                    Visibility.Visible;
            }

            if (_isInitialized)
            {
                UpdateMaintenanceTotal();
            }
        }


        // =========================================================
        // Add Spare Part Unchecked
        // =========================================================

        private void AddSparePartCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            if (SparePartPanel != null)
            {
                SparePartPanel.Visibility =
                    Visibility.Collapsed;
            }

            if (SparePartsNameTextBox != null)
            {
                SparePartsNameTextBox.Clear();
            }

            if (SparePartsCostTextBox != null)
            {
                SparePartsCostTextBox.Text = "0";
            }

            if (_isInitialized)
            {
                UpdateMaintenanceTotal();
            }
        }


        // =========================================================
        // Save New Maintenance Invoice
        // =========================================================

        private void SaveMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // -------------------------------------------------
                // Customer Validation
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    CustomerNameTextBox.Text))
                {
                    MessageBox.Show(
                        "اسم العميل مطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CustomerNameTextBox.Focus();
                    return;
                }


                // -------------------------------------------------
                // Device Validation
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    DeviceNameTextBox.Text))
                {
                    MessageBox.Show(
                        "اسم الجهاز مطلوب.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    DeviceNameTextBox.Focus();
                    return;
                }


                // -------------------------------------------------
                // Maintenance Type Validation
                // -------------------------------------------------

                if (MaintenanceTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "اختر نوع الصيانة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    MaintenanceTypeComboBox.Focus();
                    return;
                }


                // -------------------------------------------------
                // Maintenance Cost
                // -------------------------------------------------

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


                // -------------------------------------------------
                // Spare Part
                // -------------------------------------------------

                decimal sparePartsCost = 0m;
                string sparePartsName = "";


                if (AddSparePartCheckBox.IsChecked.GetValueOrDefault())
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
                            "أدخل سعر قطعة غيار صحيح.",
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


                // -------------------------------------------------
                // Final Total
                // -------------------------------------------------

                decimal totalPrice =
                    maintenanceCost +
                    sparePartsCost;


                // -------------------------------------------------
                // Maintenance Type
                // -------------------------------------------------

                ComboBoxItem typeItem =
                    MaintenanceTypeComboBox.SelectedItem
                    as ComboBoxItem;

                string maintenanceType =
                    typeItem?.Content?.ToString() ?? "";


                if (string.IsNullOrWhiteSpace(
                    maintenanceType))
                {
                    MessageBox.Show(
                        "اختر نوع الصيانة.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // -------------------------------------------------
                // Invoice Number
                // -------------------------------------------------

                string invoiceNumber =
                    DatabaseHelper.GetNextMaintenanceInvoiceNumber();


                // -------------------------------------------------
                // Current User
                // -------------------------------------------------

                string username =
                    SessionManager.CurrentUser?.Username
                    ?? "Unknown";


                // -------------------------------------------------
                // Device Condition
                // -------------------------------------------------

                string deviceCondition = "";


                // -------------------------------------------------
                // Initial Status
                // -------------------------------------------------

                string status = "مستلم";


                // -------------------------------------------------
                // Save Invoice
                // -------------------------------------------------

                int maintenanceId =
                    DatabaseHelper.AddMaintenanceInvoice(
                        invoiceNumber,
                        CustomerNameTextBox.Text.Trim(),
                        CustomerPhoneTextBox.Text.Trim(),
                        DeviceNameTextBox.Text.Trim(),
                        SerialNumberTextBox.Text.Trim(),
                        deviceCondition,
                        DeviceColorTextBox.Text.Trim(),
                        ProblemDescriptionTextBox.Text.Trim(),
                        AccessoriesTextBox.Text.Trim(),
                        NotesTextBox.Text.Trim(),
                        maintenanceType,
                        totalPrice,
                        username,
                        DeliveryDatePicker.SelectedDate,
                        status);


                // -------------------------------------------------
                // Save Extra Cost Information
                // -------------------------------------------------

                DatabaseHelper.UpdateMaintenanceCostDetails(
                    maintenanceId,
                    maintenanceCost,
                    sparePartsName,
                    sparePartsCost);


                // -------------------------------------------------
                // Success
                // -------------------------------------------------

                MessageBox.Show(
                    "تم حفظ فاتورة الصيانة بنجاح.\n\n" +
                    "رقم الفاتورة: " + invoiceNumber +
                    "\nالتكلفة التقديرية: " +
                    maintenanceCost.ToString(
                        "N2",
                        CultureInfo.CurrentCulture) +
                    " جنيه" +
                    "\nسعر قطعة الغيار: " +
                    sparePartsCost.ToString(
                        "N2",
                        CultureInfo.CurrentCulture) +
                    " جنيه" +
                    "\nالإجمالي النهائي: " +
                    totalPrice.ToString(
                        "N2",
                        CultureInfo.CurrentCulture) +
                    " جنيه",
                    "تم الحفظ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // -------------------------------------------------
                // Open Invoice View
                // -------------------------------------------------

                try
                {
                    MaintenanceInvoiceViewWindow invoiceWindow =
                        new MaintenanceInvoiceViewWindow(
                            maintenanceId);

                    invoiceWindow.Owner = this;

                    invoiceWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "تم حفظ الفاتورة، ولكن حدث خطأ أثناء فتح شاشة الفاتورة:\n\n" +
                        ex.Message,
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }


                // -------------------------------------------------
                // Clear + Refresh
                // -------------------------------------------------

                ClearForm();

                LoadMaintenanceInvoices();
                LoadCompletedMaintenanceInvoices();
                LoadMaintenanceRevenue();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ فاتورة الصيانة:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Clear Form
        // =========================================================

        private void ClearForm()
        {
            if (CustomerNameTextBox != null)
                CustomerNameTextBox.Clear();

            if (CustomerPhoneTextBox != null)
                CustomerPhoneTextBox.Clear();

            if (DeviceNameTextBox != null)
                DeviceNameTextBox.Clear();

            if (SerialNumberTextBox != null)
                SerialNumberTextBox.Clear();

            if (DeviceColorTextBox != null)
                DeviceColorTextBox.Clear();

            if (ProblemDescriptionTextBox != null)
                ProblemDescriptionTextBox.Clear();

            if (AccessoriesTextBox != null)
                AccessoriesTextBox.Clear();

            if (NotesTextBox != null)
                NotesTextBox.Clear();

            if (MaintenanceCostTextBox != null)
                MaintenanceCostTextBox.Text = "0";

            if (SparePartsNameTextBox != null)
                SparePartsNameTextBox.Clear();

            if (SparePartsCostTextBox != null)
                SparePartsCostTextBox.Text = "0";

            if (AddSparePartCheckBox != null)
                AddSparePartCheckBox.IsChecked = false;

            if (SparePartPanel != null)
                SparePartPanel.Visibility =
                    Visibility.Collapsed;

            if (MaintenanceTypeComboBox != null)
                MaintenanceTypeComboBox.SelectedIndex = -1;

            if (DeliveryDatePicker != null)
                DeliveryDatePicker.SelectedDate = null;

            UpdateMaintenanceTotal();

            if (CustomerNameTextBox != null)
                CustomerNameTextBox.Focus();
        }


        // =========================================================
        // Clear Form Button
        // =========================================================

        private void ClearFormButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ClearForm();
        }


        // =========================================================
        // Search Active Invoices
        // =========================================================

        private void MaintenanceSearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            try
            {
                string searchText =
                    MaintenanceSearchTextBox.Text.Trim();


                if (string.IsNullOrWhiteSpace(
                    searchText))
                {
                    LoadMaintenanceInvoices();
                    return;
                }


                DataTable table =
                    DatabaseHelper.SearchMaintenanceInvoices(
                        searchText);


                if (table != null &&
                    table.Columns.Contains("Status"))
                {
                    DataView view =
                        table.DefaultView;

                    view.RowFilter =
                        "Status IS NULL OR Status <> 'تم التسليم'";

                    MaintenanceInvoicesGrid.ItemsSource =
                        view;
                }
                else
                {
                    MaintenanceInvoicesGrid.ItemsSource =
                        table?.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء البحث:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Refresh
        // =========================================================

        private void RefreshMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MaintenanceSearchTextBox.Clear();

            LoadMaintenanceInvoices();
            LoadCompletedMaintenanceInvoices();
            LoadMaintenanceRevenue();
        }


        // =========================================================
        // Selection Changed
        // =========================================================

        private void MaintenanceInvoicesGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // لا يوجد إجراء حاليًا
        }


        // =========================================================
        // Double Click Active Invoice
        // =========================================================

        private void MaintenanceInvoicesGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            try
            {
                if (MaintenanceInvoicesGrid.SelectedItem
                    is not DataRowView row)
                {
                    return;
                }


                if (!int.TryParse(
                    row["MaintenanceID"]?.ToString(),
                    out int maintenanceId))
                {
                    MessageBox.Show(
                        "رقم الفاتورة غير صالح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                OpenMaintenanceView(maintenanceId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Open Maintenance Invoice View
        // =========================================================

        private void OpenMaintenanceView(
            object maintenanceIdValue)
        {
            try
            {
                if (!int.TryParse(
                    maintenanceIdValue?.ToString(),
                    out int maintenanceId))
                {
                    MessageBox.Show(
                        "رقم الفاتورة غير صالح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                MaintenanceInvoiceViewWindow window =
                    new MaintenanceInvoiceViewWindow(
                        maintenanceId);

                window.Owner = this;

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء عرض الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Edit Invoice
        // =========================================================

        private void EditMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button ||
                    button.Tag == null)
                {
                    return;
                }


                if (!int.TryParse(
                    button.Tag.ToString(),
                    out int maintenanceId))
                {
                    return;
                }


                MaintenanceInvoiceWindow window =
                    new MaintenanceInvoiceWindow(
                        maintenanceId);

                window.Owner = this;

                window.ShowDialog();


                LoadMaintenanceInvoices();
                LoadCompletedMaintenanceInvoices();
                LoadMaintenanceRevenue();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة تعديل الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Complete / Deliver Invoice
        // =========================================================

        private void CompleteMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button ||
                    button.Tag == null)
                {
                    return;
                }


                if (!int.TryParse(
                    button.Tag.ToString(),
                    out int maintenanceId))
                {
                    MessageBox.Show(
                        "رقم الفاتورة غير صالح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                MessageBoxResult result =
                    MessageBox.Show(
                        "هل أنت متأكد من تسليم هذه الفاتورة؟\n\n" +
                        "بعد التأكيد سيتم تسجيل تاريخ التسليم ونقل الفاتورة إلى الأرشيف،\n" +
                        "وسيتم احتساب إجمالي الفاتورة ضمن إيرادات الصيانة.",
                        "تأكيد التسليم",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);


                if (result != MessageBoxResult.Yes)
                {
                    return;
                }


                DatabaseHelper.CompleteMaintenanceInvoice(
                    maintenanceId);


                LoadMaintenanceInvoices();
                LoadCompletedMaintenanceInvoices();
                LoadMaintenanceRevenue();


                MessageBox.Show(
                    "تم تسليم الجهاز بنجاح.\n\n" +
                    "تم تسجيل تاريخ التسليم ونقل الفاتورة إلى الأرشيف.\n" +
                    "وتم تسجيل قيمة الفاتورة كاملة ضمن إيرادات الصيانة.",
                    "تم التسليم",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تسليم الفاتورة:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // Delete Active Invoice
        // =========================================================

        private void DeleteMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button button ||
                    button.Tag == null)
                {
                    return;
                }


                if (!int.TryParse(
                    button.Tag.ToString(),
                    out int maintenanceId))
                {
                    return;
                }


                MessageBoxResult result =
                    MessageBox.Show(
                        "هل أنت متأكد من حذف فاتورة الصيانة؟\n\n" +
                        "لا يمكن التراجع عن هذه العملية.",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);


                if (result != MessageBoxResult.Yes)
                {
                    return;
                }


                DatabaseHelper.DeleteMaintenanceInvoice(
                    maintenanceId);


                MessageBox.Show(
                    "تم حذف الفاتورة بنجاح.",
                    "تم الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                LoadMaintenanceInvoices();
                LoadCompletedMaintenanceInvoices();
                LoadMaintenanceRevenue();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حذف الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // View Completed Invoice
        // =========================================================

        private void ViewCompletedMaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag != null)
            {
                OpenMaintenanceView(button.Tag);
            }
        }


        // =========================================================
        // Double Click Completed Invoice
        // =========================================================

        private void CompletedMaintenanceInvoicesGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            try
            {
                if (CompletedMaintenanceInvoicesGrid.SelectedItem
                    is not DataRowView row)
                {
                    return;
                }


                if (!int.TryParse(
                    row["MaintenanceID"]?.ToString(),
                    out int maintenanceId))
                {
                    MessageBox.Show(
                        "رقم الفاتورة غير صالح.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                OpenMaintenanceView(maintenanceId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}