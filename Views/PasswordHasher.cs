using System;
using System.Security.Cryptography;

namespace WpfApp1
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split(':');

                if (parts.Length != 2)
                    return false;

                byte[] salt =
                    Convert.FromBase64String(parts[0]);

                byte[] storedPasswordHash =
                    Convert.FromBase64String(parts[1]);

                byte[] passwordHash =
                    Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        100000,
                        HashAlgorithmName.SHA256,
                        32);

                return CryptographicOperations.FixedTimeEquals(
                    passwordHash,
                    storedPasswordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}