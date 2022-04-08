using KonbiBrain.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Commands.RFIDTable
{
    public class ConfigCommand: UniversalCommands<TableSettingPayload>
    {
        public ConfigCommand() : base(UniversalCommandConstants.RfidTableConfiguration)
        {
            CommandObject = new TableSettingPayload();
        }
    }
    public class TableSettingPayload
    {
        public string Action { get; set; }
        public List<string> ComPortAvaliable { get; set; }
        public string selectedComPort { get; set; }
        public bool IsServiceRunning { get; set; }
    }
}
