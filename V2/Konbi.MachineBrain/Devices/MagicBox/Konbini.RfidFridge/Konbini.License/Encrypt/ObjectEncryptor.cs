namespace Konbini.Encrypt
{
    using Konbini.Algorithm;
    using System;
    using System.Runtime.CompilerServices;

    public class ObjectEncryptor
    {
        public virtual string ObjectCryptography(string secretKey, string data, TransformType type, AlgorithmType algType, AlgorithmKeyType algKeyType)
        {
            string str = null;
            switch (algType)
            {
                case AlgorithmType.Rijndael:
                    this.Encryptor = new AlgorithmRijndael(secretKey, algKeyType);
                    str = this.Encryptor.ObjectCryptography(data, type);
                    break;

                case AlgorithmType.TripleDES:
                    this.Encryptor = new AlgorithmTripleDES(secretKey, algKeyType);
                    str = this.Encryptor.ObjectCryptography(data, type);
                    break;

                case AlgorithmType.DES:
                    this.Encryptor = new AlgorithmDES(secretKey, algKeyType);
                    str = this.Encryptor.ObjectCryptography(data, type);
                    break;

                default:
                    break;
            }
            return str;
        }

        protected Konbini.Encrypt.Encryptor Encryptor { get; set; }
    }
}

