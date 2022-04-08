using Konbini.RfidFridge.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class CmdExecuteService
    {
        public LogService LogService;
        public SlackService SlackService;

        private string DevConLocation;

        public CmdExecuteService(LogService _logService,
            SlackService _slackService
            )
        {
            LogService = _logService;
            SlackService = _slackService;
        }

        public void Init()
        {
            DevConLocation = RfidFridgeSetting.System.DevCon.Location;
            //ListComport();

            //ResetIuc(false);
        }

        public void ListComport()
        {
            var cmd = $"{DevConLocation} hwids =ports";
            ExecuteCommand(cmd);
        }
        public void ResetIuc(bool sendSlack)
        {
            var iucReset = RfidFridgeSetting.System.DevCon.Command.IucReset;
            var cmd = $"{DevConLocation} {iucReset}";

            var message = ExecuteCommand(cmd);
            if (sendSlack)
            {
                SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "ResetIuc result: " + message);
            }
        }

        public void ResetCardHolder(bool sendSlack)
        {
            var iucReset = RfidFridgeSetting.System.DevCon.Command.ResetCardHolder;
            var cmd = $"{DevConLocation} {iucReset}";

            var message = ExecuteCommand(cmd);
            if (sendSlack)
            {
                SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "Reset Card holder result: " + message);
            }
        }


        private string ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            LogService.LogCmdExec("Executing command: " + command);
            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Verb = "runas"
            };

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            LogService.LogCmdExec("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            LogService.LogCmdExec("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            LogService.LogCmdExec("ExitCode: " + exitCode.ToString());
            process.Close();
            return $"[OUTPUT] {output} | [ERROR] {error}";
        }
    }
}
