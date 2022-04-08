namespace Konbini.License
{
    using Konbini.Algorithm;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class KeyManager
    {
        private string EncryptionKey = string.Empty;

        public KeyManager(string encryptionKey)
        {
            this.EncryptionKey = encryptionKey + "";
        }

        public bool DisassembleKey(string ProductKey, ref KeyValuesClass KeyValues)
        {
            bool flag;
         
            try
            {
                if (string.IsNullOrEmpty(ProductKey))
                {
                    throw new ArgumentNullException("Product Key is null or empty.");
                }
                if (ProductKey.Length != 0x23)
                {
                    throw new ArgumentException("Product key is invalid.");
                }
                byte[] buffer = new byte[2];
                byte[] buffer2 = new byte[1];
                byte[] buffer3 = new byte[0x10];
                using (MemoryStream stream = new MemoryStream(Base32Converter.FromBase32String(ProductKey.Replace("-", ""))))
                {
                    stream.Read(buffer3, 0, 8);
                    stream.Read(buffer, 0, 1);
                    stream.Read(buffer2, 0, 1);
                    stream.Read(buffer3, 8, buffer3.Length - 8);
                    stream.ToArray();
                }
                byte[] bytes = Encoding.UTF8.GetBytes(this.EncryptionKey);
                byte[] rgbKey = new byte[0x20];
                int index = 0;
                while (true)
                {
                    if (index >= 0x20)
                    {
                        byte[] rgbIV = new byte[0x10];
                        int num = 15;
                        int num3 = bytes.Length - 1;
                        while (true)
                        {
                            if (num3 < 0x17)
                            {
                                byte[] buffer7 = new byte[2];
                                byte[] buffer8 = new byte[2];
                                byte[] buffer9 = new byte[2];
                                byte[] buffer10 = new byte[2];
                                byte[] buffer11 = new byte[2];
                                byte[] buffer12 = new byte[4];
                                byte[] buffer13 = new byte[2];
                                byte[] buffer14 = new byte[2];
                                using (MemoryStream stream2 = new MemoryStream(new RijndaelManaged().CreateDecryptor(rgbKey, rgbIV).TransformFinalBlock(buffer3, 0, buffer3.Length)))
                                {
                                    stream2.Read(buffer7, 0, 1);
                                    stream2.Read(buffer8, 0, 1);
                                    stream2.Read(buffer9, 0, 1);
                                    stream2.Read(buffer10, 0, 1);
                                    stream2.Read(buffer11, 0, 1);
                                    stream2.Read(buffer12, 0, 4);
                                    stream2.Read(buffer13, 0, 1);
                                    stream2.Read(buffer14, 0, 1);
                                }
                                KeyValuesClass class2 = new KeyValuesClass
                                {
                                    Header = (byte)BitConverter.ToInt16(buffer7, 0),
                                    ProductCode = (byte)BitConverter.ToInt16(buffer8, 0),
                                    Version = (byte)BitConverter.ToInt16(buffer9, 0),
                                    Edition = (Edition)BitConverter.ToInt16(buffer10, 0),
                                    Type = (LicenseType)BitConverter.ToInt16(buffer11, 0)
                                };
                                if (class2.Type == LicenseType.TRIAL)
                                {
                                    string str = BitConverter.ToUInt32(buffer12, 0).ToString().PadLeft(8, '0');
                                    class2.Expiration = new DateTime(Convert.ToInt16(str.Substring(4, 4)), Convert.ToInt16(str.Substring(2, 2)), Convert.ToInt16(str.Substring(0, 2)));
                                }
                                class2.Footer = (byte)BitConverter.ToInt16(buffer14, 0);
                                KeyValues = class2;
                                flag = true;
                                break;
                            }
                            rgbIV[num] = bytes[num3];
                            num--;
                            num3--;
                        }
                        break;
                    }
                    rgbKey[index] = bytes[index];
                    index++;
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public bool GenerateKey(KeyValuesClass KeyValues, ref string ProductKey)
        {
            bool flag;
            CspParameters parameters = new CspParameters
            {
                Flags = CspProviderFlags.UseMachineKeyStore
            };
            RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider(0x400, parameters);
            try
            {
                byte[] buffer9;
                byte[] bytes = BitConverter.GetBytes((short)KeyValues.Header);
                byte[] buffer2 = BitConverter.GetBytes((short)KeyValues.ProductCode);
                byte[] buffer3 = BitConverter.GetBytes((short)KeyValues.Version);
                byte[] buffer4 = BitConverter.GetBytes((short)((byte)KeyValues.Edition));
                byte[] buffer5 = BitConverter.GetBytes((short)((byte)KeyValues.Type));
                byte[] buffer6 = BitConverter.GetBytes(Convert.ToUInt32((string.Empty + KeyValues.Expiration.Day.ToString().PadLeft(2, '0')) + KeyValues.Expiration.Month.ToString().PadLeft(2, '0') + KeyValues.Expiration.Year.ToString()));
                byte[] buffer7 = BitConverter.GetBytes(0xff);//BitConverter.GetBytes(new Random().Next(0, 0xff));
                byte[] buffer8 = BitConverter.GetBytes((short)KeyValues.Footer);
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(bytes, 0, 1);
                    stream.Write(buffer2, 0, 1);
                    stream.Write(buffer3, 0, 1);
                    stream.Write(buffer4, 0, 1);
                    stream.Write(buffer5, 0, 1);
                    stream.Write(buffer6, 0, 4);
                    stream.Write(buffer7, 0, 1);
                    stream.Write(buffer8, 0, 1);
                    buffer9 = stream.ToArray();
                }
                byte[] buffer10 = Encoding.UTF8.GetBytes(this.EncryptionKey);
                byte[] rgbKey = new byte[0x20];
                int index = 0;
                while (true)
                {
                    if (index >= 0x20)
                    {
                        byte[] rgbIV = new byte[0x10];
                        int num = 15;
                        int num4 = buffer10.Length - 1;
                        while (true)
                        {
                            if (num4 < 23)
                            {
                                byte[] buffer14;
                                byte[] buffer13 = new RijndaelManaged().CreateEncryptor(rgbKey, rgbIV).TransformFinalBlock(buffer9, 0, buffer9.Length);
                                using (MemoryStream stream2 = new MemoryStream())
                                {
                                    stream2.Write(buffer13, 0, 8);
                                    stream2.Write(bytes, 0, 1);
                                    stream2.Write(buffer7, 0, 1);
                                    stream2.Write(buffer13, 8, buffer13.Length - 8);
                                    buffer14 = stream2.ToArray();
                                }
                                string str = Base32Converter.ToBase32String(buffer14);
                                ProductKey = $"{str.Substring(0, 5)}-{str.Substring(5, 5)}-{str.Substring(10, 5)}-{str.Substring(15, 5)}-{str.Substring(20, 5)}-{str.Substring(0x19, 5)}";
                                flag = true;
                                break;
                            }
                            rgbIV[num] = buffer10[num4];
                            num--;
                            num4--;
                        }
                        break;
                    }
                    rgbKey[index] = buffer10[index];
                    index++;
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public int LoadSuretyFile(string filename, ref LicenseInfo LicInfo)
        {
            int num;
            if (!File.Exists(filename))
            {
                return -1;
            }
            ObjectPacketLicense license = new ObjectPacketLicense(filename);
            try
            {
                LicenseInfo info = license.ReadLicense(this.EncryptionKey, 1);
                if (info == null)
                {
                    num = -3;
                }
                else
                {
                    LicInfo = info;
                    num = 1;
                }
            }
            catch
            {
                num = -2;
            }
            return num;
        }

        public bool SaveSuretyFile(string filename, LicenseInfo licInfo)
        {
            try
            {
                new ObjectPacketLicense(filename).SaveLicenseToFile(this.EncryptionKey, licInfo, 1, Konbini.Algorithm.AlgorithmType.Rijndael, AlgorithmKeyType.MD5);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidKey(ref string ProductKey)
        {
            bool flag;
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            try
            {
                if (string.IsNullOrEmpty(ProductKey))
                {
                    throw new ArgumentNullException("Product Key is null or empty.");
                }
                if (ProductKey.Length != 0x23)
                {
                    throw new ArgumentException("Product key is invalid.");
                }
                byte[] buffer = new byte[1];
                byte[] buffer2 = new byte[1];
                byte[] buffer3 = new byte[0x10];
                using (MemoryStream stream = new MemoryStream(Base32Converter.FromBase32String(ProductKey.Replace("-", ""))))
                {
                    stream.Read(buffer3, 0, 8);
                    stream.Read(buffer, 0, 1);
                    stream.Read(buffer2, 0, 1);
                    stream.Read(buffer3, 8, buffer3.Length - 8);
                    stream.ToArray();
                }
                byte[] bytes = Encoding.UTF8.GetBytes(this.EncryptionKey);
                byte[] rgbKey = new byte[0x20];
                int index = 0;
                while (true)
                {
                    if (index >= 0x20)
                    {
                        byte[] rgbIV = new byte[0x10];
                        int num = 15;
                        int num3 = bytes.Length - 1;
                        while (true)
                        {
                            if (num3 < 23)
                            {
                                new RijndaelManaged().CreateDecryptor(rgbKey, rgbIV).TransformFinalBlock(buffer3, 0, buffer3.Length);
                                flag = true;
                                break;
                            }
                            rgbIV[num] = bytes[num3];
                            num--;
                            num3--;
                        }
                        break;
                    }
                    rgbKey[index] = bytes[index];
                    index++;
                }
            }
            catch
            {
                flag = false;
            }
            finally
            {
                if (provider != null)
                {
                    //provider
                }
            }
            return flag;
        }
    }
}

