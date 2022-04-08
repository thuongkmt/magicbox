using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Konbi.Common.Interfaces;
using Newtonsoft.Json;

namespace KonbiBrain.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Log the error to local file, the file will be collected and send to konbini log server later
        /// </summary>
        /// <param name="ex"></param>
        public static void LogError(this Exception ex) {
            var fileName = $"error_log/{DateTime.Now.ToFileTimeUtc()}.log";
            var errorText = JsonConvert.SerializeObject(ex);
            File.AppendAllText(fileName,errorText);
        }

        public static void LogTaskExceptions(this Task task)
        {
            var logService = IoC.Get<IKonbiBrainLogService>();
            if(logService==null) throw new Exception("IoC could not resolve IKonbiBrainLogService");
            task.ContinueWith(t =>
                {
                    var aggException = t.Exception.Flatten();
                    foreach (var exception in aggException.InnerExceptions)
                        logService.LogException(exception);
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void RemoveTaskExceptions(this Task task)
        {

            task.ContinueWith(t =>
                {

                },
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
