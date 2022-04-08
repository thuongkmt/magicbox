using Serilog;
using System;

namespace Konbini.RfidFridge.TagManagement.Service
{
    public static class SeriLogService
    {

        private static ILogger InfoLogger;
        private static ILogger ErrorLogger;


        public static void CreateLoggers()
        {
            var desktopPath = System.AppDomain.CurrentDomain.BaseDirectory + @"Logs\";
            InfoLogger = Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.LiterateConsole()
              .WriteTo.RollingFile(desktopPath + "Info-{Date}.txt")
              .CreateLogger();

            ErrorLogger = Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.LiterateConsole()
              .WriteTo.RollingFile(desktopPath + "Error-{Date}.txt")
              .CreateLogger();
        }

        public static void LogInfo(string msg)
        {
            InfoLogger?.Information(msg);
        }

        public static void LogError(string msg)
        {
            ErrorLogger?.Error(msg);
        }

    }
}
