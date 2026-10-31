using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WpfApp1
{
    internal class DatabaseHelper
    {
        private static readonly string connectionString =
            @"Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";


        // =========================================================
        // جلب المنتجات
        // =========================================================

        public static DataTable GetProducts()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        p.ProductID,
                        p.ProductName,
                        p.CategoryID,
                        c.CategoryName,
                        p.Quantity,
                        p.PurchasePrice,
                        p.SellingPrice
                    FROM Products p
                    LEFT JOIN Categories c
                        ON p.CategoryID = c.CategoryID
                    ORDER BY p.ProductID";

                SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection);

                adapter.Fill(table);
            }

            return table;
        }


        // =========================================================
        // جلب الأقسام
        // =========================================================
        // ProductCount هنا يتم حسابه من جدول Products
        // وليس من Categories
        // =========================================================

        public static DataTable GetCategories()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        c.CategoryID,
                        c.CategoryName,
                        COUNT(p.ProductID) AS ProductCount
                    FROM Categories c
                    LEFT JOIN Products p
                        ON c.CategoryID = p.CategoryID
                    GROUP BY
                        c.CategoryID,
                        c.CategoryName
                    ORDER BY c.CategoryID";

                SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection);

                adapter.Fill(table);
            }

            return table;
        }


        // =========================================================
        // إضافة منتج
        // =========================================================

        public static void AddProduct(
            string productName,
            int categoryId,
            int quantity,
            decimal? purchasePrice,
            decimal? sellingPrice)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Products
                    (
                        ProductName,
                        CategoryID,
                        Quantity,
                        PurchasePrice,
                        SellingPrice
                    )
                    VALUES
                    (
                        @ProductName,
                        @CategoryID,
                        @Quantity,
                        @PurchasePrice,
                        @SellingPrice
                    )";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@ProductName",
                        productName);

                    command.Parameters.AddWithValue(
                        "@CategoryID",
                        categoryId);

                    command.Parameters.AddWithValue(
                        "@Quantity",
                        quantity);

                    command.Parameters.AddWithValue(
                        "@PurchasePrice",
                        (object?)purchasePrice ?? DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@SellingPrice",
                        (object?)sellingPrice ?? DBNull.Value);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }


        // =========================================================
        // تعديل منتج
        // =========================================================

        public static void UpdateProduct(
            int productId,
            string? productName,
            int? categoryId,
            int? quantity,
            decimal? purchasePrice,
            decimal? sellingPrice)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Products
                    SET
                        ProductName =
                            COALESCE(
                                @ProductName,
                                ProductName),

                        CategoryID =
                            COALESCE(
                                @CategoryID,
                                CategoryID),

                        Quantity =
                            COALESCE(
                                @Quantity,
                                Quantity),

                        PurchasePrice =
                            COALESCE(
                                @PurchasePrice,
                                PurchasePrice),

                        SellingPrice =
                            COALESCE(
                                @SellingPrice,
                                SellingPrice)

                    WHERE ProductID = @ProductID";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@ProductID",
                        productId);

                    command.Parameters.AddWithValue(
                        "@ProductName",
                        (object?)productName ??
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@CategoryID",
                        (object?)categoryId ??
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@Quantity",
                        (object?)quantity ??
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@PurchasePrice",
                        (object?)purchasePrice ??
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@SellingPrice",
                        (object?)sellingPrice ??
                        DBNull.Value);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }


        // =========================================================
        // حذف منتج
        // =========================================================

        public static void DeleteProduct(int productId)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    DELETE FROM Products
                    WHERE ProductID = @ProductID";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@ProductID",
                        productId);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }


        // =========================================================
        // عدد المنتجات
        // =========================================================

        public static int GetTotalProducts()
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM Products";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar());
                }
            }
        }


        // =========================================================
        // عدد الأقسام بدون تكرار
        // =========================================================

        public static int GetTotalCategories()
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(DISTINCT CategoryName)
                    FROM Categories";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar());
                }
            }
        }


        // =========================================================
        // إجمالي كميات المنتجات
        // =========================================================

        public static int GetTotalQuantity()
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COALESCE(SUM(Quantity), 0)
                    FROM Products";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar());
                }
            }
        }


        // =========================================================
        // عدد المنتجات ذات المخزون المنخفض
        // =========================================================

        public static int GetLowStockProducts()
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM Products
                    WHERE Quantity < 5";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar());
                }
            }
        }


        // =========================================================
        // إضافة تصنيف + إنشاء المنتجات المطلوبة
        // =========================================================

        public static void AddCategory(
            string categoryName,
            int productCount)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction =
                       connection.BeginTransaction())
                {
                    try
                    {
                        // -----------------------------------------
                        // 1. إضافة التصنيف
                        // -----------------------------------------

                        string categoryQuery = @"
                            INSERT INTO Categories
                            (
                                CategoryName
                            )
                            OUTPUT INSERTED.CategoryID
                            VALUES
                            (
                                @CategoryName
                            )";

                        int categoryId;

                        using (SqlCommand categoryCommand =
                               new SqlCommand(
                                   categoryQuery,
                                   connection,
                                   transaction))
                        {
                            categoryCommand.Parameters.AddWithValue(
                                "@CategoryName",
                                categoryName);

                            categoryId =
                                Convert.ToInt32(
                                    categoryCommand.ExecuteScalar());
                        }


                        // -----------------------------------------
                        // 2. إنشاء المنتجات المبدئية
                        // -----------------------------------------

                        for (int i = 1; i <= productCount; i++)
                        {
                            string productQuery = @"
                                INSERT INTO Products
                                (
                                    ProductName,
                                    CategoryID,
                                    Quantity,
                                    PurchasePrice,
                                    SellingPrice
                                )
                                VALUES
                                (
                                    @ProductName,
                                    @CategoryID,
                                    0,
                                    NULL,
                                    NULL
                                )";

                            using (SqlCommand productCommand =
                                   new SqlCommand(
                                       productQuery,
                                       connection,
                                       transaction))
                            {
                                productCommand.Parameters.AddWithValue(
                                    "@ProductName",
                                    $"{categoryName} {i}");

                                productCommand.Parameters.AddWithValue(
                                    "@CategoryID",
                                    categoryId);

                                productCommand.ExecuteNonQuery();
                            }
                        }


                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // =========================================================
        // تعديل تصنيف + تعديل عدد المنتجات
        // =========================================================

        public static void UpdateCategory(
            int categoryId,
            string categoryName,
            int productCount)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction =
                       connection.BeginTransaction())
                {
                    try
                    {
                        // -----------------------------------------
                        // 1. تغيير اسم التصنيف
                        // -----------------------------------------

                        string categoryQuery = @"
                            UPDATE Categories
                            SET CategoryName = @CategoryName
                            WHERE CategoryID = @CategoryID";

                        using (SqlCommand categoryCommand =
                               new SqlCommand(
                                   categoryQuery,
                                   connection,
                                   transaction))
                        {
                            categoryCommand.Parameters.AddWithValue(
                                "@CategoryID",
                                categoryId);

                            categoryCommand.Parameters.AddWithValue(
                                "@CategoryName",
                                categoryName);

                            categoryCommand.ExecuteNonQuery();
                        }


                        // -----------------------------------------
                        // 2. معرفة العدد الحالي من Products
                        // -----------------------------------------

                        string countQuery = @"
                            SELECT COUNT(*)
                            FROM Products
                            WHERE CategoryID = @CategoryID";

                        int currentProductCount;

                        using (SqlCommand countCommand =
                               new SqlCommand(
                                   countQuery,
                                   connection,
                                   transaction))
                        {
                            countCommand.Parameters.AddWithValue(
                                "@CategoryID",
                                categoryId);

                            currentProductCount =
                                Convert.ToInt32(
                                    countCommand.ExecuteScalar());
                        }


                        // -----------------------------------------
                        // 3. لو العدد الجديد أكبر
                        // -----------------------------------------

                        if (productCount > currentProductCount)
                        {
                            int productsToAdd =
                                productCount -
                                currentProductCount;

                            for (int i = 1;
                                 i <= productsToAdd;
                                 i++)
                            {
                                string productQuery = @"
                                    INSERT INTO Products
                                    (
                                        ProductName,
                                        CategoryID,
                                        Quantity,
                                        PurchasePrice,
                                        SellingPrice
                                    )
                                    VALUES
                                    (
                                        @ProductName,
                                        @CategoryID,
                                        0,
                                        NULL,
                                        NULL
                                    )";

                                using (SqlCommand productCommand =
                                       new SqlCommand(
                                           productQuery,
                                           connection,
                                           transaction))
                                {
                                    productCommand.Parameters.AddWithValue(
                                        "@ProductName",
                                        $"{categoryName} New {i}");

                                    productCommand.Parameters.AddWithValue(
                                        "@CategoryID",
                                        categoryId);

                                    productCommand.ExecuteNonQuery();
                                }
                            }
                        }


                        // -----------------------------------------
                        // 4. لو العدد الجديد أصغر
                        // -----------------------------------------

                        else if (productCount < currentProductCount)
                        {
                            int productsToDelete =
                                currentProductCount -
                                productCount;

                            string deleteQuery = @"
                                DELETE FROM Products
                                WHERE ProductID IN
                                (
                                    SELECT TOP (@ProductsToDelete)
                                        ProductID
                                    FROM Products
                                    WHERE CategoryID = @CategoryID
                                    ORDER BY ProductID DESC
                                )";

                            using (SqlCommand deleteCommand =
                                   new SqlCommand(
                                       deleteQuery,
                                       connection,
                                       transaction))
                            {
                                deleteCommand.Parameters.AddWithValue(
                                    "@ProductsToDelete",
                                    productsToDelete);

                                deleteCommand.Parameters.AddWithValue(
                                    "@CategoryID",
                                    categoryId);

                                deleteCommand.ExecuteNonQuery();
                            }
                        }


                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // =========================================================
        // حذف تصنيف
        // =========================================================

        public static void DeleteCategory(int categoryId)
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
                    DELETE FROM Categories
                    WHERE CategoryID = @CategoryID";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@CategoryID",
                        categoryId);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }
        public static DataTable GetProductsForSales()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
            SELECT
                p.ProductID,
                p.ProductName,
                p.Quantity,

                CASE
                    WHEN o.OfferID IS NOT NULL
                        THEN o.OfferPrice
                    ELSE p.SellingPrice
                END AS SellingPrice,

                p.SellingPrice AS OriginalSellingPrice,

                CASE
                    WHEN o.OfferID IS NOT NULL
                        THEN CAST(1 AS BIT)
                    ELSE CAST(0 AS BIT)
                END AS HasOffer

            FROM Products p

            OUTER APPLY
            (
                SELECT TOP 1
                    o.OfferID,
                    o.OfferPrice
                FROM Offers o
                WHERE
                    o.ProductID = p.ProductID
                    AND o.IsActive = 1
                    AND o.StartDate <= GETDATE()
                    AND
                    (
                        o.EndDate IS NULL
                        OR GETDATE() < o.EndDate
                    )
                ORDER BY o.OfferID DESC
            ) o

            WHERE p.Quantity > 0

            ORDER BY p.ProductID;
        ";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }
        public static decimal GetEffectiveProductPrice(
    SqlConnection connection,
    SqlTransaction transaction,
    int productId)
        {
            string query = @"
        SELECT
            CASE
                WHEN o.OfferID IS NOT NULL
                    THEN o.OfferPrice
                ELSE p.SellingPrice
            END AS EffectiveSellingPrice

        FROM Products p

        OUTER APPLY
        (
            SELECT TOP 1
                o.OfferID,
                o.OfferPrice
            FROM Offers o
            WHERE
                o.ProductID = p.ProductID
                AND o.IsActive = 1
                AND o.StartDate <= GETDATE()
                AND
                (
                    o.EndDate IS NULL
                    OR GETDATE() < o.EndDate
                )
            ORDER BY o.OfferID DESC
        ) o

        WHERE p.ProductID = @ProductID;
    ";

            using (SqlCommand command =
                   new SqlCommand(
                       query,
                       connection,
                       transaction))
            {
                command.Parameters.Add(
                    "@ProductID",
                    SqlDbType.Int).Value =
                    productId;

                object result =
                    command.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    throw new Exception(
                        "المنتج ليس له سعر بيع صالح.");
                }

                return Convert.ToDecimal(result);
            }
        }
        public static DataTable GetInvoiceItems(int saleId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
            SELECT
                ROW_NUMBER() OVER (
                    ORDER BY SI.SaleItemID
                ) AS Number,

                P.ProductName,

                SI.Quantity,

                SI.UnitPrice,

                SI.Total

            FROM SaleItems SI

            INNER JOIN Products P
                ON SI.ProductID = P.ProductID

            WHERE SI.SaleID = @SaleID

            ORDER BY SI.SaleItemID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@SaleID",
                        saleId);


                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }



        public static int GetNextInvoiceNumber()
        {
            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
            SELECT ISNULL(MAX(SaleID), 0) + 1
            FROM Sales;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar());
                }
            }
        }


