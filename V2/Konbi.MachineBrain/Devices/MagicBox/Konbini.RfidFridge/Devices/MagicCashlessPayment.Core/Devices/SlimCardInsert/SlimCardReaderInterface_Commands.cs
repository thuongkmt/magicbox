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
        public const byte EQN = 0x05;
        public const byte ACK = 0x06;
        public const byte NACK = 0x15;
        public const byte EOT = 0x04;
        public const byte STX = 0x02;
        public const byte ETX = 0x03;

        public static class Commands
        {
            public static class Generic
            {
                public static byte[] ACK()
                {
                    return new byte[] { 0x06 };
                }
                public static byte[] NACK()
                {
                    return new byte[] { 0x15 };
                }

                public static byte[] EQN()
                {
                    return new byte[] { 0x05 };
                }
            }
            public static class CardReaderReset
            {
                public const byte CM = 0x30;
                public static byte[] Reset()
                {
                    return BuildCommand(CM, PM: 0x30);
                }
            }

            public static class CheckStatus
            {
                public const byte CM = 0x31;
           
                public static byte[] CardStatus()
                {
                    return BuildCommand(CM, PM: 0x30);
                }

                public static byte[] AutoTestCard()
                {
                    return BuildCommand(CM, PM: 0x31);
                }
            }

            public static class ControlCommand
            {
                public const byte CM = 0x32;
                public static byte[] EjectCard()
                {
                    return BuildCommand(CM, PM: 0x30);
                }
            }


            private static byte[] BuildCommand(byte CM, byte PM, byte[] DATA = null)
            {
                var mainCmd = new List<byte>
                    {
                        CM,PM
                    };


                if (DATA != null)
                {
                    mainCmd.AddRange(DATA);
                }

                var lengthBytes = mainCmd.Count.IntToBcd();

                // General
                var generalCmd = new List<byte>();
                generalCmd.Add(STX);
                generalCmd.AddRange(lengthBytes);
                generalCmd.AddRange(mainCmd);
                generalCmd.Add(ETX);

                var checksum = generalCmd.ToArray().XorCheckSum();
                generalCmd.Add(checksum);


                return generalCmd.ToArray();
            }
        }



    }
}
