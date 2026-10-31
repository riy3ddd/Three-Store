using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApp1.Views
{
    public partial class InvoiceWindow : Window
    {
        private readonly int saleId;

        private decimal invoiceTotal;

        private decimal invoicePaid;

        private decimal invoiceCredit;


        public InvoiceWindow(int saleId)
        {
            InitializeComponent();

            this.saleId =
                saleId;

            LoadInvoice();
        }


        // =========================================================
        // تحميل الفاتورة
        // =========================================================

        private void LoadInvoice()
        {
            try
            {
                DataTable invoiceInfo =
                    DatabaseHelper.GetInvoiceInfo(
                        saleId);


                if (invoiceInfo.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "لم يتم العثور على بيانات الفاتورة.",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }


                DataRow sale =
                    invoiceInfo.Rows[0];


                // =====================================================
                // رقم الفاتورة
                // =====================================================

                InvoiceNumberText.Text =
                    $"رقم الفاتورة: INV-{saleId:D6}";


                // =====================================================
                // التاريخ
                // =====================================================

                if (sale["SaleDate"] !=
                    DBNull.Value)
                {
                    DateTime saleDate =
                        Convert.ToDateTime(
                            sale["SaleDate"]);


                    InvoiceDateText.Text =
                        $"التاريخ: {saleDate:yyyy/MM/dd HH:mm}";
                }


                // =====================================================
                // اسم العميل
                // =====================================================

                string customerName =
                    sale["CustomerName"] ==
                    DBNull.Value
                        ? ""
                        : sale["CustomerName"]
                            .ToString()!
                            .Trim();


                if (!string.IsNullOrWhiteSpace(
                        customerName))
                {
                    CustomerNameText.Text =
                        $"اسم العميل: {customerName}";

                    CustomerNameText.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    CustomerNameText.Visibility =
                        Visibility.Collapsed;
                }


                // =====================================================
                // الهاتف
                // =====================================================

                string customerPhone =
                    sale["CustomerPhone"] ==
                    DBNull.Value
                        ? ""
                        : sale["CustomerPhone"]
                            .ToString()!
                            .Trim();


                if (!string.IsNullOrWhiteSpace(
                        customerPhone))
                {
                    CustomerPhoneText.Text =
                        $"رقم العميل: {customerPhone}";

                    CustomerPhoneText.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    CustomerPhoneText.Visibility =
                        Visibility.Collapsed;
                }


                // =====================================================
                // الشيفت / المستخدم
                // =====================================================

                string username =
                    sale["Username"] ==
                    DBNull.Value
                        ? ""
                        : sale["Username"]
                            .ToString()!
                            .Trim();


                if (!string.IsNullOrWhiteSpace(username))
                {
                    ShiftUserText.Text =
                        $"الشيفت: {username}";

                    ShiftUserText.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    ShiftUserText.Text =
                        "الشيفت: غير محدد";

                    ShiftUserText.Visibility =
                        Visibility.Visible;
                }


                // =====================================================
                // طريقة الدفع
                // =====================================================

                string paymentMethod =
                    sale["PaymentMethod"] ==
                    DBNull.Value
                        ? ""
                        : sale["PaymentMethod"]
                            .ToString()!
                            .Trim();


                PaymentMethodText.Text =
                    $"طريقة الدفع: {paymentMethod}";


                // =====================================================
                // رقم Vodafone Cash
                // =====================================================

                string vodafoneCashNumber =
                    sale["VodafoneCashNumber"] ==
                    DBNull.Value
                        ? ""
                        : sale["VodafoneCashNumber"]
                            .ToString()!
                            .Trim();


                if (paymentMethod ==
                        "فودافون كاش" &&
                    !string.IsNullOrWhiteSpace(
                        vodafoneCashNumber))
                {
                    VodafoneCashText.Text =
                        $"رقم فودافون كاش: {vodafoneCashNumber}";

                    VodafoneCashText.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    VodafoneCashText.Visibility =
                        Visibility.Collapsed;
                }


                // =====================================================
                // القيم المالية
                // =====================================================

                invoiceTotal =
                    sale["Total"] ==
                    DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(
                            sale["Total"]);


                invoicePaid =
                    sale["Paid"] ==
                    DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(
                            sale["Paid"]);


                invoiceCredit =
                    invoiceTotal -
                    invoicePaid;


                if (invoiceCredit < 0)
                    invoiceCredit = 0;


                // =====================================================
                // عرض الدفع
                // =====================================================

                PaidAmountText.Text =
                    $"المدفوع: {invoicePaid:N2} جنيه";


                CreditAmountText.Text =
                    $"الآجل المتبقي: {invoiceCredit:N2} جنيه";


                PaymentSummaryText.Text =
                    $"المدفوع الآن: {invoicePaid:N2} جنيه";


                CreditSummaryText.Text =
                    $"الآجل المتبقي: {invoiceCredit:N2} جنيه";


                // =====================================================
                // المنتجات
                // =====================================================

                DataTable invoiceTable =
                    DatabaseHelper.GetInvoiceItems(
                        saleId);


                InvoiceItemsGrid.ItemsSource =
                    invoiceTable.DefaultView;


                // =====================================================
                // الإجمالي النهائي
                // =====================================================

                GrandTotalText.Text =
                    $"الإجمالي النهائي: {invoiceTotal:N2} جنيه";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء تحميل الفاتورة:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إنشاء مستند الطباعة
        // =========================================================

        private FixedDocument CreateInvoiceDocument()
        {
            const double pageWidth =
                793.7;

            const double pageHeight =
                1122.5;


            FixedDocument document =
                new FixedDocument();


            document.DocumentPaginator.PageSize =
                new Size(
                    pageWidth,
                    pageHeight);


            FixedPage page =
                new FixedPage();


            page.Width =
                pageWidth;


            page.Height =
                pageHeight;


            page.Background =
                Brushes.White;


            page.FlowDirection =
                FlowDirection.RightToLeft;


            Grid mainGrid =
                new Grid();


            mainGrid.Width =
                pageWidth - 80;


            mainGrid.Height =
                pageHeight - 80;


            mainGrid.Margin =
                new Thickness(40);


            mainGrid.FlowDirection =
                FlowDirection.RightToLeft;


            for (int i = 0; i < 9; i++)
            {
                mainGrid.RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height =
                            i == 4
                                ? new GridLength(
                                    1,
                                    GridUnitType.Star)
                                : GridLength.Auto
                    });
            }


            // =====================================================
            // اسم المحل
            // =====================================================

            TextBlock storeName =
                new TextBlock
                {
                    Text =
                        "Three Store",

                    FontSize =
                        32,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                15,
                                23,
                                42)),

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            8)
                };


            Grid.SetRow(
                storeName,
                0);


            mainGrid.Children.Add(
                storeName);


            // =====================================================
            // عنوان الفاتورة
            // =====================================================

            TextBlock invoiceTitle =
                new TextBlock
                {
                    Text =
                        "فاتورة مبيعات",

                    FontSize =
                        21,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                37,
                                99,
                                235)),

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            8)
                };


            Grid.SetRow(
                invoiceTitle,
                1);


            mainGrid.Children.Add(
                invoiceTitle);


            // =====================================================
            // رقم + تاريخ
            // =====================================================

            StackPanel invoiceInfoPanel =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };


            invoiceInfoPanel.Children.Add(
                new TextBlock
                {
                    Text =
                        InvoiceNumberText.Text,

                    FontSize =
                        16,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brushes.Black,

                    HorizontalAlignment =
                        HorizontalAlignment.Center
                });


            invoiceInfoPanel.Children.Add(
                new TextBlock
                {
                    Text =
                        InvoiceDateText.Text,

                    FontSize =
                        14,

                    Foreground =
                        Brushes.DimGray,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            10)
                });


            Grid.SetRow(
                invoiceInfoPanel,
                2);


            mainGrid.Children.Add(
                invoiceInfoPanel);


            // =====================================================
            // بيانات العميل
            // =====================================================

            Border customerBorder =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                248,
                                250,
                                252)),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                226,
                                232,
                                240)),

                    BorderThickness =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(6),

                    Padding =
                        new Thickness(12),

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            12)
                };


            StackPanel customerPanel =
                new StackPanel
                {
                    FlowDirection =
                        FlowDirection.RightToLeft
                };


            AddPrintableText(
                customerPanel,
                CustomerNameText);

            AddPrintableText(
                customerPanel,
                CustomerPhoneText);

            AddPrintableText(
                customerPanel,
                ShiftUserText);

            AddPrintableText(
                customerPanel,
                PaymentMethodText);

            AddPrintableText(
                customerPanel,
                VodafoneCashText);

            AddPrintableText(
                customerPanel,
                PaidAmountText);

            AddPrintableText(
                customerPanel,
                CreditAmountText);


            customerBorder.Child =
                customerPanel;


            Grid.SetRow(
                customerBorder,
                3);


            mainGrid.Children.Add(
                customerBorder);


            // =====================================================
            // المنتجات
            // =====================================================

            DataGrid printGrid =
                new DataGrid
                {
                    AutoGenerateColumns =
                        false,

                    IsReadOnly =
                        true,

                    CanUserAddRows =
                        false,

                    HeadersVisibility =
                        DataGridHeadersVisibility.Column,

                    GridLinesVisibility =
                        DataGridGridLinesVisibility.All,

                    FlowDirection =
                        FlowDirection.RightToLeft,

                    ItemsSource =
                        InvoiceItemsGrid.ItemsSource,

                    FontSize =
                        13,

                    RowHeight =
                        34,

                    Width =
                        pageWidth - 80,

                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        VerticalAlignment.Stretch,

                    Background =
                        Brushes.White,

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                203,
                                213,
                                225)),

                    BorderThickness =
                        new Thickness(1)
                };


            printGrid.Columns.Add(
                new DataGridTextColumn
                {
                    Header =
                        "رقم",

                    Binding =
                        new System.Windows.Data.Binding(
                            "Number"),

                    Width =
                        55
                });


            printGrid.Columns.Add(
                new DataGridTextColumn
                {
                    Header =
                        "المنتج",

                    Binding =
                        new System.Windows.Data.Binding(
                            "ProductName"),

                    Width =
                        new DataGridLength(
                            1,
                            DataGridLengthUnitType.Star)
                });


            printGrid.Columns.Add(
                new DataGridTextColumn
                {
                    Header =
                        "الكمية",

                    Binding =
                        new System.Windows.Data.Binding(
                            "Quantity"),

                    Width =
                        75
                });


            printGrid.Columns.Add(
                new DataGridTextColumn
                {
                    Header =
                        "السعر",

                    Binding =
                        new System.Windows.Data.Binding(
                            "UnitPrice")
                        {
                            StringFormat =
                                "{0:N2}"
                        },

                    Width =
                        90
                });


            printGrid.Columns.Add(
                new DataGridTextColumn
                {
                    Header =
                        "الإجمالي",

                    Binding =
                        new System.Windows.Data.Binding(
                            "Total")
                        {
                            StringFormat =
                                "{0:N2}"
                        },

                    Width =
                        100
                });


            Grid.SetRow(
                printGrid,
                4);


            mainGrid.Children.Add(
                printGrid);


            // =====================================================
            // الإجمالي
            // =====================================================

            Border totalBorder =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                239,
                                246,
                                255)),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                191,
                                219,
                                254)),

                    BorderThickness =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(6),

                    Padding =
                        new Thickness(
                            15,
                            10,
                            15,
                            10),

                    Margin =
                        new Thickness(
                            0,
                            15,
                            0,
                            5),

                    HorizontalAlignment =
                        HorizontalAlignment.Stretch
                };


            totalBorder.Child =
                new TextBlock
                {
                    Text =
                        GrandTotalText.Text,

                    FontSize =
                        20,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                30,
                                64,
                                175)),

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    TextAlignment =
                        TextAlignment.Right
                };


            Grid.SetRow(
                totalBorder,
                5);


            mainGrid.Children.Add(
                totalBorder);


            // =====================================================
            // المدفوع
            // =====================================================

            Border paidBorder =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                240,
                                253,
                                244)),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                187,
                                247,
                                208)),

                    BorderThickness =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(6),

                    Padding =
                        new Thickness(
                            15,
                            8,
                            15,
                            8),

                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            4)
                };


            paidBorder.Child =
                new TextBlock
                {
                    Text =
                        $"المدفوع الآن: {invoicePaid:N2} جنيه",

                    FontSize =
                        15,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                21,
                                128,
                                61)),

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    TextAlignment =
                        TextAlignment.Right
                };


            Grid.SetRow(
                paidBorder,
                6);


            mainGrid.Children.Add(
                paidBorder);


            // =====================================================
            // الآجل
            // =====================================================

            Border creditBorder =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                255,
                                251,
                                235)),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                253,
                                230,
                                138)),

                    BorderThickness =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(6),

                    Padding =
                        new Thickness(
                            15,
                            8,
                            15,
                            8),

                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            5)
                };


            creditBorder.Child =
                new TextBlock
                {
                    Text =
                        $"الآجل المتبقي: {invoiceCredit:N2} جنيه",

                    FontSize =
                        15,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                180,
                                83,
                                9)),

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    TextAlignment =
                        TextAlignment.Right
                };


            Grid.SetRow(
                creditBorder,
                7);


            mainGrid.Children.Add(
                creditBorder);


            // =====================================================
            // Footer
            // =====================================================

            TextBlock footer =
                new TextBlock
                {
                    Text =
                        "شكراً لتعاملكم مع Three Store",

                    FontSize =
                        12,

                    Foreground =
                        Brushes.Gray,

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    TextAlignment =
                        TextAlignment.Center,

                    Margin =
                        new Thickness(
                            0,
                            8,
                            0,
                            0)
                };


            Grid.SetRow(
                footer,
                8);


            mainGrid.Children.Add(
                footer);


            page.Children.Add(
                mainGrid);


            PageContent pageContent =
                new PageContent();


            pageContent.Child =
                page;


            document.Pages.Add(
                pageContent);


            return document;
        }


        // =========================================================
        // TextBlock للطباعة
        // =========================================================

        private void AddPrintableText(
            Panel panel,
            TextBlock source)
        {
            if (source.Visibility !=
                Visibility.Visible)
            {
                return;
            }


            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        source.Text,

                    FontSize =
                        13,

                    Foreground =
                        Brushes.Black,

                    Margin =
                        new Thickness(
                            0,
                            2,
                            0,
                            2),

                    TextAlignment =
                        TextAlignment.Right
                });
        }


        // =========================================================
        // الطباعة
        // =========================================================

        private void PrintButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                FixedDocument document =
                    CreateInvoiceDocument();


                PrintDialog printDialog =
                    new PrintDialog();


                if (printDialog.ShowDialog() !=
                    true)
                {
                    return;
                }


                printDialog.PrintDocument(
                    document.DocumentPaginator,
                    $"Three Store - INV-{saleId:D6}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء طباعة الفاتورة:\n\n{ex.Message}",
                    "خطأ في الطباعة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إغلاق
        // =========================================================

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}