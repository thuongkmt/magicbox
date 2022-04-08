using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Utilities
{
    public class ProcessUtils
    {
        public static bool IsProcessInProgress(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }
        public static Process StartProcess(string processPath)
        {
            Process newProcess = new Process();
            newProcess.StartInfo = new ProcessStartInfo(processPath);
            newProcess.Start();
            return newProcess;
        }

        public static void KillProcesses(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
                process.Close();
            }
        }

    }
}
