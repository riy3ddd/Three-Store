using System;

namespace WpfApp1
{
    public static class CreateAdmin
    {
        public static void Generate()
        {
            string password = "123456";

            string hash =
                PasswordHasher.HashPassword(password);

            Console.WriteLine(hash);
        }
    }
}