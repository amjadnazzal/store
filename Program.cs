using System;
using System.Windows.Forms;
using ClothingStoreManager.Forms;

namespace ClothingStoreManager
{
    internal static class Program
    {
        /// <summary>
        /// نقطة الدخول الرئيسية للتطبيق.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // تهيئة قاعدة البيانات
            DatabaseHelper.InitializeDatabase();
            
            // عرض نافذة تسجيل الدخول أولاً
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // تشغيل النافذة الرئيسية بعد تسجيل الدخول بنجاح
                Application.Run(new MainForm());
            }
        }
    }
}