public static DataTable GetAllInvoices()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT
                SaleID,

                'INV-' +
                RIGHT('000000' + CAST(SaleID AS VARCHAR(6)), 6)
                AS InvoiceNumber,

                SaleDate,
                Total,
                Paid,
                Change,
                PaymentMethod,
                VodafoneCashNumber

            FROM Sales

            ORDER BY SaleID DESC;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }


        public static DataTable GetInvoiceInfo(int saleId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                string query = @"
    SELECT
        S.SaleID,
        S.SaleDate,
        S.PaymentMethod,
        S.VodafoneCashNumber,
        S.CustomerName,
        S.CustomerPhone,
        S.Subtotal,
        S.Discount,
        S.Total,
        S.Paid,
        S.Change,
        U.Username AS Username
    FROM Sales S
    LEFT JOIN Users U
        ON S.CreatedBy = U.UserID
    WHERE S.SaleID = @SaleID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@SaleID",
                        SqlDbType.Int).Value =
                        saleId;

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        // ============================================================
        // Maintenance - الصيانة
        // ============================================================

        public static string GetNextMaintenanceInvoiceNumber()
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = @"
        SELECT ISNULL(MAX(MaintenanceID), 0) + 1
        FROM MaintenanceInvoices";

            using SqlCommand command = new SqlCommand(query, connection);

            int nextId = Convert.ToInt32(command.ExecuteScalar());

            return $"MNT-{nextId:D6}";
        }


        // ------------------------------------------------------------
        // إضافة فاتورة صيانة
        // ------------------------------------------------------------

        public static int AddMaintenanceInvoice(
            string invoiceNumber,
            string customerName,
            string customerPhone,
            string deviceName,
            string serialNumber,
            string deviceCondition,
            string deviceColor,
            string problemDescription,
            string accessories,
            string notes,
            string maintenanceType,
            decimal price,
            string username,
            DateTime? deliveryDate,
            string status = "مستلم")
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = @"
        INSERT INTO MaintenanceInvoices
        (
            InvoiceNumber,
            CustomerName,
            CustomerPhone,
            DeviceName,
            SerialNumber,
            DeviceCondition,
            DeviceColor,
            ProblemDescription,
            Accessories,
            Notes,
            MaintenanceType,
            Price,
            Username,
            DeliveryDate,
            Status
        )
        VALUES
        (
            @InvoiceNumber,
            @CustomerName,
            @CustomerPhone,
            @DeviceName,
            @SerialNumber,
            @DeviceCondition,
            @DeviceColor,
            @ProblemDescription,
            @Accessories,
            @Notes,
            @MaintenanceType,
            @Price,
            @Username,
            @DeliveryDate,
            @Status
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
            command.Parameters.AddWithValue("@CustomerName", customerName);
            command.Parameters.AddWithValue(
                "@CustomerPhone",
                string.IsNullOrWhiteSpace(customerPhone)
                    ? DBNull.Value
                    : customerPhone);

            command.Parameters.AddWithValue("@DeviceName", deviceName);

            command.Parameters.AddWithValue(
                "@SerialNumber",
                string.IsNullOrWhiteSpace(serialNumber)
                    ? DBNull.Value
                    : serialNumber);

            command.Parameters.AddWithValue(
                "@DeviceCondition",
                string.IsNullOrWhiteSpace(deviceCondition)
                    ? DBNull.Value
                    : deviceCondition);

            command.Parameters.AddWithValue(
                "@DeviceColor",
                string.IsNullOrWhiteSpace(deviceColor)
                    ? DBNull.Value
                    : deviceColor);

            command.Parameters.AddWithValue(
                "@ProblemDescription",
                string.IsNullOrWhiteSpace(problemDescription)
                    ? DBNull.Value
                    : problemDescription);

            command.Parameters.AddWithValue(
                "@Accessories",
                string.IsNullOrWhiteSpace(accessories)
                    ? DBNull.Value
                    : accessories);

            command.Parameters.AddWithValue(
                "@Notes",
                string.IsNullOrWhiteSpace(notes)
                    ? DBNull.Value
                    : notes);

            command.Parameters.AddWithValue("@MaintenanceType", maintenanceType);
            command.Parameters.AddWithValue("@Price", price);

            command.Parameters.AddWithValue(
                "@Username",
                string.IsNullOrWhiteSpace(username)
                    ? DBNull.Value
                    : username);

            command.Parameters.AddWithValue(
                "@DeliveryDate",
                deliveryDate.HasValue
                    ? deliveryDate.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue("@Status", status);

            return Convert.ToInt32(command.ExecuteScalar());
        }


        // ------------------------------------------------------------
        // جلب كل فواتير الصيانة
        // ------------------------------------------------------------

        public static DataTable GetAllMaintenanceInvoices()
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            string query = @"
        SELECT
            MaintenanceID,
            InvoiceNumber,
            CustomerName,
            CustomerPhone,
            DeviceName,
            SerialNumber,
            DeviceCondition,
            DeviceColor,
            ProblemDescription,
            Accessories,
            Notes,
            MaintenanceType,
            Price,
            Username,
            ReceiveDate,
            DeliveryDate,
            Status
        FROM MaintenanceInvoices
        ORDER BY MaintenanceID DESC";

            using SqlCommand command = new SqlCommand(query, connection);

            using SqlDataAdapter adapter = new SqlDataAdapter(command);

            DataTable table = new DataTable();

            adapter.Fill(table);

            return table;
        }


        // ------------------------------------------------------------
        // البحث في فواتير الصيانة
        // ------------------------------------------------------------

        public static DataTable SearchMaintenanceInvoices(string searchText)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            string query = @"
        SELECT
            MaintenanceID,
            InvoiceNumber,
            CustomerName,
            CustomerPhone,
            DeviceName,
            SerialNumber,
            DeviceCondition,
            DeviceColor,
            ProblemDescription,
            Accessories,
            Notes,
            MaintenanceType,
            Price,
            Username,
            ReceiveDate,
            DeliveryDate,
            Status
        FROM MaintenanceInvoices
        WHERE
            InvoiceNumber LIKE @Search
            OR CustomerName LIKE @Search
            OR CustomerPhone LIKE @Search
            OR DeviceName LIKE @Search
            OR SerialNumber LIKE @Search
            OR Status LIKE @Search
        ORDER BY MaintenanceID DESC";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@Search",
                "%" + searchText + "%");

            using SqlDataAdapter adapter = new SqlDataAdapter(command);

            DataTable table = new DataTable();

            adapter.Fill(table);

            return table;
        }


        // ------------------------------------------------------------
        // جلب فاتورة صيانة واحدة
        // ------------------------------------------------------------
        public static DataTable GetMaintenanceInvoice(int maintenanceId)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
        SELECT 
            MaintenanceID,
            InvoiceNumber,
            CustomerName,
            CustomerPhone,
            DeviceName,
            SerialNumber,
            DeviceCondition,
            DeviceColor,
            ProblemDescription,
            Accessories,
            Notes,
            MaintenanceType,

            Price,
            MaintenanceCost,
            SparePartsName,
            SparePartsCost,

            Username,
            ReceiveDate,
            DeliveryDate,
            Status

        FROM MaintenanceInvoices
        WHERE MaintenanceID = @MaintenanceID";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MaintenanceID",
                maintenanceId);

            using SqlDataAdapter adapter =
                new SqlDataAdapter(command);

            DataTable table =
                new DataTable();

            adapter.Fill(table);

            return table;
        }

        // ------------------------------------------------------------
        // تحديث حالة الصيانة
        // ------------------------------------------------------------

        public static void UpdateMaintenanceStatus(
            int maintenanceId,
            string status)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = @"
        UPDATE MaintenanceInvoices
        SET Status = @Status
        WHERE MaintenanceID = @MaintenanceID";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue(
                "@MaintenanceID",
                maintenanceId);

            command.ExecuteNonQuery();
        }


        // ------------------------------------------------------------
        // حذف فاتورة صيانة
        // ------------------------------------------------------------

        public static void DeleteMaintenanceInvoice(int maintenanceId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = @"
        DELETE FROM MaintenanceInvoices
        WHERE MaintenanceID = @MaintenanceID";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MaintenanceID",
                maintenanceId);

            command.ExecuteNonQuery();
        }


        public static void UpdateMaintenanceInvoice(
    int maintenanceId,
    string customerName,
    string customerPhone,
    string deviceName,
    string serialNumber,
    string deviceCondition,
    string deviceColor,
    string problemDescription,
    string accessories,
    string notes,
    string maintenanceType,
    decimal price,
    DateTime? deliveryDate,
    string status)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
        UPDATE MaintenanceInvoices
        SET
            CustomerName = @CustomerName,
            CustomerPhone = @CustomerPhone,
            DeviceName = @DeviceName,
            SerialNumber = @SerialNumber,
            DeviceCondition = @DeviceCondition,
            DeviceColor = @DeviceColor,
            ProblemDescription = @ProblemDescription,
            Accessories = @Accessories,
            Notes = @Notes,
            MaintenanceType = @MaintenanceType,
            Price = @Price,
            DeliveryDate = @DeliveryDate,
            Status = @Status
        WHERE MaintenanceID = @MaintenanceID";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MaintenanceID",
                maintenanceId);

            command.Parameters.AddWithValue(
                "@CustomerName",
                customerName);

            command.Parameters.AddWithValue(
                "@CustomerPhone",
                string.IsNullOrWhiteSpace(customerPhone)
                    ? DBNull.Value
                    : customerPhone);

            command.Parameters.AddWithValue(
                "@DeviceName",
                deviceName);

            command.Parameters.AddWithValue(
                "@SerialNumber",
                string.IsNullOrWhiteSpace(serialNumber)
                    ? DBNull.Value
                    : serialNumber);

            command.Parameters.AddWithValue(
                "@DeviceCondition",
                string.IsNullOrWhiteSpace(deviceCondition)
                    ? DBNull.Value
                    : deviceCondition);

            command.Parameters.AddWithValue(
                "@DeviceColor",
                string.IsNullOrWhiteSpace(deviceColor)
                    ? DBNull.Value
                    : deviceColor);

            command.Parameters.AddWithValue(
                "@ProblemDescription",
                string.IsNullOrWhiteSpace(problemDescription)
                    ? DBNull.Value
                    : problemDescription);

            command.Parameters.AddWithValue(
                "@Accessories",
                string.IsNullOrWhiteSpace(accessories)
                    ? DBNull.Value
                    : accessories);

            command.Parameters.AddWithValue(
                "@Notes",
                string.IsNullOrWhiteSpace(notes)
                    ? DBNull.Value
                    : notes);

            command.Parameters.AddWithValue(
                "@MaintenanceType",
                maintenanceType);

            command.Parameters.AddWithValue(
                "@Price",
                price);

            command.Parameters.AddWithValue(
                "@DeliveryDate",
                deliveryDate.HasValue
                    ? deliveryDate.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "@Status",
                status);

            command.ExecuteNonQuery();
        }

        public static DataTable GetCompletedMaintenanceInvoices()
        {
            DataTable table = new DataTable();

            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
        SELECT
            MaintenanceID,
            InvoiceNumber,
            CustomerName,
            CustomerPhone,
            DeviceName,
            MaintenanceType,
            Price,
            Username,
            ReceiveDate,
            DeliveryDate,
            Status
        FROM MaintenanceInvoices
        WHERE Status = N'تم التسليم'
        ORDER BY MaintenanceID DESC";

            using SqlCommand command =
                new SqlCommand(query, connection);

            using SqlDataAdapter adapter =
                new SqlDataAdapter(command);

            adapter.Fill(table);

            return table;
        }

        // ============================================================
        // REVENUE BOX
        // ============================================================
        public static decimal GetSalesRevenue(
            DateTime? startDate,
            DateTime? endDate)
        {
            // =========================================================
            // إيرادات المبيعات العادية
            // =========================================================

            const string salesQuery = @"
        SELECT ISNULL(SUM(Paid - Change), 0)
        FROM Sales
        WHERE
            (Paid - Change) > 0
            AND
            (
                @IsAll = 1
                OR
                (
                    SaleDate >= @StartDate
                    AND SaleDate < @EndDate
                )
            );";


            // =========================================================
            // تسديد الآجل الخاص بالمبيعات العادية فقط
            //
            // SaleID موجود
            // DeviceSaleID غير موجود
            //
            // بالتالي:
            // آجل مبيعات → يدخل المبيعات
            // آجل جهاز   → لا يدخل المبيعات
            // =========================================================

            const string creditPaymentsQuery = @"
        SELECT ISNULL(SUM(Amount), 0)
        FROM CreditTransactions
        WHERE
            TransactionType = N'Payment'
            AND SaleID IS NOT NULL
            AND DeviceSaleID IS NULL
            AND
            (
                @IsAll = 1
                OR
                (
                    TransactionDate >= @StartDate
                    AND TransactionDate < @EndDate
                )
            );";


            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();


            bool isAll =
                !startDate.HasValue ||
                !endDate.HasValue;


            decimal salesRevenue = 0m;


            // =========================================================
            // المبيعات العادية
            // =========================================================

            using (SqlCommand salesCommand =
                   new SqlCommand(
                       salesQuery,
                       connection))
            {
                salesCommand.Parameters.AddWithValue(
                    "@IsAll",
                    isAll ? 1 : 0);

                salesCommand.Parameters.Add(
                    "@StartDate",
                    SqlDbType.DateTime).Value =
                    (object?)startDate ??
                    DBNull.Value;

                salesCommand.Parameters.Add(
                    "@EndDate",
                    SqlDbType.DateTime).Value =
                    (object?)endDate ??
                    DBNull.Value;


                object result =
                    salesCommand.ExecuteScalar();


                if (result != null &&
                    result != DBNull.Value)
                {
                    salesRevenue =
                        Convert.ToDecimal(result);
                }
            }


            // =========================================================
            // تسديد آجل المبيعات العادية
            // =========================================================

            decimal creditPayments = 0m;


            using (SqlCommand creditCommand =
                   new SqlCommand(
                       creditPaymentsQuery,
                       connection))
            {
                creditCommand.Parameters.AddWithValue(
                    "@IsAll",
                    isAll ? 1 : 0);

                creditCommand.Parameters.Add(
                    "@StartDate",
                    SqlDbType.DateTime).Value =
                    (object?)startDate ??
                    DBNull.Value;

                creditCommand.Parameters.Add(
                    "@EndDate",
                    SqlDbType.DateTime).Value =
                    (object?)endDate ??
                    DBNull.Value;


                object result =
                    creditCommand.ExecuteScalar();


                if (result != null &&
                    result != DBNull.Value)
                {
                    creditPayments =
                        Convert.ToDecimal(result);
                }
            }


            return
                salesRevenue +
                creditPayments;
        }
        public static DataTable GetRevenueTransactions(
    DateTime? startDate,
    DateTime? endDate)
        {
            const string query = @"
        SELECT
            SaleID AS RecordID,

            CAST('Sales' AS VARCHAR(20))
                AS RecordSource,

            SaleDate AS TransactionDate,

            N'مبيعات' AS RevenueType,

            'INV-' +
            RIGHT(
                '000000' +
                CAST(
                    SaleID AS VARCHAR(6)
                ),
                6
            ) AS InvoiceNumber,

            ISNULL(
                CustomerName,
                N'عميل نقدي'
            ) AS CustomerName,

            (Paid - Change) AS Amount

        FROM Sales

        WHERE
            (Paid - Change) > 0
            AND
            (
                @IsAll = 1
                OR
                (
                    SaleDate >= @StartDate
                    AND SaleDate < @EndDate
                )
            )


        UNION ALL


        SELECT

            MaintenanceID AS RecordID,

            CAST('Maintenance' AS VARCHAR(20))
                AS RecordSource,

            DeliveryDate AS TransactionDate,

            N'صيانة' AS RevenueType,

            InvoiceNumber,

            ISNULL(
                CustomerName,
                N'عميل'
            ) AS CustomerName,

            Price AS Amount

        FROM MaintenanceInvoices

        WHERE
            Status = N'تم التسليم'

            AND Price > 0

            AND DeliveryDate IS NOT NULL

            AND
            (
                @IsAll = 1

                OR
                (
                    DeliveryDate >= @StartDate
                    AND DeliveryDate < @EndDate
                )
            )


        UNION ALL


        SELECT

            ct.CreditTransactionID
                AS RecordID,

            CAST(
                'CreditPayment'
                AS VARCHAR(20)
            ) AS RecordSource,

            ct.TransactionDate,

            N'تسديد آجل'
                AS RevenueType,

            CASE
                WHEN ct.SaleID IS NOT NULL
                    THEN
                        'INV-' +
                        RIGHT(
                            '000000' +
                            CAST(
                                ct.SaleID
                                AS VARCHAR(6)
                            ),
                            6
                        )

                ELSE
                    'CREDIT-' +
                    RIGHT(
                        '000000' +
                        CAST(
                            ct.CreditTransactionID
                            AS VARCHAR(6)
                        ),
                        6
                    )
            END AS InvoiceNumber,

            ca.CustomerName,

            ct.Amount

        FROM CreditTransactions ct

        INNER JOIN CreditAccounts ca
            ON ca.CreditAccountID =
               ct.CreditAccountID

        WHERE
            ct.TransactionType =
                N'Payment'

            AND
            (
                @IsAll = 1

                OR
                (
                    ct.TransactionDate >=
                        @StartDate

                    AND
                    ct.TransactionDate <
                        @EndDate
                )
            )

        ORDER BY
            TransactionDate DESC;";


            using SqlConnection connection =
                new SqlConnection(
                    connectionString);


            using SqlCommand command =
                new SqlCommand(
                    query,
                    connection);


            bool isAll =
                !startDate.HasValue ||
                !endDate.HasValue;


            command.Parameters.AddWithValue(
                "@IsAll",
                isAll ? 1 : 0);


            command.Parameters.Add(
                "@StartDate",
                SqlDbType.DateTime).Value =
                (object?)startDate ??
                DBNull.Value;


            command.Parameters.Add(
                "@EndDate",
                SqlDbType.DateTime).Value =
                (object?)endDate ??
                DBNull.Value;


            using SqlDataAdapter adapter =
                new SqlDataAdapter(
                    command);


            DataTable table =
                new DataTable();


            adapter.Fill(table);


            return table;
        }
        public static void CompleteMaintenanceInvoice(int maintenanceId)
        {
            const string query = @"
        UPDATE MaintenanceInvoices
        SET
            Status = N'تم التسليم',
            DeliveryDate = GETDATE()
        WHERE MaintenanceID = @MaintenanceID;";

            using SqlConnection connection =
                new SqlConnection(connectionString);

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@MaintenanceID",
                SqlDbType.Int).Value = maintenanceId;

            connection.Open();

            int affectedRows = command.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new Exception(
                    "لم يتم العثور على فاتورة الصيانة.");
            }
        }


        public static decimal GetExpensesRevenue(
    DateTime? startDate,
    DateTime? endDate)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
        SELECT ISNULL(SUM(Amount), 0)
        FROM Expenses
        WHERE ISNULL(IsDeleted, 0) = 0
          AND (@StartDate IS NULL OR ExpenseDate >= @StartDate)
          AND (@EndDate IS NULL OR ExpenseDate < @EndDate);";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@StartDate",
                SqlDbType.DateTime).Value =
                    startDate.HasValue
                        ? startDate.Value
                        : DBNull.Value;

            command.Parameters.Add(
                "@EndDate",
                SqlDbType.DateTime).Value =
                    endDate.HasValue
                        ? endDate.Value
                        : DBNull.Value;

            object result = command.ExecuteScalar();

            return result == null || result == DBNull.Value
                ? 0m
                : Convert.ToDecimal(result);
        }

        public static void UpdateMaintenanceCostDetails(
    int maintenanceId,
    decimal maintenanceCost,
    string sparePartsName,
    decimal sparePartsCost)
        {
            const string query = @"
        UPDATE MaintenanceInvoices
        SET
            MaintenanceCost = @MaintenanceCost,
            SparePartsName = @SparePartsName,
            SparePartsCost = @SparePartsCost
        WHERE MaintenanceID = @MaintenanceID;";

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@MaintenanceID",
                        maintenanceId);

                    command.Parameters.AddWithValue(
                        "@MaintenanceCost",
                        maintenanceCost);

                    command.Parameters.AddWithValue(
                        "@SparePartsName",
                        string.IsNullOrWhiteSpace(sparePartsName)
                            ? (object)DBNull.Value
                            : sparePartsName);

                    command.Parameters.AddWithValue(
                        "@SparePartsCost",
                        sparePartsCost);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }
        public static decimal GetMaintenanceRevenue(
    DateTime? startDate,
    DateTime? endDate)
        {
            const string query = @"
        SELECT ISNULL(SUM(Price), 0)
        FROM MaintenanceInvoices
        WHERE
            Status = N'تم التسليم'
            AND Price > 0
            AND DeliveryDate IS NOT NULL
            AND
            (
                @IsAll = 1
                OR
                (
                    DeliveryDate >= @StartDate
                    AND DeliveryDate < @EndDate
                )
            );";

            using SqlConnection connection =
                new SqlConnection(connectionString);

            using SqlCommand command =
                new SqlCommand(
                    query,
                    connection);

            bool isAll =
                !startDate.HasValue ||
                !endDate.HasValue;

            command.Parameters.AddWithValue(
                "@IsAll",
                isAll ? 1 : 0);

            command.Parameters.Add(
                "@StartDate",
                SqlDbType.DateTime).Value =
                (object?)startDate ??
                DBNull.Value;

            command.Parameters.Add(
                "@EndDate",
                SqlDbType.DateTime).Value =
                (object?)endDate ??
                DBNull.Value;

            connection.Open();

            object result =
                command.ExecuteScalar();

            if (result == null ||
                result == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(result);
        }
    }
}
