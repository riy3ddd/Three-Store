using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    public partial class FavoriteCustomersWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private List<FavoriteCustomer> allCustomers = new();

        public FavoriteCustomersWindow()
        {
            InitializeComponent();

            // الافتراضي: أعلى 5
            TopCountComboBox.SelectedIndex = 0;

            LoadCustomers();
        }

        // =========================================================
        // تحميل العملاء
        // =========================================================
        private void LoadCustomers()
        {
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();

                string query = @"
                    SELECT
                        CustomerName,
                        CustomerPhone,
                        InvoiceCount,
                        LastInvoiceDate,
                        SalesInvoiceCount,
                        MaintenanceInvoiceCount
                    FROM dbo.FavoriteCustomers
                    ORDER BY InvoiceCount DESC, LastInvoiceDate DESC;";

                using SqlCommand command = new SqlCommand(query, connection);

                using SqlDataReader reader = command.ExecuteReader();

                allCustomers.Clear();

                while (reader.Read())
                {
                    allCustomers.Add(new FavoriteCustomer
                    {
                        CustomerName = reader["CustomerName"]?.ToString() ?? "",
                        CustomerPhone = reader["CustomerPhone"]?.ToString() ?? "",
                        InvoiceCount = Convert.ToInt32(reader["InvoiceCount"]),
                        SalesInvoiceCount = Convert.ToInt32(reader["SalesInvoiceCount"]),
                        MaintenanceInvoiceCount = Convert.ToInt32(reader["MaintenanceInvoiceCount"]),
                        LastInvoiceDate = reader["LastInvoiceDate"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["LastInvoiceDate"])
                    });
                }

                UpdateStatistics();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل قائمة العملاء المفضلين:\n\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // الإحصائيات
        // =========================================================
        private void UpdateStatistics()
        {
            TotalCustomersTextBlock.Text = allCustomers.Count.ToString("N0");

            int totalInvoices = allCustomers.Sum(x => x.InvoiceCount);

            TotalInvoicesTextBlock.Text = totalInvoices.ToString("N0");

            FavoriteCustomer? topCustomer = allCustomers
                .OrderByDescending(x => x.InvoiceCount)
                .ThenByDescending(x => x.LastInvoiceDate)
                .FirstOrDefault();

            if (topCustomer != null)
            {
                TopCustomerTextBlock.Text =
                    $"{topCustomer.CustomerName} ({topCustomer.InvoiceCount})";
            }
            else
            {
                TopCustomerTextBlock.Text = "لا يوجد";
            }
        }

        // =========================================================
        // تطبيق البحث + TOP
        // =========================================================
        private void ApplyFilters()
        {
            IEnumerable<FavoriteCustomer> filteredCustomers = allCustomers;

            string searchText = SearchTextBox?.Text?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredCustomers = filteredCustomers.Where(x =>
                    x.CustomerName.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    x.CustomerPhone.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase));
            }

            filteredCustomers = filteredCustomers
                .OrderByDescending(x => x.InvoiceCount)
                .ThenByDescending(x => x.LastInvoiceDate);

            int topCount = GetSelectedTopCount();

            if (topCount > 0)
            {
                filteredCustomers = filteredCustomers.Take(topCount);
            }

            List<FavoriteCustomer> result = filteredCustomers.ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Rank = i + 1;
            }

            CustomersDataGrid.ItemsSource = result;
        }

        // =========================================================
        // الحصول على TOP
        // =========================================================
        private int GetSelectedTopCount()
        {
            if (TopCountComboBox.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Tag?.ToString(), out int value))
                {
                    return value;
                }
            }

            return 5;
        }

        // =========================================================
        // تغيير TOP
        // =========================================================
        private void TopCountComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            ApplyFilters();
        }

        // =========================================================
        // البحث
        // =========================================================
        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            ApplyFilters();
        }

        // =========================================================
        // تحديث
        // =========================================================
        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadCustomers();
        }
    }

    // =============================================================
    // Model
    // =============================================================
    public class FavoriteCustomer
    {
        public int Rank { get; set; }

        public string CustomerName { get; set; } = "";

        public string CustomerPhone { get; set; } = "";

        public int InvoiceCount { get; set; }

        public int SalesInvoiceCount { get; set; }

        public int MaintenanceInvoiceCount { get; set; }

        public DateTime? LastInvoiceDate { get; set; }
    }
}