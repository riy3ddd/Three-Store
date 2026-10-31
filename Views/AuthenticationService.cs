using Microsoft.Data.SqlClient;

namespace WpfApp1
{
    public static class AuthenticationService
    {
        private static readonly string ConnectionString =
            "Server=localhost;Database=StoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static User? Login(
            string username,
            string password)
        {
            using SqlConnection connection =
                new SqlConnection(ConnectionString);

            connection.Open();

            const string query = @"
                SELECT
                    UserID,
                    Username,
                    PasswordHash,
                    Role,
                    IsActive
                FROM Users
                WHERE Username = @Username";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@Username",
                System.Data.SqlDbType.NVarChar,
                50).Value = username;

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            bool isActive =
                reader.GetBoolean(
                    reader.GetOrdinal("IsActive"));

            if (!isActive)
                return null;

            string storedHash =
                reader["PasswordHash"]?.ToString() ?? "";

            if (!PasswordHasher.VerifyPassword(
                    password,
                    storedHash))
            {
                return null;
            }

            return new User
            {
                UserID =
                    reader.GetInt32(
                        reader.GetOrdinal("UserID")),

                Username =
                    reader["Username"]?.ToString() ?? "",

                Role =
                    reader["Role"]?.ToString() ?? "",

                IsActive = isActive
            };
        }
    }
}