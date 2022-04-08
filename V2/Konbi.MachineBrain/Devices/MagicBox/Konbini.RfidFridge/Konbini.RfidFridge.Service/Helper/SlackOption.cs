using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Helper
{
    public class SlackOption
    {
        public string HookUrl { get; set; }
        public string UserName { get; set; }
        public string ChannelName { get; set; }
        public string ServerName { get; set; }
    }
}
