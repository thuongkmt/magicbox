using System.Collections.Generic;
using KonbiBrain.Common.Messages;

namespace Konbini.Messages.Commands.RFIDTable
{
    public class DetectPlatesCommand:UniversalCommands<DetectPlatesCommandPayload>
    {
        private const string command = UniversalCommandConstants.RfidTableDetectPlates;
        public DetectPlatesCommand():base(command)
        {
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
