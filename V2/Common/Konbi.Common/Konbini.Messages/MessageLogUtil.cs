using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages
{
    public class MessageLogUtil
    {
        private static ILogger detailLog = null;
        private static Serilog.ILogger DetailLogger
        {
            get
            {
                if (detailLog == null)
                {
                    detailLog =
                       new LoggerConfiguration()
                           .WriteTo.LiterateConsole()
                           .WriteTo.RollingFile("Messages\\log-messages-{Date}.txt", shared: true)
                           .CreateLogger();
                }
                return detailLog;
            }
        }

        static MessageLogUtil()
        {

        }
        public static void Error(string message, Exception e)
        {
            DetailLogger.Error(e, message);
        }

        public static void Log(string message)
        {
            DetailLogger.Information(message);
        }
    }
}
