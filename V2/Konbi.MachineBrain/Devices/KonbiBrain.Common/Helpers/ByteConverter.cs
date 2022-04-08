using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.Common.Helpers
{
    public static class ByteConverter
    {
        public static string ByteArrayToString(this byte[] byteArray)
        {
            string result = System.Text.Encoding.UTF8.GetString(byteArray);
            return result;
        }

        public static string ByteToHex(this byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
    }
}
