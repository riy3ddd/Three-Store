using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public static class WindowManager
    {
        // =========================================================
        // تخزين النوافذ المفتوحة
        // =========================================================

        private static readonly Dictionary<Type, Window> _openWindows =
            new Dictionary<Type, Window>();


        // =========================================================
        // فتح نافذة مرة واحدة فقط
        // =========================================================

        public static T ShowSingle<T>(
            Window owner = null,
            Action<T> configure = null)
            where T : Window, new()
        {
            Type windowType = typeof(T);

            // =====================================================
            // هل النافذة مفتوحة بالفعل؟
            // =====================================================

            if (_openWindows.TryGetValue(
                windowType,
                out Window existingWindow))
            {
                // لو النافذة موجودة ولكن تم إغلاقها
                if (existingWindow == null ||
                    !existingWindow.IsVisible)
                {
                    _openWindows.Remove(windowType);
                }
                else
                {
                    // لو Minimized نرجعها
                    if (existingWindow.WindowState ==
                        WindowState.Minimized)
                    {
                        existingWindow.WindowState =
                            WindowState.Normal;
                    }

                    // نخليها قدام باقي النوافذ
                    existingWindow.Show();
                    existingWindow.Activate();

                    return (T)existingWindow;
                }
            }


            // =====================================================
            // إنشاء نافذة جديدة
            // =====================================================

            T newWindow = new T();

            if (owner != null)
            {
                newWindow.Owner = owner;
            }

            // =====================================================
            // إعدادات إضافية للنافذة
            // =====================================================

            configure?.Invoke(newWindow);


            // =====================================================
            // تسجيل النافذة
            // =====================================================

            _openWindows[windowType] = newWindow;


            // =====================================================
            // عند إغلاق النافذة نحذفها من الـ Manager
            // =====================================================

            newWindow.Closed += (sender, e) =>
            {
                if (_openWindows.TryGetValue(
                    windowType,
                    out Window currentWindow))
                {
                    if (ReferenceEquals(
                        currentWindow,
                        newWindow))
                    {
                        _openWindows.Remove(windowType);
                    }
                }
            };


            // =====================================================
            // فتح النافذة
            // =====================================================

            newWindow.Show();

            newWindow.Activate();

            return newWindow;
        }


        // =========================================================
        // معرفة هل نافذة معينة مفتوحة
        // =========================================================

        public static bool IsOpen<T>()
            where T : Window
        {
            Type windowType = typeof(T);

            if (!_openWindows.TryGetValue(
                windowType,
                out Window window))
            {
                return false;
            }

            if (window == null ||
                !window.IsVisible)
            {
                _openWindows.Remove(windowType);
                return false;
            }

            return true;
        }


        // =========================================================
        // إغلاق نافذة معينة من الـ Manager
        // =========================================================

        public static void Close<T>()
            where T : Window
        {
            Type windowType = typeof(T);

            if (_openWindows.TryGetValue(
                windowType,
                out Window window))
            {
                if (window != null)
                {
                    window.Close();
                }

                _openWindows.Remove(windowType);
            }
        }


        // =========================================================
        // إغلاق كل النوافذ المفتوحة
        // =========================================================

        public static void CloseAll()
        {
            Window[] windows =
                new Window[_openWindows.Count];

            _openWindows.Values.CopyTo(
                windows,
                0);

            _openWindows.Clear();

            foreach (Window window in windows)
            {
                try
                {
                    if (window != null)
                    {
                        window.Close();
                    }
                }
                catch
                {
                    // تجاهل أي خطأ أثناء إغلاق النوافذ
                }
            }
        }
    }
}