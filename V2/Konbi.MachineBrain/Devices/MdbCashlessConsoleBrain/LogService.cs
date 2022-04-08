using System;
using System.Diagnostics;
using Konbi.Common.Interfaces;
using Serilog;

//using Serilog.Sinks.Slack;

namespace KonbiBrain.Common.Services
{
    public class LogService : IKonbiBrainLogService
    {
      

        public LogService()
        {
            try
            {
             
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs\";
                Logger =
                    new LoggerConfiguration()
                        //.Enrich.WithExceptionDetails()
                        .WriteTo.RollingFile(desktopPath + "log-{Date}.txt", shared: true)
                        .CreateLogger();

                NsqCommandLog =
                    new LoggerConfiguration()
                        .WriteTo.RollingFile(desktopPath + "command-{Date}.txt", shared: true)
                        .CreateLogger();

                HopperPayoutLogger = new LoggerConfiguration()
                        .WriteTo.RollingFile(desktopPath + "log-hopperpayout-{Date}.txt", shared: true)
                        .CreateLogger();

                MdbLogger = new LoggerConfiguration()
                        .WriteTo.RollingFile(desktopPath + "log-mdb-{Date}.txt", shared: true)
                        .CreateLogger();

                VmcLogger = new LoggerConfiguration()
                        .WriteTo.RollingFile(desktopPath + "log-vmc-{Date}.txt", shared: true)
                        .CreateLogger();

                HotChillerLogger = new LoggerConfiguration()
                        .WriteTo.RollingFile(desktopPath + "log-hotchiller-{Date}.txt", shared: true)
                        .CreateLogger();

                DebugLogger = new LoggerConfiguration()
                       .WriteTo.RollingFile(desktopPath + "log-information-{Date}.txt", shared: true)
                       .CreateLogger();

                CloudSyncLogger = new LoggerConfiguration()
                    .WriteTo.RollingFile(desktopPath + "log-cloudsync-{Date}.txt", shared: true)
                    .CreateLogger();


                EventhubLogger = new LoggerConfiguration()
                    .WriteTo.RollingFile(desktopPath + "log-eventhub-{Date}.txt", shared: true)
                    .CreateLogger();

                HopperPayoutEventLogger = new LoggerConfiguration()
                       .WriteTo.RollingFile(desktopPath + "log-hopperpayout-events-{Date}.txt", shared: true)
                       .CreateLogger();
                ViewModelLogger = new LoggerConfiguration()
                .WriteTo.RollingFile(desktopPath + "log-viewmodel-events-{Date}.txt", shared: true)
                .CreateLogger();

                IucLogger = new LoggerConfiguration()
                    .WriteTo.RollingFile(desktopPath + "log-iuc-api-{Date}.txt", shared: true)
                    .CreateLogger();

                WhiteVmcLogger = new LoggerConfiguration()
                    .WriteTo.RollingFile(desktopPath + "log-whitevmc-{Date}.txt", shared: true)
                    .CreateLogger();

                CykloneLogger = new LoggerConfiguration()
                   .WriteTo.RollingFile(desktopPath + "log-cyklone-{Date}.txt", shared: true)
                   .CreateLogger();

                ScbLogger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    //.WriteTo.LiterateConsole()
                    .WriteTo.RollingFile(desktopPath + "log-scb-{Date}.txt", shared: true)
                    .CreateLogger();

                EmailLogger = new LoggerConfiguration().MinimumLevel.Debug()
                    //.WriteTo.LiterateConsole()
                    .WriteTo.RollingFile(desktopPath + "log-email-{Date}.txt", shared: true).CreateLogger();

                BbposLoger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile(desktopPath + "log-bbpos-{Date}.txt", shared: true)
                    .CreateLogger();

                HidRfidLogger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile(desktopPath + "log-hidrfid-{Date}.txt", shared: true)
                    .CreateLogger();

                RawInputLogger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile(desktopPath + "log-rawinput-{Date}.txt", shared: true)
                    .CreateLogger();

             


                PerformanceLogger = new LoggerConfiguration()
                    .WriteTo.RollingFile(desktopPath + "log-performance-{Date}.txt", shared: true)
                    .CreateLogger();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }

        //private ILogger SlackDbRequestsLogger { get; }

        private ILogger HopperPayoutEventLogger { get; }
        private ILogger HopperPayoutLogger { get; }
        private ILogger MdbLogger { get; }
        private ILogger VmcLogger { get; }
        private ILogger HotChillerLogger { get; }
        private ILogger DebugLogger { get; }
        private ILogger CloudSyncLogger { get; }
        private ILogger CloudLogger { get; }
        private ILogger ViewModelLogger { get; }

        private ILogger EventhubLogger { get; }
        private ILogger Logger { set; get; }
        private ILogger IucLogger { get; set; }
        private ILogger WhiteVmcLogger { get; set; }
        private ILogger PerformanceLogger { get; set; }

        private ILogger CykloneLogger { get; set; }

        private ILogger ScbLogger { get; set; }
        private ILogger EmailLogger { get; set; }
        private ILogger BbposLoger { get; set; }
        private ILogger HidRfidLogger { get; set; }
        private ILogger RawInputLogger { get; set; }


      
        private ILogger NsqCommandLog { get; }

        public void LogHopperPayoutEvents(string message)
        {
            HopperPayoutEventLogger.Information(message);
        }
        public virtual void LogHopperPayoutError(Exception ex)
        {
            HopperPayoutLogger.Fatal(ex.Message);
            HopperPayoutLogger.Fatal(ex.StackTrace);
            HopperPayoutLogger.Fatal(ex.ToString());
            //SlackGeneralErrorLogger.Fatal(ex.ToString());
        }

        public void LogWhiteVmc(string msg)
        {
            WhiteVmcLogger.Information(msg);
        }

        public virtual void LogException(Exception ex)
        {
            Logger.Fatal(ex.Message);
            Logger.Fatal(ex.StackTrace);
            Logger.Fatal(ex.ToString());
            //SlackGeneralErrorLogger.Fatal(ex.ToString());
        }

        public virtual void LogMdbError(Exception ex)
        {
            MdbLogger.Fatal(ex.Message);
            MdbLogger.Fatal(ex.StackTrace);
            MdbLogger.Fatal(ex.ToString());
            //SlackGeneralErrorLogger.Fatal(ex.ToString());
        }

        public virtual void LogPerformanceDebug(string message)
        {
            //PerformanceLogger.Information(message);
        }


        public virtual void LogTemperatureDeviceError(Exception ex)
        {
            HotChillerLogger.Fatal(ex.Message);
            HotChillerLogger.Fatal(ex.StackTrace);
            HotChillerLogger.Fatal(ex.ToString());
            //SlackGeneralErrorLogger.Fatal(ex.ToString());
        }

        public virtual void LogVMCError(Exception ex)
        {
            VmcLogger.Fatal(ex.Message);
            VmcLogger.Fatal(ex.StackTrace);
            VmcLogger.Fatal(ex.ToString());
            //            SlackGeneralErrorLogger.Fatal(ex.ToString());
            //SendGeneralMessageToSlack(ex.ToString(), Emoji.Bug, "VMC Exception Error");
        }

        public virtual void LogCloudSyncInfo(string message)
        {
            //            SlackGeneralErrorLogger.Information("[Cloud Sync] " + message);
            //            SendGeneralMessageToSlack(message, Emoji.Cloud, "Cloud Sync");
            CloudSyncLogger.Information("[Cloud Sync] " + message);
        }

     
        public void LogIucApi(string message)
        {
            IucLogger.Information(message);
        }

        public void NotifyHopperPayoutError(string message)
        {
            
        }

        public void LogCyklone(string message)
        {
            CykloneLogger.Information(message);
        }

        public virtual void LogHopperPayoutError(string message)
        {
            HopperPayoutLogger.Fatal(message);
        }

        public virtual void LogMdbError(string message)
        {
            MdbLogger.Fatal(message);
        }

        public virtual void LogTemperatureDeviceError(string message)
        {
            HotChillerLogger.Fatal(message);
        }

        public virtual void LogVMCError(string message)
        {
            VmcLogger.Fatal(message);
        }
        public virtual void LogVMCInfo(string message)
        {
            VmcLogger.Information(message);
        }

        public virtual void LogException(string message)
        {
            Logger.Fatal(message);
            //            SlackGeneralErrorLogger.Fatal(message);
            //SendGeneralMessageToSlack(message, Emoji.Bug, "General Exception");
        }

        //public virtual void LogRequestInfo(string message)
        //{
        //    SlackDbRequestsLogger.Information(message);
        //}

        public virtual void LogInfo(string message)
        {
            DebugLogger.Information(message);
        }

        public virtual void LogLockerError(Exception ex)
        {
            HotChillerLogger.Fatal(ex.Message);
            HotChillerLogger.Fatal(ex.StackTrace);
            HotChillerLogger.Fatal(ex.ToString());
        }

        public virtual Guid LogCloudError(Exception ex)
        {
            var guid = Guid.NewGuid();
            //CloudLogger.Fatal($"ID:{guid} Date:{DateTime.Now} Detail:{ex.ToString()}");
            return guid;
        }

        public virtual void LogCloudInfo(string message)
        {
            CloudLogger.Information(message);

        }

        public void LogViewModelNavigation(string message)
        {
            ViewModelLogger.Information(message);
        }

       
        public void LogEventHub(string message)
        {
            EventhubLogger.Information(message);
        }



        public void LogScbInfo(string info)
        {
            ScbLogger.Information(info);
        }

        public void LogBbposInfo(string info)
        {
            BbposLoger.Information(info);
        }

        public void LogEmailInfo(string info)
        {
            EmailLogger.Information(info);
        }


        public void LogHidRfidInfo(string info)
        {
            HidRfidLogger.Information(info);
        }

        public void LogRawInputInfo(string info)
        {
            RawInputLogger.Information(info);
        }
    }
}
