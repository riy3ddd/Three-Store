using System;
using System.Data;
using System.Globalization;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MaintenanceInvoiceViewWindow : Window
    {
        private readonly int _maintenanceId;

        // =========================================================
        // Constructor
        // =========================================================

        public MaintenanceInvoiceViewWindow(int maintenanceId)
        {
            InitializeComponent();

            _maintenanceId = maintenanceId;

            LoadInvoice();
        }


        // =========================================================
        // تحميل بيانات الفاتورة
        // =========================================================

        private void LoadInvoice()
        {
            try
            {
                DataTable table =
                    DatabaseHelper.GetMaintenanceInvoice(_maintenanceId);

                if (table == null || table.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "لم يتم العثور على فاتورة الصيانة.",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    Close();
                    return;
                }

                DataRow row = table.Rows[0];


                // =================================================
                // بيانات الفاتورة
                // =================================================

                InvoiceNumberText.Text =
                    GetString(row, "InvoiceNumber");

                UsernameText.Text =
                    GetString(row, "Username");


                // =================================================
                // بيانات العميل
                // =================================================

                CustomerNameText.Text =
                    GetString(row, "CustomerName");

                CustomerPhoneText.Text =
                    GetString(row, "CustomerPhone");


                // =================================================
                // بيانات الجهاز
                // =================================================

                DeviceNameText.Text =
                    GetString(row, "DeviceName");

                SerialNumberText.Text =
                    GetString(row, "SerialNumber");

                DeviceConditionText.Text =
                    GetString(row, "DeviceCondition");

                // موجود في الـ XAML للتوافق مع الكود
                DeviceColorText.Text =
                    GetString(row, "DeviceColor");


                // =================================================
                // تفاصيل الصيانة
                // =================================================

                MaintenanceTypeText.Text =
                    GetString(row, "MaintenanceType");

                StatusText.Text =
                    GetString(row, "Status");

                ProblemText.Text =
                    GetString(row, "ProblemDescription");

                AccessoriesText.Text =
                    GetString(row, "Accessories");

                NotesText.Text =
                    GetString(row, "Notes");

                SparePartsNameText.Text =
    GetString(row, "SparePartsName");

                if (row.Table.Columns.Contains("SparePartsCost") &&
                    row["SparePartsCost"] != DBNull.Value)
                {
                    decimal sparePartsCost =
                        Convert.ToDecimal(row["SparePartsCost"]);

                    SparePartsCostText.Text =
                        sparePartsCost.ToString(
                            "N2",
                            CultureInfo.CurrentCulture)
                        + " ج.م";
                }
                else
                {
                    SparePartsCostText.Text = "0.00 ج.م";
                }
                // =================================================
                // السعر
                // =================================================

                if (row["Price"] != DBNull.Value)
                {
                    decimal price =
                        Convert.ToDecimal(row["Price"]);

                    PriceText.Text =
                        price.ToString(
                            "N2",
                            CultureInfo.InvariantCulture) + " ج.م";
                }
                else
                {
                    PriceText.Text = "0.00 ج.م";
                }


                // =================================================
                // تاريخ الاستلام
                // =================================================

                if (row["ReceiveDate"] != DBNull.Value)
                {
                    DateTime receiveDate =
                        Convert.ToDateTime(row["ReceiveDate"]);

                    ReceiveDateText.Text =
                        receiveDate.ToString(
                            "dd/MM/yyyy - hh:mm tt",
                            new CultureInfo("ar-EG"));
                }
                else
                {
                    ReceiveDateText.Text = "-";
                }


                // =================================================
                // موعد التسليم
                // =================================================

                if (row["DeliveryDate"] != DBNull.Value)
                {
                    DateTime deliveryDate =
                        Convert.ToDateTime(row["DeliveryDate"]);

                    DeliveryDateText.Text =
                        deliveryDate.ToString(
                            "dd/MM/yyyy - hh:mm tt",
                            new CultureInfo("ar-EG"));
                }
                else
                {
                    DeliveryDateText.Text = "-";
                }


                // =================================================
                // تغيير شكل الحالة حسب الحالة
                // =================================================

                SetStatusStyle(
                    GetString(row, "Status"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }


        // =========================================================
        // قراءة قيمة String بأمان
        // =========================================================

        private static string GetString(
            DataRow row,
            string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "-";

            if (row[columnName] == DBNull.Value)
                return "-";

            string value =
                Convert.ToString(row[columnName]) ?? "";

            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value;
        }


        // =========================================================
        // تنسيق حالة الصيانة
        // =========================================================

        private void SetStatusStyle(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                status = "-";
            }

            StatusText.Text = status;

            string background = "#E0F2FE";
            string foreground = "#0369A1";

            switch (status.Trim())
            {
                case "تم التسليم":
                    background = "#DCFCE7";
                    foreground = "#15803D";
                    break;

                case "جاهز":
                    background = "#FEF3C7";
                    foreground = "#B45309";
                    break;

                case "قيد الصيانة":
                    background = "#EDE9FE";
                    foreground = "#7C3AED";
                    break;

                case "مستلم":
                    background = "#E0F2FE";
                    foreground = "#0369A1";
                    break;

                case "ملغي":
                    background = "#FEE2E2";
                    foreground = "#DC2626";
                    break;
            }

            if (StatusText.Parent is Border statusBorder)
            {
                statusBorder.Background =
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            background));
            }

            StatusText.Foreground =
                new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(
                        foreground));
        }


        // =========================================================
        // زر الإغلاق
        // =========================================================

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // =========================================================
        // زر الطباعة
        // =========================================================

        private void PrintButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog =
                    new PrintDialog();

                if (printDialog.ShowDialog() != true)
                    return;


                // =================================================
                // إنشاء نسخة من محتوى الفاتورة للطباعة
                // =================================================

                Grid printArea =
                    CreatePrintArea();


                // =================================================
                // تحديد حجم الورق
                // =================================================

                double printableWidth =
                    printDialog.PrintableAreaWidth;

                double printableHeight =
                    printDialog.PrintableAreaHeight;

                printArea.Measure(
                    new Size(
                        printableWidth,
                        printableHeight));

                printArea.Arrange(
                    new Rect(
                        0,
                        0,
                        printableWidth,
                        printableHeight));

                printArea.UpdateLayout();


                // =================================================
                // الطباعة
                // =================================================

                printDialog.PrintVisual(
                    printArea,
                    "فاتورة صيانة - " +
                    InvoiceNumberText.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء طباعة الفاتورة:\n\n" +
                    ex.Message,
                    "خطأ في الطباعة",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // إنشاء نسخة مخصصة للطباعة
        // =========================================================

        private Grid CreatePrintArea()
        {
            Grid root =
                new Grid
                {
                    Background = Brushes.White,
                    Width = 700
                };


            // =====================================================
            // الصفوف
            // =====================================================

            root.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            root.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            root.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            root.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            root.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });


            // =====================================================
            // Header
            // =====================================================

            Border header =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(15, 23, 42)),

                    Padding =
                        new Thickness(20),

                    Margin =
                        new Thickness(0, 0, 0, 15)
                };

            Grid.SetRow(header, 0);

            Grid headerGrid = new Grid();

            headerGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

            headerGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });


            // Store

            StackPanel storePanel =
                new StackPanel();

            storePanel.Children.Add(
                new TextBlock
                {
                    Text = "THREE STORE",
                    Foreground = Brushes.White,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold
                });

            storePanel.Children.Add(
                new TextBlock
                {
                    Text = "فاتورة صيانة",
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(203, 213, 225)),
                    FontSize = 14,
                    Margin =
                        new Thickness(0, 4, 0, 0)
                });

            storePanel.Children.Add(
                new TextBlock
                {
                    Text = "خدمة وصيانة الأجهزة",
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(148, 163, 184)),
                    FontSize = 11,
                    Margin =
                        new Thickness(0, 2, 0, 0)
                });

            Grid.SetColumn(storePanel, 0);

            headerGrid.Children.Add(storePanel);


            // Invoice info

            StackPanel invoicePanel =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Left
                };

            invoicePanel.Children.Add(
                CreatePrintLabel(
                    "رقم الفاتورة"));

            invoicePanel.Children.Add(
                CreatePrintValue(
                    InvoiceNumberText.Text,
                    18));

            invoicePanel.Children.Add(
                CreatePrintLabel(
                    "الموظف",
                    7));

            invoicePanel.Children.Add(
                CreatePrintValue(
                    UsernameText.Text,
                    13));

            Grid.SetColumn(
                invoicePanel,
                1);

            headerGrid.Children.Add(
                invoicePanel);

            header.Child = headerGrid;

            root.Children.Add(header);


            // =====================================================
            // Customer
            // =====================================================

            Border customer =
                CreatePrintSection(
                    "بيانات العميل");

            Grid.SetRow(
                customer,
                1);

            Grid customerGrid =
                CreateTwoColumnGrid();

            AddPrintField(
                customerGrid,
                "اسم العميل",
                CustomerNameText.Text,
                0);

            AddPrintField(
                customerGrid,
                "رقم التليفون",
                CustomerPhoneText.Text,
                1);

            customer.Child =
                WrapPrintSection(
                    "بيانات العميل",
                    customerGrid);

            root.Children.Add(customer);


            // =====================================================
            // Device
            // =====================================================

            Border device =
                CreatePrintSection(
                    "بيانات الجهاز");

            Grid.SetRow(
                device,
                2);

            Grid deviceGrid =
                CreateTwoColumnGrid();

            AddPrintField(
                deviceGrid,
                "اسم الجهاز",
                DeviceNameText.Text,
                0);

            AddPrintField(
                deviceGrid,
                "الرقم التسلسلي",
                SerialNumberText.Text,
                1);

            AddPrintField(
                deviceGrid,
                "حالة الجهاز عند الاستلام",
                DeviceConditionText.Text,
                2);

            device.Child =
                WrapPrintSection(
                    "بيانات الجهاز",
                    deviceGrid);

            root.Children.Add(device);


            // =====================================================
            // Maintenance
            // =====================================================

            Border maintenance =
                CreatePrintSection(
                    "تفاصيل الصيانة");

            Grid.SetRow(
                maintenance,
                3);

            StackPanel maintenancePanel =
                new StackPanel();

            Grid maintenanceTop =
                CreateTwoColumnGrid();

            AddPrintField(
                maintenanceTop,
                "نوع الصيانة",
                MaintenanceTypeText.Text,
                0);

            AddPrintField(
                maintenanceTop,
                "حالة الصيانة",
                StatusText.Text,
                1);

            maintenancePanel.Children.Add(
                maintenanceTop);

            maintenancePanel.Children.Add(
                CreateLargePrintField(
                    "الأعطال / المشكلة",
                    ProblemText.Text));

            maintenancePanel.Children.Add(
                CreateLargePrintField(
                    "ملحقات الجهاز",
                    AccessoriesText.Text));

            maintenancePanel.Children.Add(
                CreateLargePrintField(
                    "الملاحظات",
                    NotesText.Text));

            // =====================================================
            // تفاصيل قطع الغيار
            // =====================================================

            maintenancePanel.Children.Add(
                CreateLargePrintField(
                    "تفاصيل قطع الغيار",
                    "-"));

            maintenance.Child =
                WrapPrintSection(
                    "تفاصيل الصيانة",
                    maintenancePanel);

            root.Children.Add(maintenance);


            // =====================================================
            // Summary
            // =====================================================

            Border summary =
                CreatePrintSection("");

            Grid.SetRow(
                summary,
                4);

            Grid summaryGrid =
                CreateThreeColumnGrid();

            AddPrintField(
                summaryGrid,
                "السعر",
                PriceText.Text,
                0);

            AddPrintField(
                summaryGrid,
                "تاريخ الاستلام",
                ReceiveDateText.Text,
                1);

            AddPrintField(
                summaryGrid,
                "موعد التسليم",
                DeliveryDateText.Text,
                2);

            summary.Child =
                WrapPrintSection(
                    "",
                    summaryGrid);

            root.Children.Add(summary);


            return root;
        }


        // =========================================================
        // أدوات تصميم الطباعة
        // =========================================================

        private static TextBlock CreatePrintLabel(
            string text,
            double topMargin = 0)
        {
            return new TextBlock
            {
                Text = text,
                Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(148, 163, 184)),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Margin =
                    new Thickness(0, topMargin, 0, 2)
            };
        }


        private static TextBlock CreatePrintValue(
            string text,
            double fontSize)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold
            };
        }


        private static Border CreatePrintSection(
            string title)
        {
            return new Border
            {
                Background = Brushes.White,
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(226, 232, 240)),
                BorderThickness =
                    new Thickness(1),
                Padding =
                    new Thickness(15),
                Margin =
                    new Thickness(0, 0, 0, 10)
            };
        }


        private static StackPanel WrapPrintSection(
            string title,
            UIElement content)
        {
            StackPanel panel =
                new StackPanel();

            if (!string.IsNullOrWhiteSpace(title))
            {
                panel.Children.Add(
                    new TextBlock
                    {
                        Text = title,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(15, 23, 42)),
                        Margin =
                            new Thickness(0, 0, 0, 10)
                    });
            }

            panel.Children.Add(content);

            return panel;
        }


        private static Grid CreateTwoColumnGrid()
        {
            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(30)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            return grid;
        }


        private static Grid CreateThreeColumnGrid()
        {
            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            return grid;
        }


        private static void AddPrintField(
            Grid grid,
            string label,
            string value,
            int logicalColumn)
        {
            int actualColumn =
                logicalColumn == 0
                    ? 0
                    : logicalColumn == 1
                        ? 2
                        : logicalColumn;

            StackPanel panel =
                new StackPanel
                {
                    Margin =
                        new Thickness(0, 0, 0, 10)
                };

            panel.Children.Add(
                new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(100, 116, 139)),
                    Margin =
                        new Thickness(0, 0, 0, 3)
                });

            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        string.IsNullOrWhiteSpace(value)
                            ? "-"
                            : value,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(23, 32, 51)),
                    TextWrapping =
                        TextWrapping.Wrap
                });

            Grid.SetColumn(
                panel,
                actualColumn);

            grid.Children.Add(panel);
        }


        private static StackPanel CreateLargePrintField(
            string label,
            string value)
        {
            StackPanel panel =
                new StackPanel
                {
                    Margin =
                        new Thickness(0, 5, 0, 8)
                };

            panel.Children.Add(
                new TextBlock
                {
                    Text = label,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(100, 116, 139)),
                    Margin =
                        new Thickness(0, 0, 0, 3)
                });

            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        string.IsNullOrWhiteSpace(value)
                            ? "-"
                            : value,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(23, 32, 51)),
                    TextWrapping =
                        TextWrapping.Wrap
                });

            return panel;
        }
    }
}