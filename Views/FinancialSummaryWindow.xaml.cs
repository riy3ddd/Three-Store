using System;
using System.Windows;

namespace WpfApp1.Views
{
    public partial class FinancialSummaryWindow : Window
    {
        public FinancialSummaryWindow()
        {
            InitializeComponent();

            Loaded += FinancialSummaryWindow_Loaded;
        }


        private void FinancialSummaryWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadToday();
        }


        // =========================================================
        // اليوم
        // =========================================================

        private void LoadToday()
        {
            DateTime start =
                DateTime.Today;

            DateTime end =
                DateTime.Today.AddDays(1);

            LoadSummary(start, end);
        }


        // =========================================================
        // آخر 7 أيام
        // =========================================================

        private void LoadLast7Days()
        {
            DateTime start =
                DateTime.Today.AddDays(-6);

            DateTime end =
                DateTime.Today.AddDays(1);

            LoadSummary(start, end);
        }


        // =========================================================
        // الشهر الحالي
        // =========================================================

        private void LoadThisMonth()
        {
            DateTime start =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime end =
                start.AddMonths(1);

            LoadSummary(start, end);
        }


        // =========================================================
        // تحميل البيانات
        // =========================================================

        private void LoadSummary(
            DateTime start,
            DateTime end)
        {
            try
            {
                FinancialSummary summary =
                    FinancialCalculator.GetSummary(
                        start,
                        end);


                SalesText.Text =
                    $"{summary.SalesRevenue:N2} ج.م";


                MaintenanceText.Text =
                    $"{summary.MaintenanceRevenue:N2} ج.م";


                ExpensesText.Text =
                    $"{summary.Expenses:N2} ج.م";


                NetText.Text =
                    $"{summary.NetRevenue:N2} ج.م";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حساب الملخص المالي:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void Today_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadToday();
        }


        private void Last7Days_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadLast7Days();
        }


        private void ThisMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadThisMonth();
        }
    }
}