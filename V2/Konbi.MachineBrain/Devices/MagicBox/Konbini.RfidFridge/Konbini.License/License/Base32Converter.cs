namespace Konbini.License
{
    using System;

    public class Base32Converter
    {
        private static readonly char[] BASE32_TABLE = new char[] { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static byte[] FromBase32String(string base32)
        {
            byte[] sourceArray = new byte[base32.Length];
            int num = base32.Length % 5;
            if (num != 0)
            {
                throw new InvalidOperationException("Base32 input string incorrect. Required multiple of 5 character length.");
            }
            int num2 = base32.Length - num;
            int length = 0;
            for (int i = 0; i < num2; i += 5)
            {
                long num4 = ((((GetBase32Number(base32[i]) << 0x13) | (GetBase32Number(base32[i + 1]) << 14)) | (GetBase32Number(base32[i + 2]) << 9)) | (GetBase32Number(base32[i + 3]) << 4)) | ((byte) GetBase32Number(base32[i + 4]));
                sourceArray[length + 0] = (byte) ((num4 & 0xff0000L) >> 0x10);
                sourceArray[length + 1] = (byte) ((num4 & 0xff00L) >> 8);
                sourceArray[length + 2] = (byte) (num4 & 0xffL);
                length += 3;
            }
            byte[] destinationArray = new byte[length];
            Array.Copy(sourceArray, 0, destinationArray, 0, length);
            return destinationArray;
        }

        private static int GetBase32Number(char c)
        {
            if (((c == 'I') || ((c == 'O') || (c == 'Q'))) || (c == 'U'))
            {
                throw new ArgumentOutOfRangeException();
            }
            int num = c - '0';
            if (num > 9)
            {
                num -= 0x10;
                if (num > 9)
                {
                    num--;
                    if (num > 13)
                    {
                        num--;
                        if (num > 14)
                        {
                            num--;
                            if (num > 0x11)
                            {
                                num--;
                            }
                        }
                    }
                }
                num += 9;
            }
            if ((num < 0) || (num > 0x1f))
            {
                throw new ArgumentOutOfRangeException();
            }
            return num;
        }

        public static string ToBase32String(byte[] buffer)
        {
            char[] chArray = new char[buffer.Length * 2];
            int num = buffer.Length % 3;
            if (num != 0)
            {
                throw new InvalidOperationException("Input data incorrect. Required multiple of 3 bytes length.");
            }
            int num2 = buffer.Length - num;
            int length = 0;
            for (int i = 0; i < num2; i += 3)
            {
                chArray[length + 0] = BASE32_TABLE[(buffer[i] & 0xf8) >> 3];
                chArray[length + 1] = BASE32_TABLE[((buffer[i] & 7) << 2) | ((buffer[i + 1] & 0xc0) >> 6)];
                chArray[length + 2] = BASE32_TABLE[(buffer[i + 1] & 0x3e) >> 1];
                chArray[length + 3] = BASE32_TABLE[((buffer[i + 1] & 1) << 4) | ((buffer[i + 2] & 240) >> 4)];
                chArray[length + 4] = BASE32_TABLE[buffer[i + 2] & 15];
                length += 5;
            }
            return new string(chArray, 0, length);
        }
    }
}

