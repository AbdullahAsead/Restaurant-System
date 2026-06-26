using System;
using System.IO;

namespace Restaurant_System.Helpers
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            string databaseFileName = "Restaurant.db";
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string databasePath = Path.Combine(exeDirectory, databaseFileName);
            return $"Data Source={databasePath}";
        }
    }
}
