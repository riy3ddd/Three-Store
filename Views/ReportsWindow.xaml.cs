using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Views
{
    public partial class ReportsWindow : Window
    {
        // =========================================================
        // DATABASE
        // =========================================================

        private const string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";


        // =========================================================
        // FIELDS
        // =========================================================

        private DateTime currentStartDate;
        private DateTime currentEndDate;

        private List<SalesPoint> currentChartData =
            new List<SalesPoint>();

        private bool isLoadingReport = false;

        private ChartType currentChartType =
            ChartType.Bar;


        // =========================================================
        // CHART TYPE
        // =========================================================

        private enum ChartType
        {
            Bar,
            SimpleLine,
            DetailedLine
        }


        // =========================================================
        // SALES POINT
        // =========================================================

        private class SalesPoint
        {
            public DateTime Date { get; set; }

            public decimal Amount { get; set; }

            public string Label { get; set; } = "";
        }


        // =========================================================
        // TOP PRODUCT
        // =========================================================

        private class TopProduct
        {
            public int Rank { get; set; }

            public string ProductName { get; set; } = "";

            public int TotalQuantity { get; set; }

            public decimal TotalSales { get; set; }
        }


        // =========================================================
        // TOP CATEGORY
        // =========================================================

        private class TopCategory
        {
            public int Rank { get; set; }

            public string CategoryName { get; set; } = "";

            public int TotalQuantity { get; set; }

            public decimal TotalSales { get; set; }
        }


        // =========================================================
        // BEST SELLING HOUR
        // =========================================================

        private class BestSellingHour
        {
            public int Rank { get; set; }

            public int Hour { get; set; }

            public string HourLabel { get; set; } = "";

            public int InvoiceCount { get; set; }

            public decimal TotalSales { get; set; }
        }


        // =========================================================
        // ANALYSIS SUMMARY
        // =========================================================

        private class AnalysisSummary
        {
            public decimal TotalSales { get; set; }

            public int InvoiceCount { get; set; }

            public int UnitsSold { get; set; }

            public decimal TotalCost { get; set; }

            public decimal TotalProfit { get; set; }
        }


        // =========================================================
        // PRODUCT ANALYSIS
        // =========================================================

        private class ProductAnalysis
        {
            public string ProductName { get; set; } = "";

            public string CategoryName { get; set; } = "";

            public int Quantity { get; set; }

            public decimal Sales { get; set; }

            public decimal Cost { get; set; }

            public decimal Profit { get; set; }
        }


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ReportsWindow()
        {
            InitializeComponent();

            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show(
                    "ليس لديك صلاحية للوصول إلى التقارير.",
                    "غير مسموح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            PeriodComboBox.SelectedIndex = 1;

            Loaded += ReportsWindow_Loaded;
        }


        // =========================================================
        // LOADED
        // =========================================================

        private void ReportsWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadReport();
        }


        // =========================================================
        // PERIOD CHANGED
        // =========================================================

        private void PeriodComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (isLoadingReport)
                return;

            LoadReport();
        }


        // =========================================================
        // REFRESH
        // =========================================================

        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadReport();
        }


        // =========================================================
        // LOAD REPORT
        // =========================================================

        private void LoadReport()
        {
            try
            {
                isLoadingReport = true;

                GetSelectedPeriod();

                CurrentPeriodText.Text =
                    GetPeriodText();

                LoadStatistics();

                LoadSalesChart();

                LoadTopProducts();

                LoadTopCategories();

                LoadBestSellingHours();

                UpdateChartButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل التقارير:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                isLoadingReport = false;
            }
        }


        // =========================================================
        // GET SELECTED PERIOD
        // =========================================================

        private void GetSelectedPeriod()
        {
            DateTime today =
                DateTime.Today;

            switch (PeriodComboBox.SelectedIndex)
            {
                case 0:

                    currentStartDate =
                        today;

                    currentEndDate =
                        today.AddDays(1);

                    break;


                case 1:

                    currentStartDate =
                        today.AddDays(-6);

                    currentEndDate =
                        today.AddDays(1);

                    break;


                case 2:

                    currentStartDate =
                        today.AddDays(-29);

                    currentEndDate =
                        today.AddDays(1);

                    break;


                case 3:

                    DateTime firstOfMonth =
                        new DateTime(
                            today.Year,
                            today.Month,
                            1);

                    currentStartDate =
                        firstOfMonth.AddMonths(-1);

                    currentEndDate =
                        firstOfMonth.AddMonths(1);

                    break;


                case 4:

                    firstOfMonth =
                        new DateTime(
                            today.Year,
                            today.Month,
                            1);

                    currentStartDate =
                        firstOfMonth.AddMonths(-2);

                    currentEndDate =
                        firstOfMonth.AddMonths(1);

                    break;


                case 5:

                    firstOfMonth =
                        new DateTime(
                            today.Year,
                            today.Month,
                            1);

                    currentStartDate =
                        firstOfMonth.AddMonths(-5);

                    currentEndDate =
                        firstOfMonth.AddMonths(1);

                    break;


                case 6:

                    firstOfMonth =
                        new DateTime(
                            today.Year,
                            today.Month,
                            1);

                    currentStartDate =
                        firstOfMonth.AddMonths(-11);

                    currentEndDate =
                        firstOfMonth.AddMonths(1);

                    break;


                case 7:

                    firstOfMonth =
                        new DateTime(
                            today.Year,
                            today.Month,
                            1);

                    currentStartDate =
                        firstOfMonth.AddMonths(-23);

                    currentEndDate =
                        firstOfMonth.AddMonths(1);

                    break;


                default:

                    currentStartDate =
                        today.AddDays(-6);

                    currentEndDate =
                        today.AddDays(1);

                    break;
            }
        }


        // =========================================================
        // PERIOD TEXT
        // =========================================================

        private string GetPeriodText()
        {
            DateTime endDisplay =
                currentEndDate.AddDays(-1);

            return
                $"من {currentStartDate:dd/MM/yyyy} " +
                $"إلى {endDisplay:dd/MM/yyyy}";
        }


        // =========================================================
        // LOAD STATISTICS
        // =========================================================

        private void LoadStatistics()
        {
            decimal totalSales = 0;

            int invoiceCount = 0;

            int productsSold = 0;

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();


            // -----------------------------------------------------
            // TOTAL SALES
            // -----------------------------------------------------

            using SqlCommand salesCommand =
                new SqlCommand(
                    @"
                    SELECT ISNULL(SUM(Total), 0)
                    FROM Sales
                    WHERE SaleDate >= @StartDate
                      AND SaleDate < @EndDate
                    ",
                    connection);

            salesCommand.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            salesCommand.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);

            totalSales =
                Convert.ToDecimal(
                    salesCommand.ExecuteScalar());


            // -----------------------------------------------------
            // INVOICE COUNT
            // -----------------------------------------------------

            using SqlCommand invoiceCommand =
                new SqlCommand(
                    @"
                    SELECT COUNT(*)
                    FROM Sales
                    WHERE SaleDate >= @StartDate
                      AND SaleDate < @EndDate
                    ",
                    connection);

            invoiceCommand.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            invoiceCommand.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);

            invoiceCount =
                Convert.ToInt32(
                    invoiceCommand.ExecuteScalar());


            // -----------------------------------------------------
            // PRODUCTS SOLD
            // -----------------------------------------------------

            using SqlCommand productsCommand =
                new SqlCommand(
                    @"
                    SELECT ISNULL(SUM(SI.Quantity), 0)
                    FROM SaleItems SI
                    INNER JOIN Sales S
                        ON SI.SaleID = S.SaleID
                    WHERE S.SaleDate >= @StartDate
                      AND S.SaleDate < @EndDate
                    ",
                    connection);

            productsCommand.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            productsCommand.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);

            productsSold =
                Convert.ToInt32(
                    productsCommand.ExecuteScalar());


            // -----------------------------------------------------
            // DISPLAY
            // -----------------------------------------------------

            TotalSalesText.Text =
                $"{totalSales:N2} جنيه";

            InvoicesCountText.Text =
                invoiceCount.ToString("N0");

            ProductsSoldText.Text =
                productsSold.ToString("N0");

            decimal averageInvoice =
                invoiceCount > 0
                    ? totalSales / invoiceCount
                    : 0;

            AverageInvoiceText.Text =
                $"{averageInvoice:N2} جنيه";


            LoadTrend();
        }


        // =========================================================
        // LOAD TREND
        // =========================================================

        private void LoadTrend()
        {
            TimeSpan periodLength =
                currentEndDate - currentStartDate;

            DateTime previousEnd =
                currentStartDate;

            DateTime previousStart =
                previousEnd - periodLength;


            decimal currentSales =
                GetSalesAmount(
                    currentStartDate,
                    currentEndDate);

            decimal previousSales =
                GetSalesAmount(
                    previousStart,
                    previousEnd);


            if (previousSales == 0)
            {
                TrendText.Text =
                    currentSales > 0
                        ? "زيادة"
                        : "مستقر";

                TrendPercentageText.Text =
                    currentSales > 0
                        ? "جديد"
                        : "0%";

                return;
            }


            decimal percentage =
                ((currentSales - previousSales)
                / previousSales) * 100;


            if (percentage > 0)
            {
                TrendText.Text =
                    "↗ زيادة";
            }
            else if (percentage < 0)
            {
                TrendText.Text =
                    "↘ انخفاض";
            }
            else
            {
                TrendText.Text =
                    "→ مستقر";
            }


            TrendPercentageText.Text =
                $"{percentage:+0.00;-0.00;0.00}%";
        }


        // =========================================================
        // GET SALES AMOUNT
        // =========================================================

        private decimal GetSalesAmount(
            DateTime startDate,
            DateTime endDate)
        {
            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            using SqlCommand command =
                new SqlCommand(
                    @"
                    SELECT ISNULL(SUM(Total), 0)
                    FROM Sales
                    WHERE SaleDate >= @StartDate
                      AND SaleDate < @EndDate
                    ",
                    connection);

            command.Parameters.AddWithValue(
                "@StartDate",
                startDate);

            command.Parameters.AddWithValue(
                "@EndDate",
                endDate);

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }


        // =========================================================
        // LOAD TOP PRODUCTS
        // =========================================================

        private void LoadTopProducts()
        {
            List<TopProduct> products =
                new List<TopProduct>();

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            using SqlCommand command =
                new SqlCommand(
                    @"
                    SELECT TOP 5
                        P.ProductName,
                        SUM(SI.Quantity) AS TotalQuantity,
                        SUM(SI.Total) AS TotalSales
                    FROM SaleItems SI
                    INNER JOIN Sales S
                        ON SI.SaleID = S.SaleID
                    INNER JOIN Products P
                        ON SI.ProductID = P.ProductID
                    WHERE S.SaleDate >= @StartDate
                      AND S.SaleDate < @EndDate
                    GROUP BY
                        P.ProductName
                    ORDER BY
                        SUM(SI.Quantity) DESC,
                        SUM(SI.Total) DESC;
                    ",
                    connection);

            command.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            command.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);


            using SqlDataReader reader =
                command.ExecuteReader();

            int rank = 1;

            while (reader.Read())
            {
                products.Add(
                    new TopProduct
                    {
                        Rank = rank++,

                        ProductName =
                            reader["ProductName"]
                                .ToString() ?? "",

                        TotalQuantity =
                            Convert.ToInt32(
                                reader["TotalQuantity"]),

                        TotalSales =
                            Convert.ToDecimal(
                                reader["TotalSales"])
                    });
            }


            TopProductsItemsControl.ItemsSource =
                products;
        }


        // =========================================================
        // LOAD TOP CATEGORIES
        // =========================================================

        private void LoadTopCategories()
        {
            List<TopCategory> categories =
                new List<TopCategory>();

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            using SqlCommand command =
                new SqlCommand(
                    @"
                    SELECT TOP 5
                        ISNULL(C.CategoryName, N'بدون تصنيف')
                            AS CategoryName,

                        SUM(SI.Quantity)
                            AS TotalQuantity,

                        SUM(SI.Total)
                            AS TotalSales

                    FROM SaleItems SI

                    INNER JOIN Sales S
                        ON SI.SaleID = S.SaleID

                    INNER JOIN Products P
                        ON SI.ProductID = P.ProductID

                    LEFT JOIN Categories C
                        ON P.CategoryID = C.CategoryID

                    WHERE S.SaleDate >= @StartDate
                      AND S.SaleDate < @EndDate

                    GROUP BY
                        C.CategoryName

                    ORDER BY
                        SUM(SI.Quantity) DESC,
                        SUM(SI.Total) DESC;
                    ",
                    connection);

            command.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            command.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);


            using SqlDataReader reader =
                command.ExecuteReader();

            int rank = 1;

            while (reader.Read())
            {
                categories.Add(
                    new TopCategory
                    {
                        Rank = rank++,

                        CategoryName =
                            reader["CategoryName"]
                                .ToString() ?? "",

                        TotalQuantity =
                            Convert.ToInt32(
                                reader["TotalQuantity"]),

                        TotalSales =
                            Convert.ToDecimal(
                                reader["TotalSales"])
                    });
            }


            TopCategoriesItemsControl.ItemsSource =
                categories;
        }


        // =========================================================
        // LOAD BEST SELLING HOURS
        // =========================================================

        private void LoadBestSellingHours()
        {
            List<BestSellingHour> hours =
                new List<BestSellingHour>();

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            using SqlCommand command =
                new SqlCommand(
                    @"
                    SELECT TOP 5

                        DATEPART(HOUR, S.SaleDate)
                            AS SaleHour,

                        COUNT(DISTINCT S.SaleID)
                            AS InvoiceCount,

                        SUM(S.Total)
                            AS TotalSales

                    FROM Sales S

                    WHERE S.SaleDate >= @StartDate
                      AND S.SaleDate < @EndDate

                    GROUP BY
                        DATEPART(HOUR, S.SaleDate)

                    ORDER BY
                        SUM(S.Total) DESC;
                    ",
                    connection);

            command.Parameters.AddWithValue(
                "@StartDate",
                currentStartDate);

            command.Parameters.AddWithValue(
                "@EndDate",
                currentEndDate);


            using SqlDataReader reader =
                command.ExecuteReader();

            int rank = 1;

            while (reader.Read())
            {
                int hour =
                    Convert.ToInt32(
                        reader["SaleHour"]);

                hours.Add(
                    new BestSellingHour
                    {
                        Rank = rank++,

                        Hour = hour,

                        HourLabel =
                            $"{hour:00}:00 - {(hour + 1) % 24:00}:00",

                        InvoiceCount =
                            Convert.ToInt32(
                                reader["InvoiceCount"]),

                        TotalSales =
                            Convert.ToDecimal(
                                reader["TotalSales"])
                    });
            }


            BestHoursItemsControl.ItemsSource =
                hours;
        }


        // =========================================================
        // LOAD SALES CHART
        // =========================================================

        private void LoadSalesChart()
        {
            List<SalesPoint> points =
                new List<SalesPoint>();

            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();


            // -----------------------------------------------------
            // HOURLY - TODAY
            // -----------------------------------------------------

            if (PeriodComboBox.SelectedIndex == 0)
            {
                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            DATEPART(HOUR, SaleDate) AS SaleHour,
                            ISNULL(SUM(Total), 0) AS TotalSales
                        FROM Sales
                        WHERE SaleDate >= @StartDate
                          AND SaleDate < @EndDate
                        GROUP BY DATEPART(HOUR, SaleDate)
                        ORDER BY DATEPART(HOUR, SaleDate);
                        ",
                        connection);

                command.Parameters.AddWithValue(
                    "@StartDate",
                    currentStartDate);

                command.Parameters.AddWithValue(
                    "@EndDate",
                    currentEndDate);


                using SqlDataReader reader =
                    command.ExecuteReader();

                Dictionary<int, decimal> values =
                    new Dictionary<int, decimal>();

                while (reader.Read())
                {
                    values[
                        Convert.ToInt32(
                            reader["SaleHour"])] =
                        Convert.ToDecimal(
                            reader["TotalSales"]);
                }


                for (int hour = 0; hour < 24; hour++)
                {
                    decimal amount =
                        values.ContainsKey(hour)
                            ? values[hour]
                            : 0;

                    points.Add(
                        new SalesPoint
                        {
                            Date =
                                currentStartDate
                                    .AddHours(hour),

                            Amount = amount,

                            Label =
                                $"{hour:00}:00"
                        });
                }
            }


            // -----------------------------------------------------
            // DAILY
            // -----------------------------------------------------

            else if (
                PeriodComboBox.SelectedIndex == 1 ||
                PeriodComboBox.SelectedIndex == 2)
            {
                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            CAST(SaleDate AS DATE) AS SaleDay,
                            ISNULL(SUM(Total), 0) AS TotalSales
                        FROM Sales
                        WHERE SaleDate >= @StartDate
                          AND SaleDate < @EndDate
                        GROUP BY CAST(SaleDate AS DATE)
                        ORDER BY CAST(SaleDate AS DATE);
                        ",
                        connection);

                command.Parameters.AddWithValue(
                    "@StartDate",
                    currentStartDate);

                command.Parameters.AddWithValue(
                    "@EndDate",
                    currentEndDate);


                using SqlDataReader reader =
                    command.ExecuteReader();

                Dictionary<DateTime, decimal> values =
                    new Dictionary<DateTime, decimal>();

                while (reader.Read())
                {
                    DateTime date =
                        Convert.ToDateTime(
                            reader["SaleDay"]);

                    values[date] =
                        Convert.ToDecimal(
                            reader["TotalSales"]);
                }


                DateTime current =
                    currentStartDate.Date;

                while (current < currentEndDate.Date)
                {
                    decimal amount =
                        values.ContainsKey(current)
                            ? values[current]
                            : 0;

                    points.Add(
                        new SalesPoint
                        {
                            Date = current,

                            Amount = amount,

                            Label =
                                current.ToString("dd/MM")
                        });

                    current =
                        current.AddDays(1);
                }
            }


            // -----------------------------------------------------
            // MONTHLY
            // -----------------------------------------------------

            else
            {
                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT
                            YEAR(SaleDate) AS SaleYear,
                            MONTH(SaleDate) AS SaleMonth,
                            ISNULL(SUM(Total), 0) AS TotalSales
                        FROM Sales
                        WHERE SaleDate >= @StartDate
                          AND SaleDate < @EndDate
                        GROUP BY
                            YEAR(SaleDate),
                            MONTH(SaleDate)
                        ORDER BY
                            YEAR(SaleDate),
                            MONTH(SaleDate);
                        ",
                        connection);

                command.Parameters.AddWithValue(
                    "@StartDate",
                    currentStartDate);

                command.Parameters.AddWithValue(
                    "@EndDate",
                    currentEndDate);


                using SqlDataReader reader =
                    command.ExecuteReader();

                Dictionary<DateTime, decimal> values =
                    new Dictionary<DateTime, decimal>();

                while (reader.Read())
                {
                    int year =
                        Convert.ToInt32(
                            reader["SaleYear"]);

                    int month =
                        Convert.ToInt32(
                            reader["SaleMonth"]);

                    DateTime date =
                        new DateTime(
                            year,
                            month,
                            1);

                    values[date] =
                        Convert.ToDecimal(
                            reader["TotalSales"]);
                }


                DateTime monthCursor =
                    new DateTime(
                        currentStartDate.Year,
                        currentStartDate.Month,
                        1);

                DateTime endMonth =
                    new DateTime(
                        currentEndDate.Year,
                        currentEndDate.Month,
                        1);


                while (monthCursor < endMonth)
                {
                    decimal amount =
                        values.ContainsKey(monthCursor)
                            ? values[monthCursor]
                            : 0;

                    points.Add(
                        new SalesPoint
                        {
                            Date = monthCursor,

                            Amount = amount,

                            Label =
                                monthCursor
                                    .ToString("MM/yyyy")
                        });

                    monthCursor =
                        monthCursor.AddMonths(1);
                }
            }


            currentChartData =
                points;

            ChartTotalText.Text =
                $"إجمالي المبيعات: " +
                $"{points.Sum(x => x.Amount):N2} جنيه";

            DrawChart();
        }


        // =========================================================
        // CHART BUTTONS
        // =========================================================

        private void BarChartButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            currentChartType =
                ChartType.Bar;

            UpdateChartButtons();

            DrawChart();
        }


        private void SimpleLineButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            currentChartType =
                ChartType.SimpleLine;

            UpdateChartButtons();

            DrawChart();
        }


        private void DetailedLineButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            currentChartType =
                ChartType.DetailedLine;

            UpdateChartButtons();

            DrawChart();
        }


        // =========================================================
        // NEW - CHART ANALYSIS BUTTON
        // =========================================================

        private void ChartAnalysisButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string chartTypeName =
                    GetChartTypeName();

                ChartAnalysisWindow window =
                    new ChartAnalysisWindow(
                        ConnectionString,
                        currentStartDate,
                        currentEndDate,
                        currentChartData,
                        chartTypeName,
                        GetPeriodText());

                window.Owner = this;

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء فتح تحليل الرسم البياني:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // GET CHART TYPE NAME
        // =========================================================

        private string GetChartTypeName()
        {
            switch (currentChartType)
            {
                case ChartType.Bar:
                    return "رسم أعمدة";

                case ChartType.SimpleLine:
                    return "رسم خطي";

                case ChartType.DetailedLine:
                    return "رسم خطي مفصل";

                default:
                    return "رسم بياني";
            }
        }


        // =========================================================
        // UPDATE CHART BUTTONS
        // =========================================================

        private void UpdateChartButtons()
        {
            Brush selectedBackground =
                new SolidColorBrush(
                    Color.FromRgb(
                        37,
                        99,
                        235));

            Brush selectedForeground =
                Brushes.White;

            Brush normalBackground =
                new SolidColorBrush(
                    Color.FromRgb(
                        226,
                        232,
                        240));

            Brush normalForeground =
                new SolidColorBrush(
                    Color.FromRgb(
                        51,
                        65,
                        85));


            BarChartButton.Background =
                currentChartType == ChartType.Bar
                    ? selectedBackground
                    : normalBackground;

            BarChartButton.Foreground =
                currentChartType == ChartType.Bar
                    ? selectedForeground
                    : normalForeground;


            SimpleLineButton.Background =
                currentChartType == ChartType.SimpleLine
                    ? selectedBackground
                    : normalBackground;

            SimpleLineButton.Foreground =
                currentChartType == ChartType.SimpleLine
                    ? selectedForeground
                    : normalForeground;


            DetailedLineButton.Background =
                currentChartType == ChartType.DetailedLine
                    ? selectedBackground
                    : normalBackground;

            DetailedLineButton.Foreground =
                currentChartType == ChartType.DetailedLine
                    ? selectedForeground
                    : normalForeground;
        }


        // =========================================================
        // DRAW CHART
        // =========================================================

        private void DrawChart()
        {
            if (SalesChartCanvas == null)
                return;

            SalesChartCanvas.Children.Clear();

            if (currentChartData == null ||
                currentChartData.Count == 0)
                return;


            switch (currentChartType)
            {
                case ChartType.Bar:

                    DrawBarChart();

                    break;

                case ChartType.SimpleLine:

                    DrawLineChart(false);

                    break;

                case ChartType.DetailedLine:

                    DrawLineChart(true);

                    break;
            }
        }


        // =========================================================
        // BAR CHART
        // =========================================================

        private void DrawBarChart()
        {
            double canvasWidth =
                Math.Max(
                    SalesChartCanvas.ActualWidth,
                    1000);

            double chartHeight =
                320;

            double left =
                60;

            double right =
                30;

            double top =
                20;

            double bottom =
                55;


            decimal maxValue =
                currentChartData
                    .Max(x => x.Amount);


            if (maxValue <= 0)
                maxValue = 1;


            double chartWidth =
                canvasWidth -
                left -
                right;


            double slotWidth =
                chartWidth /
                Math.Max(
                    currentChartData.Count,
                    1);


            double barWidth =
                Math.Max(
                    slotWidth * 0.65,
                    8);


            DrawChartAxes(
                left,
                top,
                chartWidth,
                chartHeight);


            for (int i = 0;
                 i < currentChartData.Count;
                 i++)
            {
                SalesPoint point =
                    currentChartData[i];


                double barHeight =
                    (double)(
                        point.Amount /
                        maxValue
                    ) * chartHeight;


                double x =
                    left +
                    i * slotWidth +
                    (slotWidth - barWidth) / 2;


                double y =
                    top +
                    chartHeight -
                    barHeight;


                Rectangle bar =
                    new Rectangle
                    {
                        Width = barWidth,

                        Height =
                            Math.Max(
                                barHeight,
                                1),

                        Fill =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    37,
                                    99,
                                    235)),

                        RadiusX = 5,

                        RadiusY = 5
                    };


                Canvas.SetLeft(
                    bar,
                    x);

                Canvas.SetTop(
                    bar,
                    y);


                SalesChartCanvas.Children.Add(
                    bar);


                if (ShouldShowLabel(i))
                {
                    TextBlock label =
                        new TextBlock
                        {
                            Text = point.Label,

                            FontSize = 10,

                            Foreground =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        100,
                                        116,
                                        139))
                        };


                    Canvas.SetLeft(
                        label,
                        x);

                    Canvas.SetTop(
                        label,
                        top + chartHeight + 10);


                    SalesChartCanvas.Children.Add(
                        label);
                }


                if (point.Amount > 0 &&
                    ShouldShowValue(i))
                {
                    TextBlock value =
                        new TextBlock
                        {
                            Text =
                                point.Amount
                                    .ToString("N0"),

                            FontSize = 10,

                            Foreground =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        15,
                                        23,
                                        42))
                        };


                    Canvas.SetLeft(
                        value,
                        x);

                    Canvas.SetTop(
                        value,
                        Math.Max(
                            y - 20,
                            0));


                    SalesChartCanvas.Children.Add(
                        value);
                }
            }
        }


        // =========================================================
        // LINE CHART
        // =========================================================

        private void DrawLineChart(
            bool detailed)
        {
            double canvasWidth =
                Math.Max(
                    SalesChartCanvas.ActualWidth,
                    1000);

            double chartHeight =
                320;

            double left =
                60;

            double right =
                30;

            double top =
                20;


            double chartWidth =
                canvasWidth -
                left -
                right;


            decimal maxValue =
                currentChartData
                    .Max(x => x.Amount);


            if (maxValue <= 0)
                maxValue = 1;


            DrawChartAxes(
                left,
                top,
                chartWidth,
                chartHeight);


            List<Point> points =
                new List<Point>();


            double step =
                currentChartData.Count > 1
                    ? chartWidth /
                      (currentChartData.Count - 1)
                    : chartWidth;


            for (int i = 0;
                 i < currentChartData.Count;
                 i++)
            {
                SalesPoint point =
                    currentChartData[i];


                double x =
                    left +
                    i * step;


                double y =
                    top +
                    chartHeight -
                    ((double)(
                        point.Amount /
                        maxValue) *
                     chartHeight);


                points.Add(
                    new Point(
                        x,
                        y));
            }


            for (int i = 0;
                 i < points.Count - 1;
                 i++)
            {
                Line line =
                    new Line
                    {
                        X1 =
                            points[i].X,

                        Y1 =
                            points[i].Y,

                        X2 =
                            points[i + 1].X,

                        Y2 =
                            points[i + 1].Y,

                        Stroke =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    37,
                                    99,
                                    235)),

                        StrokeThickness =
                            detailed
                                ? 3
                                : 2
                    };


                SalesChartCanvas.Children.Add(
                    line);
            }


            for (int i = 0;
                 i < points.Count;
                 i++)
            {
                Point p =
                    points[i];


                Ellipse dot =
                    new Ellipse
                    {
                        Width =
                            detailed
                                ? 10
                                : 8,

                        Height =
                            detailed
                                ? 10
                                : 8,

                        Fill =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    37,
                                    99,
                                    235))
                    };


                Canvas.SetLeft(
                    dot,
                    p.X -
                    dot.Width / 2);

                Canvas.SetTop(
                    dot,
                    p.Y -
                    dot.Height / 2);


                SalesChartCanvas.Children.Add(
                    dot);


                if (ShouldShowLabel(i))
                {
                    TextBlock label =
                        new TextBlock
                        {
                            Text =
                                currentChartData[i]
                                    .Label,

                            FontSize = 10,

                            Foreground =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        100,
                                        116,
                                        139))
                        };


                    Canvas.SetLeft(
                        label,
                        p.X - 10);

                    Canvas.SetTop(
                        label,
                        top +
                        chartHeight +
                        10);


                    SalesChartCanvas.Children.Add(
                        label);
                }


                if (detailed &&
                    currentChartData[i]
                        .Amount > 0)
                {
                    TextBlock value =
                        new TextBlock
                        {
                            Text =
                                currentChartData[i]
                                    .Amount
                                    .ToString("N0"),

                            FontSize = 10,

                            Foreground =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        15,
                                        23,
                                        42))
                        };


                    Canvas.SetLeft(
                        value,
                        p.X - 15);

                    Canvas.SetTop(
                        value,
                        p.Y - 25);


                    SalesChartCanvas.Children.Add(
                        value);
                }
            }
        }


        // =========================================================
        // CHART AXES
        // =========================================================

        private void DrawChartAxes(
            double left,
            double top,
            double chartWidth,
            double chartHeight)
        {
            Line xAxis =
                new Line
                {
                    X1 = left,

                    Y1 =
                        top +
                        chartHeight,

                    X2 =
                        left +
                        chartWidth,

                    Y2 =
                        top +
                        chartHeight,

                    Stroke =
                        new SolidColorBrush(
                            Color.FromRgb(
                                203,
                                213,
                                225)),

                    StrokeThickness = 1
                };


            Line yAxis =
                new Line
                {
                    X1 = left,

                    Y1 = top,

                    X2 = left,

                    Y2 =
                        top +
                        chartHeight,

                    Stroke =
                        new SolidColorBrush(
                            Color.FromRgb(
                                203,
                                213,
                                225)),

                    StrokeThickness = 1
                };


            SalesChartCanvas.Children.Add(
                xAxis);

            SalesChartCanvas.Children.Add(
                yAxis);
        }


        // =========================================================
        // SHOW LABEL
        // =========================================================

        private bool ShouldShowLabel(
            int index)
        {
            int count =
                currentChartData.Count;


            if (count <= 12)
                return true;

            if (count <= 31)
                return index % 3 == 0;

            return index % 5 == 0;
        }


        private bool ShouldShowValue(
            int index)
        {
            int count =
                currentChartData.Count;

            if (count <= 12)
                return true;

            return index % 5 == 0;
        }


        // =========================================================
        // CANVAS SIZE
        // =========================================================

        private void SalesChartCanvas_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (currentChartData == null ||
                currentChartData.Count == 0)
                return;

            DrawChart();
        }


        // =========================================================
        // =========================================================
        // CHART ANALYSIS WINDOW
        // =========================================================
        // =========================================================

        private class ChartAnalysisWindow : Window
        {
            private readonly string connectionString;

            private readonly DateTime startDate;

            private readonly DateTime endDate;

            private readonly List<SalesPoint> chartData;

            private readonly string chartTypeName;

            private readonly string periodText;


            // =====================================================
            // CONSTRUCTOR
            // =====================================================

            public ChartAnalysisWindow(
                string connectionString,
                DateTime startDate,
                DateTime endDate,
                List<SalesPoint> chartData,
                string chartTypeName,
                string periodText)
            {
                this.connectionString =
                    connectionString;

                this.startDate =
                    startDate;

                this.endDate =
                    endDate;

                this.chartData =
                    chartData ??
                    new List<SalesPoint>();

                this.chartTypeName =
                    chartTypeName;

                this.periodText =
                    periodText;


                Title =
                    "تحليل الرسم البياني";

                Width = 1050;

                Height = 780;

                MinWidth = 800;

                MinHeight = 600;

                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner;

                FlowDirection =
                    FlowDirection.RightToLeft;

                Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            248,
                            250,
                            252));

                FontFamily =
                    new FontFamily(
                        "29 LT BUKRA REGULAR");


                LoadAnalysis();
            }


            // =====================================================
            // LOAD ANALYSIS
            // =====================================================

            private void LoadAnalysis()
            {
                try
                {
                    AnalysisSummary summary =
                        GetSummary();

                    List<ProductAnalysis> products =
                        GetProductAnalysis();

                    BuildInterface(
                        summary,
                        products);
                }
                catch (Exception ex)
                {
                    Content =
                        new TextBlock
                        {
                            Text =
                                "حدث خطأ أثناء تحميل التحليل:\n\n" +
                                ex.Message,

                            FontSize = 16,

                            Margin =
                                new Thickness(30),

                            TextWrapping =
                                TextWrapping.Wrap
                        };
                }
            }


            // =====================================================
            // GET SUMMARY
            // =====================================================

            private AnalysisSummary GetSummary()
            {
                AnalysisSummary result =
                    new AnalysisSummary();


                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                connection.Open();


                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT

                        (
                            SELECT ISNULL(SUM(Total), 0)
                            FROM Sales
                            WHERE SaleDate >= @StartDate
                              AND SaleDate < @EndDate
                        ) AS TotalSales,

                        (
                            SELECT COUNT(*)
                            FROM Sales
                            WHERE SaleDate >= @StartDate
                              AND SaleDate < @EndDate
                        ) AS InvoiceCount,

                        (
                            SELECT ISNULL(SUM(SI.Quantity), 0)
                            FROM SaleItems SI
                            INNER JOIN Sales S
                                ON SI.SaleID = S.SaleID
                            WHERE S.SaleDate >= @StartDate
                              AND S.SaleDate < @EndDate
                        ) AS UnitsSold,

                        (
                            SELECT ISNULL(
                                SUM(
                                    SI.Quantity *
                                    ISNULL(P.PurchasePrice, 0)
                                ),
                                0
                            )
                            FROM SaleItems SI
                            INNER JOIN Sales S
                                ON SI.SaleID = S.SaleID
                            INNER JOIN Products P
                                ON SI.ProductID = P.ProductID
                            WHERE S.SaleDate >= @StartDate
                              AND S.SaleDate < @EndDate
                        ) AS TotalCost,

                        (
                            SELECT ISNULL(
                                SUM(
                                    SI.Total -
                                    (
                                        SI.Quantity *
                                        ISNULL(P.PurchasePrice, 0)
                                    )
                                ),
                                0
                            )
                            FROM SaleItems SI
                            INNER JOIN Sales S
                                ON SI.SaleID = S.SaleID
                            INNER JOIN Products P
                                ON SI.ProductID = P.ProductID
                            WHERE S.SaleDate >= @StartDate
                              AND S.SaleDate < @EndDate
                        ) AS TotalProfit;
                        ",
                        connection);


                command.Parameters.AddWithValue(
                    "@StartDate",
                    startDate);

                command.Parameters.AddWithValue(
                    "@EndDate",
                    endDate);


                using SqlDataReader reader =
                    command.ExecuteReader();


                if (reader.Read())
                {
                    result.TotalSales =
                        Convert.ToDecimal(
                            reader["TotalSales"]);

                    result.InvoiceCount =
                        Convert.ToInt32(
                            reader["InvoiceCount"]);

                    result.UnitsSold =
                        Convert.ToInt32(
                            reader["UnitsSold"]);

                    result.TotalCost =
                        Convert.ToDecimal(
                            reader["TotalCost"]);

                    result.TotalProfit =
                        Convert.ToDecimal(
                            reader["TotalProfit"]);
                }


                return result;
            }


            // =====================================================
            // GET PRODUCT ANALYSIS
            // =====================================================

            private List<ProductAnalysis>
                GetProductAnalysis()
            {
                List<ProductAnalysis> products =
                    new List<ProductAnalysis>();


                using SqlConnection connection =
                    new SqlConnection(
                        connectionString);

                connection.Open();


                using SqlCommand command =
                    new SqlCommand(
                        @"
                        SELECT

                            P.ProductName,

                            ISNULL(
                                C.CategoryName,
                                N'بدون تصنيف'
                            ) AS CategoryName,

                            SUM(SI.Quantity)
                                AS Quantity,

                            SUM(SI.Total)
                                AS Sales,

                            SUM(
                                SI.Quantity *
                                ISNULL(P.PurchasePrice, 0)
                            ) AS Cost,

                            SUM(
                                SI.Total -
                                (
                                    SI.Quantity *
                                    ISNULL(P.PurchasePrice, 0)
                                )
                            ) AS Profit

                        FROM SaleItems SI

                        INNER JOIN Sales S
                            ON SI.SaleID = S.SaleID

                        INNER JOIN Products P
                            ON SI.ProductID = P.ProductID

                        LEFT JOIN Categories C
                            ON P.CategoryID = C.CategoryID

                        WHERE S.SaleDate >= @StartDate
                          AND S.SaleDate < @EndDate

                        GROUP BY
                            P.ProductID,
                            P.ProductName,
                            C.CategoryName

                        ORDER BY
                            SUM(SI.Quantity) DESC,
                            SUM(SI.Total) DESC;
                        ",
                        connection);


                command.Parameters.AddWithValue(
                    "@StartDate",
                    startDate);

                command.Parameters.AddWithValue(
                    "@EndDate",
                    endDate);


                using SqlDataReader reader =
                    command.ExecuteReader();


                while (reader.Read())
                {
                    products.Add(
                        new ProductAnalysis
                        {
                            ProductName =
                                reader["ProductName"]
                                    .ToString() ?? "",

                            CategoryName =
                                reader["CategoryName"]
                                    .ToString() ?? "",

                            Quantity =
                                Convert.ToInt32(
                                    reader["Quantity"]),

                            Sales =
                                Convert.ToDecimal(
                                    reader["Sales"]),

                            Cost =
                                Convert.ToDecimal(
                                    reader["Cost"]),

                            Profit =
                                Convert.ToDecimal(
                                    reader["Profit"])
                        });
                }


                return products;
            }


            // =====================================================
            // BUILD INTERFACE
            // =====================================================

            private void BuildInterface(
                AnalysisSummary summary,
                List<ProductAnalysis> products)
            {
                ScrollViewer scrollViewer =
                    new ScrollViewer
                    {
                        VerticalScrollBarVisibility =
                            ScrollBarVisibility.Auto,

                        HorizontalScrollBarVisibility =
                            ScrollBarVisibility.Disabled,

                        Padding =
                            new Thickness(25)
                    };


                StackPanel mainPanel =
                    new StackPanel();


                scrollViewer.Content =
                    mainPanel;


                // =================================================
                // HEADER
                // =================================================

                Border header =
                    CreateCard();


                StackPanel headerPanel =
                    new StackPanel();


                header.Child =
                    headerPanel;


                TextBlock title =
                    CreateText(
                        "📋 تحليل الرسم البياني",
                        27,
                        true,
                        "#0F172A");


                headerPanel.Children.Add(
                    title);


                TextBlock period =
                    CreateText(
                        periodText,
                        14,
                        false,
                        "#64748B");


                period.Margin =
                    new Thickness(
                        0,
                        7,
                        0,
                        0);


                headerPanel.Children.Add(
                    period);


                TextBlock chartType =
                    CreateText(
                        $"نوع الرسم المستخدم: {chartTypeName}",
                        13,
                        false,
                        "#64748B");


                chartType.Margin =
                    new Thickness(
                        0,
                        4,
                        0,
                        0);


                headerPanel.Children.Add(
                    chartType);


                mainPanel.Children.Add(
                    header);


                // =================================================
                // EXECUTIVE SUMMARY
                // =================================================

                mainPanel.Children.Add(
                    CreateSectionTitle(
                        "📌 الملخص العام"));


                string executiveText =
                    $"خلال الفترة المحددة تم تسجيل " +
                    $"{summary.InvoiceCount:N0} فاتورة، " +
                    $"وتم بيع إجمالي " +
                    $"{summary.UnitsSold:N0} وحدة، " +
                    $"بقيمة مبيعات إجمالية قدرها " +
                    $"{summary.TotalSales:N2} جنيه. " +
                    $"وبناءً على أسعار الشراء المسجلة حاليًا " +
                    $"للمنتجات المباعة، بلغت تكلفة المنتجات " +
                    $"المباعة {summary.TotalCost:N2} جنيه، " +
                    $"بينما بلغ إجمالي الربح " +
                    $"{summary.TotalProfit:N2} جنيه.";


                mainPanel.Children.Add(
                    CreateParagraph(
                        executiveText));


                // =================================================
                // SUMMARY CARDS
                // =================================================

                UniformGrid cards =
                    new UniformGrid
                    {
                        Columns = 4,

                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                20)
                    };


                cards.Children.Add(
                    CreateSmallCard(
                        "💰 إجمالي المبيعات",
                        $"{summary.TotalSales:N2} جنيه"));


                cards.Children.Add(
                    CreateSmallCard(
                        "🧾 عدد الفواتير",
                        summary.InvoiceCount
                            .ToString("N0")));


                cards.Children.Add(
                    CreateSmallCard(
                        "📦 الوحدات المباعة",
                        summary.UnitsSold
                            .ToString("N0")));


                decimal averageInvoice =
                    summary.InvoiceCount > 0
                        ? summary.TotalSales /
                          summary.InvoiceCount
                        : 0;


                cards.Children.Add(
                    CreateSmallCard(
                        "📊 متوسط الفاتورة",
                        $"{averageInvoice:N2} جنيه"));


                mainPanel.Children.Add(
                    cards);


                // =================================================
                // COST & PROFIT
                // =================================================

                mainPanel.Children.Add(
                    CreateSectionTitle(
                        "💵 التكلفة والربح"));


                decimal profitMargin =
                    summary.TotalSales > 0
                        ? (
                            summary.TotalProfit /
                            summary.TotalSales
                          ) * 100
                        : 0;


                decimal averageProfitPerUnit =
                    summary.UnitsSold > 0
                        ? summary.TotalProfit /
                          summary.UnitsSold
                        : 0;


                decimal averageProfitPerInvoice =
                    summary.InvoiceCount > 0
                        ? summary.TotalProfit /
                          summary.InvoiceCount
                        : 0;


                mainPanel.Children.Add(
                    CreateParagraph(
                        $"• إجمالي المبيعات: " +
                        $"{summary.TotalSales:N2} جنيه.\n\n" +

                        $"• تكلفة شراء المنتجات التي تم بيعها: " +
                        $"{summary.TotalCost:N2} جنيه.\n\n" +

                        $"• إجمالي الربح: " +
                        $"{summary.TotalProfit:N2} جنيه.\n\n" +

                        $"• هامش الربح التقريبي: " +
                        $"{profitMargin:N2}%.\n\n" +

                        $"• متوسط الربح لكل وحدة مباعة: " +
                        $"{averageProfitPerUnit:N2} جنيه.\n\n" +

                        $"• متوسط الربح لكل فاتورة: " +
                        $"{averageProfitPerInvoice:N2} جنيه."
                    ));


                // =================================================
                // PRODUCTS SOLD
                // =================================================

                mainPanel.Children.Add(
                    CreateSectionTitle(
                        "🛒 ماذا تم بيعه؟"));


                if (products.Count == 0)
                {
                    mainPanel.Children.Add(
                        CreateParagraph(
                            "لا توجد مبيعات مسجلة خلال الفترة المحددة."));
                }
                else
                {
                    DataGrid grid =
                        new DataGrid
                        {
                            AutoGenerateColumns = false,

                            IsReadOnly = true,

                            CanUserAddRows = false,

                            CanUserDeleteRows = false,

                            HeadersVisibility =
                                DataGridHeadersVisibility.Column,

                            Height = 330,

                            Margin =
                                new Thickness(
                                    0,
                                    0,
                                    0,
                                    20),

                            Background =
                                Brushes.White
                        };


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "المنتج",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "ProductName"),

                            Width = new DataGridLength(
                                1,
                                DataGridLengthUnitType.Star)
                        });


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "التصنيف",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "CategoryName"),

                            Width = 150
                        });


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "الكمية",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "Quantity"),

                            Width = 100
                        });


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "المبيعات",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "Sales")
                                {
                                    StringFormat =
                                        "{0:N2} جنيه"
                                },

                            Width = 150
                        });


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "التكلفة",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "Cost")
                                {
                                    StringFormat =
                                        "{0:N2} جنيه"
                                },

                            Width = 150
                        });


                    grid.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "الربح",

                            Binding =
                                new System.Windows.Data.Binding(
                                    "Profit")
                                {
                                    StringFormat =
                                        "{0:N2} جنيه"
                                },

                            Width = 150
                        });


                    grid.ItemsSource =
                        products;


                    mainPanel.Children.Add(
                        grid);
                }


                // =================================================
                // TOP PRODUCT
                // =================================================

                if (products.Count > 0)
                {
                    ProductAnalysis topProduct =
                        products.First();


                    mainPanel.Children.Add(
                        CreateSectionTitle(
                            "🏆 قراءة المنتجات"));


                    mainPanel.Children.Add(
                        CreateParagraph(
                            $"أكثر منتج تم بيعه من حيث عدد الوحدات " +
                            $"خلال الفترة المحددة هو " +
                            $"«{topProduct.ProductName}»، " +
                            $"بعدد {topProduct.Quantity:N0} وحدة، " +
                            $"وحقق مبيعات بقيمة " +
                            $"{topProduct.Sales:N2} جنيه، " +
                            $"وتكلفة قدرها " +
                            $"{topProduct.Cost:N2} جنيه، " +
                            $"وربح قدره " +
                            $"{topProduct.Profit:N2} جنيه."
                        ));
                }


                // =================================================
                // CHART ANALYSIS
                // =================================================

                mainPanel.Children.Add(
                    CreateSectionTitle(
                        "📈 شرح الرسم البياني"));


                mainPanel.Children.Add(
                    CreateParagraph(
                        BuildChartExplanation()));


                // =================================================
                // FINAL SUMMARY
                // =================================================

                mainPanel.Children.Add(
                    CreateSectionTitle(
                        "📝 الخلاصة"));


                string finalSummary =
                    BuildFinalSummary(
                        summary,
                        products);


                mainPanel.Children.Add(
                    CreateParagraph(
                        finalSummary));


                // =================================================
                // FOOTER
                // =================================================

                Border note =
                    CreateCard();


                note.Margin =
                    new Thickness(
                        0,
                        10,
                        0,
                        20);


                note.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            241,
                            245,
                            249));


                note.Child =
                    CreateText(
                        "ملاحظة: تكلفة الشراء والربح محسوبان " +
                        "اعتمادًا على PurchasePrice الحالي " +
                        "المسجل للمنتجات في قاعدة البيانات. " +
                        "إذا تغير سعر الشراء لاحقًا، فقد يتغير " +
                        "الربح المحسوب للمبيعات القديمة.",
                        12,
                        false,
                        "#475569");


                mainPanel.Children.Add(
                    note);


                Content =
                    scrollViewer;
            }


            // =====================================================
            // BUILD CHART EXPLANATION
            // =====================================================

            private string BuildChartExplanation()
            {
                if (chartData == null ||
                    chartData.Count == 0)
                {
                    return
                        "لا توجد بيانات كافية لتحليل الرسم البياني.";
                }


                decimal chartTotal =
                    chartData.Sum(
                        x => x.Amount);


                SalesPoint highest =
                    chartData
                        .OrderByDescending(
                            x => x.Amount)
                        .First();


                SalesPoint lowest =
                    chartData
                        .OrderBy(
                            x => x.Amount)
                        .First();


                decimal average =
                    chartData.Count > 0
                        ? chartTotal /
                          chartData.Count
                        : 0;


                string result =
                    $"الرسم المستخدم حاليًا هو " +
                    $"«{chartTypeName}». " +
                    $"كل نقطة أو عمود في الرسم يمثل " +
                    $"فترة زمنية محددة، والقيمة الموجودة " +
                    $"فيها تمثل إجمالي المبيعات خلال هذه الفترة.\n\n";


                result +=
                    $"• عدد الفترات الظاهرة في الرسم: " +
                    $"{chartData.Count:N0} فترة.\n\n";


                result +=
                    $"• إجمالي المبيعات التي يعرضها الرسم: " +
                    $"{chartTotal:N2} جنيه.\n\n";


                result +=
                    $"• أعلى فترة مبيعات كانت " +
                    $"«{highest.Label}» بقيمة " +
                    $"{highest.Amount:N2} جنيه.\n\n";


                result +=
                    $"• أقل فترة كانت " +
                    $"«{lowest.Label}» بقيمة " +
                    $"{lowest.Amount:N2} جنيه.\n\n";


                result +=
                    $"• متوسط المبيعات لكل فترة ظاهرة في الرسم: " +
                    $"{average:N2} جنيه.\n\n";


                if (chartData.Count >= 2)
                {
                    SalesPoint first =
                        chartData.First();

                    SalesPoint last =
                        chartData.Last();


                    result +=
                        $"• بداية الرسم كانت عند " +
                        $"{first.Amount:N2} جنيه " +
                        $"في فترة «{first.Label}»، " +
                        $"بينما انتهى الرسم عند " +
                        $"{last.Amount:N2} جنيه " +
                        $"في فترة «{last.Label}».\n\n";


                    if (first.Amount > 0)
                    {
                        decimal change =
                            (
                                (last.Amount -
                                 first.Amount)
                                /
                                first.Amount
                            ) * 100;


                        result +=
                            $"• التغير بين أول وآخر نقطة " +
                            $"في الرسم يساوي تقريبًا " +
                            $"{change:+0.00;-0.00;0.00}%.\n\n";
                    }
                    else if (last.Amount > 0)
                    {
                        result +=
                            "• بداية الفترة لم تسجل مبيعات، " +
                            "بينما توجد مبيعات في آخر نقطة " +
                            "من الرسم.\n\n";
                    }
                    else
                    {
                        result +=
                            "• أول وآخر نقطة في الرسم " +
                            "لا تحتويان على مبيعات.\n\n";
                    }
                }


                result +=
                    "طريقة قراءة الرسم: " +
                    "ارتفاع العمود أو النقطة يعني ارتفاع " +
                    "المبيعات في تلك الفترة، بينما انخفاضها " +
                    "يعني انخفاض قيمة المبيعات. " +
                    "والفترات التي تظهر بقيمة صفر تعني " +
                    "عدم وجود مبيعات مسجلة في قاعدة البيانات " +
                    "خلال تلك الفترة المحددة.";


                return result;
            }


            // =====================================================
            // FINAL SUMMARY
            // =====================================================

            private string BuildFinalSummary(
                AnalysisSummary summary,
                List<ProductAnalysis> products)
            {
                if (summary.InvoiceCount == 0)
                {
                    return
                        "لا توجد مبيعات خلال الفترة المحددة، " +
                        "وبالتالي لا يمكن استخراج مؤشرات فعلية " +
                        "للربح أو المنتجات الأكثر مبيعًا.";
                }


                string text =
                    $"خلال الفترة من " +
                    $"{startDate:dd/MM/yyyy} " +
                    $"إلى " +
                    $"{endDate.AddDays(-1):dd/MM/yyyy} " +
                    $"تم تسجيل " +
                    $"{summary.InvoiceCount:N0} فاتورة " +
                    $"وبيع " +
                    $"{summary.UnitsSold:N0} وحدة.\n\n";


                text +=
                    $"بلغت قيمة المبيعات " +
                    $"{summary.TotalSales:N2} جنيه، " +
                    $"بينما بلغت تكلفة المنتجات المباعة " +
                    $"{summary.TotalCost:N2} جنيه، " +
                    $"ليكون إجمالي الربح المحسوب " +
                    $"{summary.TotalProfit:N2} جنيه.\n\n";


                if (products.Count > 0)
                {
                    ProductAnalysis topQuantity =
                        products
                            .OrderByDescending(
                                x => x.Quantity)
                            .First();


                    ProductAnalysis topSales =
                        products
                            .OrderByDescending(
                                x => x.Sales)
                            .First();


                    text +=
                        $"من ناحية الكمية، المنتج الأعلى مبيعًا " +
                        $"هو «{topQuantity.ProductName}» " +
                        $"بعدد {topQuantity.Quantity:N0} وحدة.\n\n";


                    text +=
                        $"ومن ناحية قيمة المبيعات، " +
                        $"المنتج الأعلى تحقيقًا للمبيعات " +
                        $"هو «{topSales.ProductName}» " +
                        $"بقيمة {topSales.Sales:N2} جنيه.\n\n";
                }


                text +=
                    "أما الرسم البياني فيوضح توزيع المبيعات " +
                    "على الفترات الزمنية المحددة، ويمكن استخدامه " +
                    "لملاحظة الفترات ذات النشاط المرتفع والفترات " +
                    "التي لم تسجل مبيعات أو سجلت مبيعات أقل.";


                return text;
            }


            // =====================================================
            // CREATE SECTION TITLE
            // =====================================================

            private TextBlock CreateSectionTitle(
                string text)
            {
                return new TextBlock
                {
                    Text = text,

                    FontSize = 20,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                15,
                                23,
                                42)),

                    Margin =
                        new Thickness(
                            0,
                            20,
                            0,
                            12)
                };
            }


            // =====================================================
            // CREATE PARAGRAPH
            // =====================================================

            private Border CreateParagraph(
                string text)
            {
                Border border =
                    CreateCard();


                border.Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        15);


                TextBlock textBlock =
                    CreateText(
                        text,
                        14,
                        false,
                        "#334155");


                textBlock.TextWrapping =
                    TextWrapping.Wrap;

                textBlock.LineHeight =
                    26;


                border.Child =
                    textBlock;


                return border;
            }


            // =====================================================
            // CREATE CARD
            // =====================================================

            private Border CreateCard()
            {
                Border border =
                    new Border
                    {
                        Background =
                            Brushes.White,

                        CornerRadius =
                            new CornerRadius(14),

                        Padding =
                            new Thickness(20),

                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                15)
                    };


                border.Effect =
                    new System.Windows.Media.Effects
                        .DropShadowEffect
                    {
                        BlurRadius = 15,

                        ShadowDepth = 2,

                        Opacity = 0.10
                    };


                return border;
            }


            // =====================================================
            // CREATE SMALL CARD
            // =====================================================

            private Border CreateSmallCard(
                string title,
                string value)
            {
                Border border =
                    CreateCard();


                StackPanel panel =
                    new StackPanel();


                panel.Children.Add(
                    CreateText(
                        title,
                        12,
                        false,
                        "#64748B"));


                TextBlock valueText =
                    CreateText(
                        value,
                        18,
                        true,
                        "#0F172A");


                valueText.Margin =
                    new Thickness(
                        0,
                        7,
                        0,
                        0);


                panel.Children.Add(
                    valueText);


                border.Child =
                    panel;


                return border;
            }


            // =====================================================
            // CREATE TEXT
            // =====================================================

            private TextBlock CreateText(
                string text,
                double fontSize,
                bool bold,
                string foreground)
            {
                return new TextBlock
                {
                    Text = text,

                    FontSize = fontSize,

                    FontWeight =
                        bold
                            ? FontWeights.Bold
                            : FontWeights.Normal,

                    Foreground =
                        new SolidColorBrush(
                            (Color)ColorConverter
                                .ConvertFromString(
                                    foreground)),

                    TextWrapping =
                        TextWrapping.Wrap
                };
            }
        }
    }
}