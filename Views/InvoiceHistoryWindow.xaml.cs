using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Views
{
    public partial class InvoiceHistoryWindow : Window
    {
        private DataTable invoicesTable =
            new DataTable();


        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public InvoiceHistoryWindow()
        {
            InitializeComponent();


            TodayDateText.Text =
                DateTime.Now.ToString(
                    "dd MMMM yyyy");


            LoadInvoices();
        }


        // ==========================================
        // LOAD INVOICES
        // ==========================================

        private void LoadInvoices()
        {
            try
            {
                invoicesTable =
                    DatabaseHelper.GetAllInvoices();


                InvoicesGrid.ItemsSource =
                    invoicesTable.DefaultView;


                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء تحميل سجل الفواتير:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // UPDATE STATISTICS
        // ==========================================

        private void UpdateStatistics()
        {
            decimal totalSales = 0;

            int invoiceCount = 0;


            if (invoicesTable == null)
                return;


            foreach (DataRowView rowView
                     in invoicesTable.DefaultView)
            {
                DataRow row =
                    rowView.Row;


                invoiceCount++;


                if (row["Total"] != DBNull.Value)
                {
                    totalSales +=
                        Convert.ToDecimal(
                            row["Total"]);
                }
            }


            TodaySalesText.Text =
                $"{totalSales:N2} ج.م";


            TodayInvoicesText.Text =
                $"{invoiceCount} فاتورة";
        }


        // ==========================================
        // APPLY ALL FILTERS
        // ==========================================

        private void ApplyFilters()
        {
            if (invoicesTable == null)
                return;


            string filter = "";


            // ==========================================
            // SEARCH
            // ==========================================

            string searchText =
                SearchBox.Text.Trim();


            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                string safeSearch =
                    searchText.Replace(
                        "'",
                        "''");


                filter =
                    $"InvoiceNumber LIKE '%{safeSearch}%'";
            }


            // ==========================================
            // PAYMENT METHOD
            // ==========================================

            string paymentFilter = "";


            if (PaymentFilterComboBox.SelectedIndex == 1)
            {
                paymentFilter =
                    "PaymentMethod = 'كاش'";
            }
            else if (PaymentFilterComboBox.SelectedIndex == 2)
            {
                paymentFilter =
                    "PaymentMethod = 'فودافون كاش'";
            }


            if (!string.IsNullOrWhiteSpace(
                paymentFilter))
            {
                if (!string.IsNullOrWhiteSpace(
                    filter))
                {
                    filter += " AND ";
                }


                filter +=
                    paymentFilter;
            }


            // ==========================================
            // PERIOD
            // ==========================================

            string periodFilter = "";


            if (PeriodFilterComboBox.SelectedIndex == 0)
            {
                // TODAY

                DateTime start =
                    DateTime.Today;

                DateTime end =
                    DateTime.Today.AddDays(1);


                periodFilter =
                    $"SaleDate >= #{start:MM/dd/yyyy HH:mm:ss}# " +
                    $"AND SaleDate < #{end:MM/dd/yyyy HH:mm:ss}#";
            }
            else if (PeriodFilterComboBox.SelectedIndex == 1)
            {
                // LAST 7 DAYS

                DateTime start =
                    DateTime.Today.AddDays(-6);

                DateTime end =
                    DateTime.Today.AddDays(1);


                periodFilter =
                    $"SaleDate >= #{start:MM/dd/yyyy HH:mm:ss}# " +
                    $"AND SaleDate < #{end:MM/dd/yyyy HH:mm:ss}#";
            }
            else if (PeriodFilterComboBox.SelectedIndex == 2)
            {
                // THIS MONTH

                DateTime start =
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1);


                DateTime end =
                    start.AddMonths(1);


                periodFilter =
                    $"SaleDate >= #{start:MM/dd/yyyy HH:mm:ss}# " +
                    $"AND SaleDate < #{end:MM/dd/yyyy HH:mm:ss}#";
            }
            else if (PeriodFilterComboBox.SelectedIndex == 3)
            {
                // ALL INVOICES

                periodFilter = "";
            }


            if (!string.IsNullOrWhiteSpace(
                periodFilter))
            {
                if (!string.IsNullOrWhiteSpace(
                    filter))
                {
                    filter += " AND ";
                }


                filter +=
                    periodFilter;
            }


            // ==========================================
            // APPLY
            // ==========================================

            try
            {
                invoicesTable
                    .DefaultView
                    .RowFilter =
                        filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء تطبيق الفلترة:\n\n{ex.Message}",
                    "خطأ في الفلترة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }


            UpdateStatistics();

            UpdateInvoiceCount();
        }


        // ==========================================
        // SEARCH
        // ==========================================

        private void SearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;


            ApplyFilters();
        }


        // ==========================================
        // PAYMENT FILTER
        // ==========================================

        private void PaymentFilterComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;


            ApplyFilters();
        }


        // ==========================================
        // PERIOD FILTER
        // ==========================================

        private void PeriodFilterComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;


            ApplyFilters();
        }


        // ==========================================
        // FILTER BUTTON
        // ==========================================

        private void FilterButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ApplyFilters();
        }


        // ==========================================
        // UPDATE INVOICE COUNT
        // ==========================================

        private void UpdateInvoiceCount()
        {
            if (invoicesTable == null)
                return;


            int count =
                invoicesTable.DefaultView.Count;


            InvoiceCountText.Text =
                $"{count} فاتورة";
        }


        // ==========================================
        // NEW INVOICE
        // ==========================================

        private void NewInvoiceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                SalesWindow salesWindow =
                    new SalesWindow();


                salesWindow.Owner =
                    this;


                salesWindow.SaleCompleted += () =>
                {
                    LoadInvoices();
                };


                salesWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء فتح شاشة المبيعات:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // OPEN INVOICE DETAILS
        // ==========================================

        private void InvoicesGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (InvoicesGrid.SelectedItem
                is not DataRowView row)
            {
                return;
            }


            int saleId =
                Convert.ToInt32(
                    row["SaleID"]);


            InvoiceWindow invoiceWindow =
                new InvoiceWindow(
                    saleId);


            invoiceWindow.Owner =
                this;


            invoiceWindow.ShowDialog();
        }
    }
}