using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Extensions
{
    public static class VHExtension
    {
        public static int ByteToInt(this byte data)
        {
            return Convert.ToInt32(data);
        }
        public static byte CheckSum(this byte[] data)
        {
            byte crc = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                crc = (byte)(crc ^ data[i]);
            }
            return crc;
        }

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
                var hexs = hex.Substring(i * 2, 2);
                b[i] = Convert.ToByte(hexs, 16);
            }
            return b;
        }

        public static byte[] IntToBcd(this int number)
        {
            return number.ToString().PadLeft(4, '0').StringToByteArray();
        }

    }
}
