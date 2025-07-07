using System;
using System.Windows.Forms;

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
            
            Application.Run(new MainForm());
        }
    }
}