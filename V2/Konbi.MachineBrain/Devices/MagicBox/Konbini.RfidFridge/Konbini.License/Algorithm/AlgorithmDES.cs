namespace Konbini.Algorithm
{
    using Konbini.Encrypt;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;

    public class AlgorithmDES : Encryptor
    {
        public AlgorithmDES(string secretKey, AlgorithmKeyType AlgType) : base(secretKey, AlgType)
        {
        }

        public override void GenerateKey(string secretKey, AlgorithmKeyType type)
        {
            this.Key = new byte[8];
            this.IV = new byte[8];
            byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
            switch (type)
            {
                case AlgorithmKeyType.None:
                    return;

                case AlgorithmKeyType.SHA1:
                    break;

                case AlgorithmKeyType.SHA256:
                    goto TR_0025;

                case AlgorithmKeyType.SHA384:
                    goto TR_0019;

                case AlgorithmKeyType.SHA512:
                    goto TR_000D;

                case AlgorithmKeyType.MD5:
                {
                    using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
                    {
                        provider.ComputeHash(bytes);
                        byte[] hash = provider.Hash;
                        int index = 0;
                        while (true)
                        {
                            if (index >= 8)
                            {
                                for (int i = 15; i >= 8; i--)
                                {
                                    this.IV[15 - i] = hash[i];
                                }
                                break;
                            }
                            this.Key[index] = hash[index];
                            index++;
                        }
                        return;
                    }
                }
                default:
                    return;
            }
            using (SHA1Managed managed = new SHA1Managed())
            {
                managed.ComputeHash(bytes);
                byte[] hash = managed.Hash;
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        for (int i = 0x13; i > 11; i--)
                        {
                            this.IV[0x13 - i] = hash[i];
                        }
                        break;
                    }
                    this.Key[index] = hash[index];
                    index++;
                }
                return;
            }
            goto TR_0025;
        TR_000D:
            using (SHA512Managed managed4 = new SHA512Managed())
            {
                managed4.ComputeHash(bytes);
                byte[] hash = managed4.Hash;
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        for (int i = 0x3f; i > 0x37; i--)
                        {
                            this.IV[0x3f - i] = hash[i];
                        }
                        break;
                    }
                    this.Key[index] = hash[index];
                    index++;
                }
            }
            return;
        TR_0019:
            using (SHA384Managed managed3 = new SHA384Managed())
            {
                managed3.ComputeHash(bytes);
                byte[] hash = managed3.Hash;
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        for (int i = 0x2f; i > 0x27; i--)
                        {
                            this.IV[0x2f - i] = hash[i];
                        }
                        break;
                    }
                    this.Key[index] = hash[index];
                    index++;
                }
                return;
            }
            goto TR_000D;
        TR_0025:
            using (SHA256Managed managed2 = new SHA256Managed())
            {
                managed2.ComputeHash(bytes);
                byte[] hash = managed2.Hash;
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        for (int i = 0x1f; i >= 0x18; i--)
                        {
                            this.IV[0x1f - i] = hash[i];
                        }
                        break;
                    }
                    this.Key[index] = hash[index];
                    index++;
                }
                return;
            }
            goto TR_0019;
        }

        public override byte[] Transform(byte[] data, TransformType type)
        {
            MemoryStream stream = null;
            ICryptoTransform transform = null;
            byte[] buffer;
            DES des = DES.Create();
            try
            {
                stream = new MemoryStream();
                des.Key = this.Key;
                des.IV = this.IV;
                transform = (type != TransformType.ENCRYPT) ? des.CreateDecryptor() : des.CreateEncryptor();
                if ((data == null) || (data.Length == 0))
                {
                    buffer = null;
                }
                else
                {
                    CryptoStream stream1 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                    stream1.Write(data, 0, data.Length);
                    stream1.FlushFinalBlock();
                    buffer = stream.ToArray();
                }
            }
            catch (CryptographicException exception1)
            {
                throw new CryptographicException(exception1.Message);
            }
            finally
            {
                if (des != null)
                {
                    des.Clear();
                }
                if (transform != null)
                {
                    transform.Dispose();
                }
                stream.Close();
            }
            return buffer;
        }

        private byte[] Key { get; set; }

        private byte[] IV { get; set; }
    }
}

