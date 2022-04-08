using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core.Devices.SlimCardInsert
{
    public partial class SlimCardReaderInterface
    {
        public class StatusReponse : ResponseCommand
        {
            public byte S { get; set; }
            public StatusReponse(byte[] cmd)
            {
               
                CM = cmd[3];
                PM = cmd[4];
                S = cmd[5];
                var length = new byte[] { cmd[1], cmd[2] };
                LENGTH = length.BcdToInt();
                DATA = cmd.ToList().Skip(5).Take(LENGTH).ToArray();
            }

            public bool IsCardInserted()
            {
                var a = (StatusCode.CARD_STATUS_S)S == StatusCode.CARD_STATUS_S.CARD_INSIDE;
                return a;
            }

            public bool IsLatchLock()
            {
                return true;
            }

            public override string ToString()
            {
                return $"S: {(StatusCode.CARD_STATUS_S)((byte)S)}";
            }
        }

        public class EjectResponse : ResponseCommand
        {
            public byte S1 { get; set; }
            public EjectResponse(byte[] cmd)
            {

                CM = cmd[3];
                PM = cmd[4];
                S1 = cmd[5];

                var length = new byte[] { cmd[1], cmd[2] };
                LENGTH = length.BcdToInt();
                DATA = cmd.ToList().Skip(5).Take(LENGTH).ToArray();
            }

            public bool IsEjectSuccess()
            {
                var a = (StatusCode.EJECT_STATUS_S1)S1 == StatusCode.EJECT_STATUS_S1.EJECT_SUCCESS;
                return a;
            }

            public override string ToString()
            {
                return $"S1: {(StatusCode.EJECT_STATUS_S1)((byte)S1)}";
            }
        }
        public class ResponseCommand
        {
            public byte CM;
            public byte PM;
            public int LENGTH;
            public byte[] DATA;
        }

        public class StatusCode
        {
            public enum CARD_STATUS_S
            {
                NO_CARD_INSIDE = 0x4E,
                CARD_INSIDE = 0x59,
            }

            public enum EJECT_STATUS_S1
            {
                EJECT_SUCCESS = 0x59,
                EJECT_FAILED = 0x4E,
            }
        }

        private void QueueResponse(byte[] response)
        {
            // Never expect to get large queue
          
            if (this.ResponseCommandsQueue.Count > 100)
                this.CleanCommandQueue();
            this.ResponseCommandsQueue.Enqueue(response);

            if (DebugMode)
            {
                LogInfo?.Invoke("Queued Response: " + response.ToHexString());
                var s = ResponseCommandsQueue.Select(x => x.ToHexString());
                var ss = string.Join("|", s);
                LogHardware?.Invoke($"Q data: {ss}");
            }
        }

        public void CleanCommandQueue()
        {
            ResponseCommandsQueue?.Clear();
        }
    }
}
