using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.RFIDTable
{
    public class DetectPlatesCommand:UniversalCommands<DetectPlatesCommandPayload>
    {
        private const string command = UniversalCommandConstants.RfidTableDetectPlates;
        public DetectPlatesCommand():base()
        {
            Command = command;
            CommandObject = new DetectPlatesCommandPayload() { Plates = new List<PlateInfo>() };
            
        }
        
      
    }
    public class DetectPlatesCommandPayload
    {
        public IEnumerable<PlateInfo> Plates { get; set; }
    }
    public class PlateInfo
    {
        public string UID { get; set; }
        public string UType { get; set; }
    }
}
