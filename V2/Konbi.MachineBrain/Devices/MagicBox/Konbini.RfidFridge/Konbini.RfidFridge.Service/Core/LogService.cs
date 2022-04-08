
using Serilog;
using System;

namespace Konbini.RfidFridge.Service.Core
{
    public class LogService
    {
        private ILogger InfoLogger;
        private ILogger TemperatureLogger;
        private ILogger TerminalLogger;
        private ILogger TerminalDebugLogger;
        private ILogger MachineAdminApiLogger;
        private ILogger DebugLogger;
        private ILogger LockLogger;
        private ILogger CardHolderLogger;
        private ILogger CardHolderHwLogger;
        private ILogger MagicPaymentLogger;
        private ILogger FridgeReaderLogger;
        private ILogger MachineStatusLogger;
        private ILogger CameraLogger;
        private ILogger CustomerCloudLogger;


        private ILogger GrabPayLogger;
        private ILogger CmdExcuteLogger;
        private ILogger WalletLogger;
        private ILogger QrLogger;
        public void Init()
        {
            TerminalDebugLogger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               //.WriteTo.Console()
               .WriteTo.File("logs\\terminal-debug.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            TerminalLogger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               //.WriteTo.Console()
               .WriteTo.File("logs\\terminal-.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            InfoLogger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.File("logs\\log-.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            TemperatureLogger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               //.WriteTo.Console()
               .WriteTo.File("logs\\temp-.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            DebugLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Console()
              .WriteTo.File("logs\\debug-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();

            MachineAdminApiLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              //.WriteTo.Console()
              .WriteTo.File("logs\\machineadminapi-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();

            LockLogger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             //.WriteTo.Console()
             .WriteTo.File("logs\\lock-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            CardHolderLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            //.WriteTo.Console()
            .WriteTo.File("logs\\cardholder-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            CardHolderHwLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            //.WriteTo.Console()
            .WriteTo.File("logs\\cardholder-hw-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            MagicPaymentLogger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.Console()
           .WriteTo.File("logs\\magic-payment.txt", rollingInterval: RollingInterval.Day)
           .CreateLogger();

            FridgeReaderLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              //.WriteTo.Console()
              .WriteTo.File("logs\\rfid-reader-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();

            MachineStatusLogger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             //.WriteTo.Console()
             .WriteTo.File("logs\\machinestatus-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            CameraLogger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             //.WriteTo.Console()
             .WriteTo.File("logs\\camera-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            CustomerCloudLogger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           //.WriteTo.Console()
           .WriteTo.File("logs\\customercloud-.txt", rollingInterval: RollingInterval.Day)
           .CreateLogger();

            GrabPayLogger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             //.WriteTo.Console()
             .WriteTo.File("logs\\grab-pay-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            CmdExcuteLogger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Console()
          .WriteTo.File("logs\\cmd-execute-.txt", rollingInterval: RollingInterval.Day)
          .CreateLogger();

            WalletLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Console()
              .WriteTo.File("logs\\wallet-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();


            QrLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Console()
              .WriteTo.File("logs\\qr-reader-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();
        }

        public void LogQrReader(string message)
        {
            QrLogger?.Information(message);
        }

        public void LogInfo(string message)
        {
            InfoLogger?.Information(message);
        }

        public void LogMachineAdminApi(string message)
        {
            MachineAdminApiLogger?.Information(message);
        }

        public void LogTerminalDebug(string message)
        {
            TerminalDebugLogger?.Information(message);
        }
        public void LogTerminalInfo(string message)
        {
            TerminalLogger?.Information(message);
        }

        public void LogError(string message)
        {
            InfoLogger?.Error(message);
        }

        public void LogTemp(string message)
        {
            TemperatureLogger?.Information(message);
        }

        public void Debug(string message)
        {
            DebugLogger?.Information(message);
        }

        public void LogError(Exception ex)
        {
            InfoLogger?.Error(ex.ToString());
        }

        public void LogLockInfo(string info)
        {
            LockLogger?.Information(info);
        }

        public void LogCardHolderInfo(string info)
        {
            CardHolderLogger?.Information(info);
        }
        public void LogCardHolderHwInfo(string info)
        {
            CardHolderHwLogger?.Information(info);
        }

        public void LogMagicPaymentInfo(string info)
        {
            MagicPaymentLogger?.Information(info);
        }

        public void LogReaderInfo(string info)
        {
            FridgeReaderLogger?.Information(info);
        }

        public void LogMachineStatus(string info)
        {
            MachineStatusLogger?.Information(info);
        }

        public void LogCamera(string info)
        {
            CameraLogger?.Information(info);
        }

        public void LogCustomerCloudApi(string info)
        {
            CustomerCloudLogger?.Information(info);
        }

        public void LogGrabPay(string info)
        {
            GrabPayLogger?.Information(info);
        }

        public void LogCmdExec(string info)
        {
            CmdExcuteLogger?.Information(info);
        }

        public void LogWallet(string info)
        {
            WalletLogger?.Information(info);
        }
    }
}
