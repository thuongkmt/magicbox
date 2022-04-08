using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.Common
{
    public class Converter
    {
        public static string ByteArrayToHexString(IEnumerable<byte> bytes)
        {
            return string.Join(string.Empty, Array.ConvertAll(bytes.ToArray(), b => b.ToString("X2")));
        }

        public static int ByteArrayToIntValue(IEnumerable<byte> bytes)
        {
            return Int32.Parse(ByteArrayToHexString(bytes), System.Globalization.NumberStyles.HexNumber);

        }

        public static decimal ByteArrayToDecValue(IEnumerable<byte> bytes)
        {
            return (decimal)Int64.Parse(ByteArrayToHexString(bytes), System.Globalization.NumberStyles.HexNumber);

        }    
    }
}
