namespace Konbini.License
{
    using System;
    using System.Management;
    using System.Security.Cryptography;
    using System.Text;

    public static class ComputerInfo
    {
        private static string BaseId() => 
            (identifier("Win32_BaseBoard", "Model") + identifier("Win32_BaseBoard", "Manufacturer") + identifier("Win32_BaseBoard", "Name") + identifier("Win32_BaseBoard", "SerialNumber"));

        private static string BiosId()
        {
            string[] textArray1 = new string[] { identifier("Win32_BIOS", "Manufacturer"), identifier("Win32_BIOS", "SMBIOSBIOSVersion"), identifier("Win32_BIOS", "IdentificationCode"), identifier("Win32_BIOS", "SerialNumber"), identifier("Win32_BIOS", "ReleaseDate"), identifier("Win32_BIOS", "Version") };
            return string.Concat(textArray1);
        }

        private static string CpuId()
        {
            string str = identifier("Win32_Processor", "UniqueId");
            if (str == "")
            {
                str = identifier("Win32_Processor", "ProcessorId");
                if (str == "")
                {
                    str = identifier("Win32_Processor", "Name");
                    if (str == "")
                    {
                        str = identifier("Win32_Processor", "Manufacturer");
                    }
                    str = str + identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return str;
        }

        private static string DiskId() => 
            (identifier("Win32_DiskDrive", "Model") + identifier("Win32_DiskDrive", "Manufacturer") + identifier("Win32_DiskDrive", "Signature") + identifier("Win32_DiskDrive", "TotalHeads"));

        public static string GetComputerId()
        {
            string[] textArray1 = new string[] { "CPU >> ", CpuId(), "\nBIOS >> ", BiosId(), "\nBASE >> ", BaseId() };
            return GetHash(string.Concat(textArray1));
        }

        private static string GetHash(string s) => 
            GetHexString(new MD5CryptoServiceProvider().ComputeHash(new ASCIIEncoding().GetBytes(s)));

        private static string GetHexString(byte[] bt)
        {
            string str = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte num1 = bt[i];
                int num2 = num1 & 15;
                int num3 = (num1 >> 4) & 15;
                if (num3 > 9)
                {
                    str = str + ((char) ((num3 - 10) + 0x41)).ToString();
                }
                else
                {
                    str = str + num3.ToString();
                }
                if (num2 > 9)
                {
                    str = str + ((char) ((num2 - 10) + 0x41)).ToString();
                }
                else
                {
                    str = str + num2.ToString();
                }
                if (((i + 1) != bt.Length) && (((i + 1) % 2) == 0))
                {
                    str = str + "-";
                }
            }
            return str;
        }

        private static string identifier(string wmiClass, string wmiProperty)
        {
            string str = "";
            foreach (ManagementObject obj2 in new ManagementClass(wmiClass).GetInstances())
            {
                if (str == "")
                {
                    try
                    {
                        str = obj2[wmiProperty].ToString();
                    }
                    catch
                    {
                        continue;
                    }
                    break;
                }
            }
            return str;
        }

        private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string str = "";
            foreach (ManagementObject obj2 in new ManagementClass(wmiClass).GetInstances())
            {
                if (obj2[wmiMustBeTrue].ToString() != "True")
                {
                    continue;
                }
                if (str == "")
                {
                    try
                    {
                        str = obj2[wmiProperty].ToString();
                    }
                    catch
                    {
                        continue;
                    }
                    break;
                }
            }
            return str;
        }

        private static string MacId() => 
            identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");

        private static string VideoId() => 
            (identifier("Win32_VideoController", "DriverVersion") + identifier("Win32_VideoController", "Name"));
    }
}

