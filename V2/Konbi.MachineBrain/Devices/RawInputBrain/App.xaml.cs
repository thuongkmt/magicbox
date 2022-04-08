using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Konbi.Common.Interfaces;

namespace RawInputBrain
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            //Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                //// If there is more than one, than it is already running.
                //MessageBox.Show("Application is already running.", "KonbiBrain MDB Single Instance Check", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            RegisterGlobalExceptionHandling();
            

            base.OnStartup(e);
        }

        private void RegisterGlobalExceptionHandling()
        {
            // this is the line you really want 
            System.AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => CurrentDomainOnUnhandledException(args);

            // optional: hooking up some more handlers
            // remember that you need to hook up additional handlers when 
            // logging from other dispatchers, shedulers, or applications

            this.Dispatcher.UnhandledException +=
                (sender, args) => DispatcherOnUnhandledException(args);

            Application.Current.DispatcherUnhandledException +=
                (sender, args) => CurrentOnDispatcherUnhandledException(args);

            TaskScheduler.UnobservedTaskException +=
                (sender, args) => TaskSchedulerOnUnobservedTaskException(args);
        }

        private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args)
        {

            LogService.LogException(args.Exception);
            args.SetObserved();
        }

        private static IKonbiBrainLogService LogService
        {
            get
            {
                var log = IoC.Get<IKonbiBrainLogService>();
                return log;
            }
        }

        private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args)
        {
            LogService.LogException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
        {
            LogService.LogException(args.Exception);
        }

        private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args)
        {
            var exception = args.ExceptionObject as Exception;
        
            LogService.LogException(exception);

        }
    }
}
