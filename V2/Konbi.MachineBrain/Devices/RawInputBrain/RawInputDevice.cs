using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInputBrain
{
    public class RawInputDevice
    {
        public string FriendlyName;

        public string Name;

        public string Manufacturer;

        public string ProductId;

        public string VendorId;

        public string Version;

        public object Device;

        public override string ToString()
        {
            return FriendlyName;
        }
    }
}
