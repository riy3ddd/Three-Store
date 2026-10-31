using System;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // يبدأ التركيز على اسم المستخدم
            Loaded += LoginWindow_Loaded;
        }


        // =========================================
        // عند فتح نافذة تسجيل الدخول
        // =========================================

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Focus();
        }


        // =========================================
        // Enter داخل اسم المستخدم
        // =========================================
        // Enter هنا لا يسجل الدخول
        // وإنما ينقل المستخدم إلى كلمة المرور

        private void UsernameTextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();

                // يمنع أي معالجة أخرى للـ Enter
                e.Handled = true;
            }
        }


        // =========================================
        // Enter داخل كلمة المرور
        // =========================================
        // Enter هنا يقوم بنفس وظيفة الضغط
        // على زر تسجيل الدخول

        private void PasswordBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                // تشغيل نفس كود زر تسجيل الدخول
                LoginButton_Click(
                    LoginButton,
                    new RoutedEventArgs());
            }
        }


        // =========================================
        // زر تسجيل الدخول
        // =========================================

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string username =
                UsernameTextBox.Text.Trim();

            string password =
                PasswordBox.Password;

            // مسح رسالة الخطأ القديمة
            ErrorTextBlock.Text = "";


            // =========================================
            // التحقق من اسم المستخدم
            // =========================================

            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorTextBlock.Text =
                    "من فضلك أدخل اسم المستخدم.";

                UsernameTextBox.Focus();

                return;
            }


            // =========================================
            // التحقق من كلمة المرور
            // =========================================

            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text =
                    "من فضلك أدخل كلمة المرور.";

                PasswordBox.Focus();

                return;
            }


            // =========================================
            // محاولة تسجيل الدخول
            // =========================================

            try
            {
                User? user =
                    AuthenticationService.Login(
                        username,
                        password);


                // =====================================
                // بيانات الدخول غير صحيحة
                // =====================================

                if (user == null)
                {
                    ErrorTextBlock.Text =
                        "اسم المستخدم أو كلمة المرور غير صحيحة.";

                    PasswordBox.Clear();

                    PasswordBox.Focus();

                    return;
                }


                // =====================================
                // حفظ المستخدم الحالي في Session
                // =====================================

                SessionManager.Login(user);


                // =====================================
                // فتح MainWindow
                // =====================================

                MainWindow mainWindow =
                    new MainWindow();

                mainWindow.Show();


                // إغلاق نافذة تسجيل الدخول
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تسجيل الدخول:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}