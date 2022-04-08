using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core.Devices.SlimCardInsert
{

    public static class ByteExt
    {
        //public static byte XorCheckSum(this byte[] data)
        //{
        //    byte crc = 0;
        //    for (int i = 0; i < data.Length; ++i)
        //    {
        //        crc = (byte)(crc ^ data[i]);
        //    }
        //    return crc;
        //}

        public static int ByteToInt(this byte data)
        {
            return Convert.ToInt32(data);
        }
        //public static byte CheckSum(this byte[] data)
        //{
        //    byte crc = 0;
        //    for (int i = 0; i < data.Length; ++i)
        //    {
        //        crc = (byte)(crc ^ data[i]);
        //    }
        //    return crc;
        //}

        public static string TryGetValue(this Dictionary<string, string> dict, string key)
        {
            if (dict.TryGetValue(key, out string value))
            {
                return value.HexStringToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static byte[] StringToByteArray(this String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] CmdToByteArray(this String cmd)
        {
            int NumberChars = cmd.Length;
            byte[] bytes = new byte[NumberChars];
            for (int i = 0; i < NumberChars; i += 1) bytes[i] = Convert.ToByte(cmd.Substring(i, 1), 16);
            return bytes;
        }

        public static string HexStringToString(this String hex)
        {
            return Encoding.ASCII.GetString(hex.StringToByteArray());
        }


        public static string ToHexString(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
                s.Append(" ");
            }
            return s.ToString();
        }


        public static string ToHexStringNoSpace(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                s.Append(b.ToString("x2").ToUpper());
            }
            return s.ToString();
        }

        public static string ToAsiiString(this byte[] hex)
        {
            return Encoding.UTF8.GetString(hex, 0, hex.Length);
        }

        public static byte[] AsiiToBytes(this String data)
        {
            return ASCIIEncoding.ASCII.GetBytes(data);
        }

        public static byte[] ToHexBytes(this string hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return new byte[0];

            int l = hex.Length / 2;
            var b = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return b;
        }

        public static byte[] CheckSumCrc16(this byte[] data)
        {
            short num = 0;
            for (int index1 = 0; index1 < data.Length - 2; ++index1)
            {
                num ^= (short)(data[index1] << 8);
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    if ((num & 32768) != 0)
                        num = (short)(num << 1 ^ 0x1021);
                    else
                        num <<= 1;
                }
            }
            var sss = BitConverter.GetBytes(num).ToHexString();
            return BitConverter.GetBytes(num).Reverse().ToArray();
        }

        public static byte[] IntTo2Bytes1(this int number)
        {
            return BitConverter.GetBytes(number).Take(2).ToArray();
        }

        public static byte[] IntToBcd(this int number)
        {
            return number.IntTo2Bytes1().Reverse().ToArray();
            //return number.ToString().PadLeft(4, '0').StringToByteArray();
        }

        public static int BcdToInt(this byte[] bytes)
        {
            // 00 03 to 0003
            //var intString = bytes.ToHexStringNoSpace();
            //int.TryParse(intString, out int number);
            return 256 * bytes[0] + bytes[1];
        }

        public static byte XorCheckSum(this byte[] bytes)
        {
            return bytes.Aggregate<byte, byte>(0, (x, y) => (byte)(x ^ y));
        }
    }
}
