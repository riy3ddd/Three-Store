using System;
using System.Windows;
using WpfApp1.Views;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // =========================================================
            // عرض اسم المستخدم الحالي
            // =========================================================

            CurrentUsernameTextBlock.Text =
                SessionManager.CurrentUsername;
            // =========================================================
            // الصلاحيات
            // =========================================================

            if (!SessionManager.IsAdmin)
            {
                // =====================================================
                // شاشات ممنوعة على الموظف
                // =====================================================

                ProductsButton.Visibility =
                    Visibility.Collapsed;

                CategoriesButton.Visibility =
                    Visibility.Collapsed;

                ReportsButton.Visibility =
                    Visibility.Collapsed;

                UsersButton.Visibility =
                    Visibility.Collapsed;

                InventoryButton.Visibility =
                    Visibility.Collapsed;

                RevenueBoxButton.Visibility =
                    Visibility.Collapsed;

                FavoriteCustomersButton.Visibility =
                    Visibility.Collapsed;

                DevicesButton.Visibility =
                    Visibility.Collapsed;

                OffersButton.Visibility =
                    Visibility.Collapsed;

                CreditButton.Visibility =
                    Visibility.Collapsed;


                // =====================================================
                // الشاشات المسموحة للموظف
                // =====================================================

                // المبيعات        → مسموحة
                // الصيانة         → مسموحة
                // المرتجعات       → مسموحة
                // المصروفات       → مسموحة
                // طلبات العملاء   → مسموحة
                // سجل الفواتير    → مسموحة
            }

            // =========================================================
            // تحميل بيانات لوحة التحكم
            // =========================================================

            LoadDashboardData();
        }


        // =========================================================
        // تحميل بيانات لوحة التحكم
        // =========================================================

        public void LoadDashboardData()
        {
            try
            {
                // عدد المنتجات
                int totalProducts =
                    DatabaseHelper.GetTotalProducts();

                // عدد الأقسام
                int totalCategories =
                    DatabaseHelper.GetTotalCategories();

                // إجمالي كمية المنتجات
                int totalQuantity =
                    DatabaseHelper.GetTotalQuantity();

                // المنتجات ذات المخزون المنخفض
                int lowStockProducts =
                    DatabaseHelper.GetLowStockProducts();


                // =====================================================
                // تحديث أرقام Dashboard
                // =====================================================

                TotalProductsTextBlock.Text =
                    totalProducts.ToString();

                TotalCategoriesTextBlock.Text =
                    totalCategories.ToString();

                TotalQuantityTextBlock.Text =
                    totalQuantity.ToString();

                LowStockProductsTextBlock.Text =
                    lowStockProducts.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات لوحة التحكم:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح نافذة المنتجات
        // =========================================================

        private void ProductsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<ProductsWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة المنتجات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح نافذة التصنيفات
        // =========================================================

        private void CategoriesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<CategoriesWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة التصنيفات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح نافذة المبيعات
        // =========================================================

        private void SalesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<SalesWindow>(
                    this,
                    salesWindow =>
                    {
                        salesWindow.SaleCompleted += () =>
                        {
                            LoadDashboardData();
                        };
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة المبيعات:\n\n" +
                    ex.ToString(),
                    "خطأ في شاشة المبيعات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح سجل الفواتير
        // =========================================================

        private void InvoiceHistoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<InvoiceHistoryWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح سجل الفواتير:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح نافذة التقارير والتحليل
        // =========================================================

        private void ReportsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<ReportsWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة التقارير:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح إدارة الأعضاء
        // =========================================================

        private void UsersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<UsersWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح إدارة الأعضاء:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح نافذة الصيانة
        // =========================================================

        private void MaintenanceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<MaintenanceWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة الصيانة:\n\n" +
                    ex.ToString(),
                    "خطأ في شاشة الصيانة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح طلبات العملاء
        // =========================================================

        private void OrdersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<OrdersWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح طلبات العملاء:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح جرد المنتجات
        // =========================================================

        private void InventoryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<InventoryWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح جرد المنتجات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح صندوق الإيرادات
        // =========================================================

        private void RevenueBoxButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<RevenueBoxWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح صندوق الإيرادات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح شاشة المصروفات / المسحوبات
        // =========================================================

        private void ExpensesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // =====================================================
                // المصروفات مسموحة للموظف والمدير
                // =====================================================

                WindowManager.ShowSingle<ExpensesWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة المصروفات:\n\n" +
                    ex.ToString(),
                    "خطأ في شاشة المصروفات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح قائمة العملاء المفضلين
        // =========================================================

        private void FavoriteCustomersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<FavoriteCustomersWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح قائمة العملاء المفضلين:\n\n" +
                    ex.ToString(),
                    "خطأ في العملاء المفضلين",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح شاشة المرتجعات
        // =========================================================

        private void ReturnsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<ReturnsWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة المرتجعات:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح جرد الأجهزة
        // =========================================================

        private void DevicesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<DevicesWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح جرد الأجهزة:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح شاشة العروض
        // =========================================================

        private void OffersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<OffersWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة العروض:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // فتح شاشة الآجل
        // =========================================================

        private void CreditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!SessionManager.IsAdmin)
                {
                    ShowPermissionDenied();
                    return;
                }

                WindowManager.ShowSingle<CreditWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة الآجل:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // رسالة عدم وجود صلاحية
        // =========================================================

        private void ShowPermissionDenied()
        {
            MessageBox.Show(
                "ليس لديك صلاحية للوصول إلى هذه الشاشة.",
                "غير مسموح",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }



        // =========================================================
        // فتح شاشة المخزون المنخفض
        // =========================================================

        private void LowStockCard_MouseLeftButtonUp(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                WindowManager.ShowSingle<LowStockWindow>(
                    this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح شاشة المخزون المنخفض:\n\n" +
                    ex.ToString(),
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}