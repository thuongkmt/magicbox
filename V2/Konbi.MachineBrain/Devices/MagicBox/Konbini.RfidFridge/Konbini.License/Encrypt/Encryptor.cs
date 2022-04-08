namespace Konbini.Encrypt
{
    using Konbini.Algorithm;
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public abstract class Encryptor
    {
        public Encryptor(string secretKey, AlgorithmKeyType AlgType)
        {
            this.GenerateKey(secretKey, AlgType);
        }

        public string Decrypt(string data)
        {
            string str;
            try
            {
                if (data.Length <= 0)
                {
                    str = null;
                }
                else
                {
                    byte[] buffer = Convert.FromBase64String(data);
                    str = Encoding.UTF8.GetString(this.Transform(buffer, TransformType.DECRYPT));
                }
            }
            catch (CryptographicException exception1)
            {
                throw exception1;
            }
            return str;
        }

        public string Encrypt(string data)
        {
            string str;
            try
            {
                if (data.Length <= 0)
                {
                    str = null;
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    str = Convert.ToBase64String(this.Transform(bytes, TransformType.ENCRYPT));
                }
            }
            catch (CryptographicException exception1)
            {
                throw exception1;
            }
            return str;
        }

        public abstract void GenerateKey(string secretKey, AlgorithmKeyType type);
        public string ObjectCryptography(string data, TransformType type)
        {
            string str = null;
            try
            {
                if (data.Length > 0)
                {
                    if (type == TransformType.ENCRYPT)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(data);
                        str = Convert.ToBase64String(this.Transform(bytes, TransformType.ENCRYPT));
                    }
                    else if (type == TransformType.DECRYPT)
                    {
                        byte[] buffer2 = Convert.FromBase64String(data);
                        str = Encoding.UTF8.GetString(this.Transform(buffer2, TransformType.DECRYPT));
                    }
                }
            }
            catch (CryptographicException exception1)
            {
                throw exception1;
            }
            return str;
        }

        public byte[] ObjectCryptography(byte[] data, TransformType type)
        {
            byte[] buffer = null;
            try
            {
                if ((data != null) && (data.Length != 0))
                {
                    if (type == TransformType.ENCRYPT)
                    {
                        buffer = this.Transform(data, TransformType.ENCRYPT);
                    }
                    else if (type == TransformType.DECRYPT)
                    {
                        buffer = this.Transform(data, TransformType.DECRYPT);
                    }
                }
            }
            catch (CryptographicException exception1)
            {
                throw exception1;
            }
            return buffer;
        }

        public abstract byte[] Transform(byte[] data, TransformType type);
    }
}

