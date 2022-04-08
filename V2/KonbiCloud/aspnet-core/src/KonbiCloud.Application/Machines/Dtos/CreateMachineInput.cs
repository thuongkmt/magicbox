using System;
using System.Collections.Generic;
using System.Text;
using KonbiCloud.Common;

namespace KonbiCloud.Machines.Dtos
{
    

    public class CreateMachineInput
    {       
        public string Id { get; set; }
        public string Name { get; set; }
        public string CashlessTerminalId { get; set; }
    }

    public class SendRemoteCommandInput
    {
        public string MachineID { get; set; }
        public string CommandName { get; set; }
        public string CommandArgs { get; set; }

        public RedisRemoteCommands Command
        {
            get
            {
                foreach (RedisRemoteCommands cmd in Enum.GetValues(typeof(RedisRemoteCommands)))
                {
                    if (cmd.ToString().ToLower() == CommandName.ToLower()) return cmd;
                }
                return RedisRemoteCommands.UNDEFINED;
            }
        }
    }

    public class SendRemoteCommandOutput
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
