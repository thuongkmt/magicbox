using Konbini.Algorithm;
using Konbini.Encrypt;
using Konbini.License;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ObjectPacketLicense : ObjectEncryptor
{
    // Fields
    private byte[] _data;
    private string _fileName;

    // Methods
    public ObjectPacketLicense()
    {
    }

    public ObjectPacketLicense(byte[] data)
    {
        this._data = data;
    }

    public ObjectPacketLicense(string fileName)
    {
        this._fileName = fileName;
    }

    public object ByteArrayToObject(byte[] arrBytes)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(arrBytes, 0, arrBytes.Length);
            stream.Seek(0L, SeekOrigin.Begin);
            return new BinaryFormatter().Deserialize(stream);
        }
    }

    public virtual bool IsValidFileFormat(short lHeader, byte version)
    {
        bool flag;
        using (FileStream stream = new FileStream(this._fileName, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        return true;
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                flag = false;
            }
        }
        return flag;
    }

    public virtual bool IsValidFileFormat(short lHeader, byte version, out AlgorithmType algType, out AlgorithmKeyType algKeyType)
    {
        bool flag;
        using (FileStream stream = new FileStream(this._fileName, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                algType = AlgorithmType.None;
                algKeyType = AlgorithmKeyType.None;
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        algType = (AlgorithmType)reader.ReadByte();
                        algKeyType = (AlgorithmKeyType)reader.ReadByte();
                        return true;
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                flag = false;
            }
        }
        return flag;
    }

    public virtual bool IsValidStreamFormat(short lHeader, byte version)
    {
        bool flag;
        using (MemoryStream stream = new MemoryStream(this._data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        return true;
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                flag = false;
            }
        }
        return flag;
    }

    public virtual bool IsValidStreamFormat(short lHeader, byte version, out AlgorithmType algType, out AlgorithmKeyType algKeyType)
    {
        bool flag;
        using (MemoryStream stream = new MemoryStream(this._data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                algType = AlgorithmType.None;
                algKeyType = AlgorithmKeyType.None;
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        algType = (AlgorithmType)reader.ReadByte();
                        algKeyType = (AlgorithmKeyType)reader.ReadByte();
                        return true;
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                flag = false;
            }
        }
        return flag;
    }

    public byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
        {
            return null;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
    }

    protected virtual string ReadFile(short lHeader, byte version, out AlgorithmType algType, out AlgorithmKeyType algKeyType)
    {
        string str2;
        using (FileStream stream = new FileStream(this._fileName, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                string str = null;
                algType = AlgorithmType.None;
                algKeyType = AlgorithmKeyType.None;
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        algType = (AlgorithmType)reader.ReadByte();
                        algKeyType = (AlgorithmKeyType)reader.ReadByte();
                        str = reader.ReadString();
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                str2 = str;
            }
        }
        return str2;
    }

    public virtual LicenseInfo ReadLicense(string secretKey, byte version)
    {
        char[] chArray1;
        AlgorithmType type;
        AlgorithmKeyType type2;
        string str = null;
        string data = null;
        if (this._data != null)
        {
            data = this.ReadStream(5, version, out type, out type2);
        }
        else
        {
            data = this.ReadFile(5, version, out type, out type2);
        }
        if (data != null)
        {
            switch (type)
            {
                case AlgorithmType.None:
                    goto Label_00A0;

                case AlgorithmType.Rijndael:
                    goto Label_0062;

                case AlgorithmType.TripleDES:
                    goto Label_0081;

                case AlgorithmType.DES:
                    try
                    {
                        base.Encryptor = new AlgorithmDES(secretKey, type2);
                        str = base.Encryptor.ObjectCryptography(data, TransformType.DECRYPT);
                        break;
                    }
                    catch (Exception exception1)
                    {
                        throw exception1;
                    }
                    goto Label_0062;
            }
        }
        goto Label_00A2;
        Label_0062:;
        try
        {
            base.Encryptor = new AlgorithmRijndael(secretKey, type2);
            str = base.Encryptor.ObjectCryptography(data, TransformType.DECRYPT);
            goto Label_00A2;
        }
        catch (Exception exception2)
        {
            throw exception2;
        }
        Label_0081:;
        try
        {
            base.Encryptor = new AlgorithmTripleDES(secretKey, type2);
            str = base.Encryptor.ObjectCryptography(data, TransformType.DECRYPT);
            goto Label_00A2;
        }
        catch (Exception exception3)
        {
            throw exception3;
        }
        Label_00A0:
        str = data;
        Label_00A2:
        chArray1 = new char[] { '#' };
        string[] strArray = str.Split(chArray1);
        if (strArray.Length == 5)
        {
            return new LicenseInfo { FullName = strArray[0], ProductKey = strArray[1], Day = Convert.ToInt32(strArray[2]), Month = Convert.ToInt32(strArray[3]), Year = Convert.ToInt32(strArray[4]) };
        }
        return null;
    }

    protected virtual string ReadStream(short lHeader, byte version, out AlgorithmType algType, out AlgorithmKeyType algKeyType)
    {
        string str2;
        using (MemoryStream stream = new MemoryStream(this._data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                string str = null;
                algType = AlgorithmType.None;
                algKeyType = AlgorithmKeyType.None;
                try
                {
                    if ((reader.ReadInt16() == lHeader) && (reader.ReadByte() == version))
                    {
                        algType = (AlgorithmType)reader.ReadByte();
                        algKeyType = (AlgorithmKeyType)reader.ReadByte();
                        str = reader.ReadString();
                    }
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    reader.Close();
                    stream.Close();
                }
                str2 = str;
            }
        }
        return str2;
    }

    public virtual void SaveLicenseToFile(string secretKey, LicenseInfo licInfo, byte version, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        switch (algType)
        {
            case AlgorithmType.None:
                goto Label_0081;

            case AlgorithmType.Rijndael:
                break;

            case AlgorithmType.TripleDES:
                goto Label_005E;

            case AlgorithmType.DES:
                try
                {
                    base.Encryptor = new AlgorithmDES(secretKey, algKeyType);
                    this.WriteFile(licInfo, version, base.Encryptor, AlgorithmType.DES, algKeyType);
                    return;
                }
                catch (Exception exception1)
                {
                    throw exception1;
                }
                break;

            default:
                return;
        }
        try
        {
            base.Encryptor = new AlgorithmRijndael(secretKey, algKeyType);
            this.WriteFile(licInfo, version, base.Encryptor, AlgorithmType.Rijndael, algKeyType);
            return;
        }
        catch (Exception exception2)
        {
            throw exception2;
        }
        Label_005E:;
        try
        {
            base.Encryptor = new AlgorithmTripleDES(secretKey, algKeyType);
            this.WriteFile(licInfo, version, base.Encryptor, AlgorithmType.TripleDES, algKeyType);
            return;
        }
        catch (Exception exception3)
        {
            throw exception3;
        }
        Label_0081:;
        try
        {
            this.WriteFile(licInfo, version, AlgorithmType.None, AlgorithmKeyType.None);
        }
        catch (Exception exception4)
        {
            throw exception4;
        }
    }

    public virtual byte[] SaveLicenseToStream(string secretKey, LicenseInfo licInfo, byte version, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        byte[] buffer = null;
        switch (algType)
        {
            case AlgorithmType.None:
                goto Label_0087;

            case AlgorithmType.Rijndael:
                break;

            case AlgorithmType.TripleDES:
                goto Label_0063;

            case AlgorithmType.DES:
                try
                {
                    base.Encryptor = new AlgorithmDES(secretKey, algKeyType);
                    return this.WriteStream(licInfo, version, base.Encryptor, AlgorithmType.DES, algKeyType);
                }
                catch (Exception exception1)
                {
                    throw exception1;
                }
                break;

            default:
                return buffer;
        }
        try
        {
            base.Encryptor = new AlgorithmRijndael(secretKey, algKeyType);
            return this.WriteStream(licInfo, version, base.Encryptor, AlgorithmType.Rijndael, algKeyType);
        }
        catch (Exception exception2)
        {
            throw exception2;
        }
        Label_0063:;
        try
        {
            base.Encryptor = new AlgorithmTripleDES(secretKey, algKeyType);
            return this.WriteStream(licInfo, version, base.Encryptor, AlgorithmType.TripleDES, algKeyType);
        }
        catch (Exception exception3)
        {
            throw exception3;
        }
        Label_0087:;
        try
        {
            buffer = this.WriteStream(licInfo, version, AlgorithmType.None, AlgorithmKeyType.None);
        }
        catch (Exception exception4)
        {
            throw exception4;
        }
        return buffer;
    }

    protected virtual void WriteFile(LicenseInfo licInfo, byte version, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        using (FileStream stream = new FileStream(this._fileName, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                try
                {
                    writer.Write((short)5);
                    writer.Write(version);
                    writer.Write(Convert.ToByte((int)algType));
                    writer.Write(Convert.ToByte((int)algKeyType));
                    writer.Write(licInfo.Data);
                    writer.Flush();
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    writer.Close();
                    stream.Close();
                }
            }
        }
    }

    protected virtual void WriteFile(LicenseInfo licInfo, byte version, Encryptor encryptor, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        using (FileStream stream = new FileStream(this._fileName, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                try
                {
                    writer.Write((short)5);
                    writer.Write(version);
                    writer.Write(Convert.ToByte((int)algType));
                    writer.Write(Convert.ToByte((int)algKeyType));
                    writer.Write(encryptor.ObjectCryptography(licInfo.Data, TransformType.ENCRYPT));
                    writer.Flush();
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    writer.Close();
                    stream.Close();
                }
            }
        }
    }

    protected virtual byte[] WriteStream(LicenseInfo licInfo, byte version, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                try
                {
                    writer.Write((short)5);
                    writer.Write(version);
                    writer.Write(Convert.ToByte((int)algType));
                    writer.Write(Convert.ToByte((int)algKeyType));
                    writer.Write(licInfo.ProductKey);
                    writer.Flush();
                }
                catch (IOException exception1)
                {
                    throw exception1;
                }
                finally
                {
                    writer.Close();
                    stream.Close();
                }
            }
            return stream.ToArray();
        }
    }

    protected virtual byte[] WriteStream(LicenseInfo licInfo, byte version, Encryptor encryptor, AlgorithmType algType, AlgorithmKeyType algKeyType)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                try
                {
                    writer.Write((short)5);
                    writer.Write(version);
                    writer.Write(Convert.ToByte((int)algType));
                    writer.Write(Convert.ToByte((int)algKeyType));
                    writer.Write(encryptor.ObjectCryptography(licInfo.Data, TransformType.ENCRYPT));
                    writer.Flush();
                }
                catch (Exception exception1)
                {
                    throw exception1;
                }
                finally
                {
                    writer.Close();
                    stream.Close();
                }
            }
            return stream.ToArray();
        }
    }
}




