using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Enums
{
    public enum FridgeReaderMode
    {
        STORAGE = 0x00,
        PAYMENT = 0x01,
        NORMAL = 0x02,
        ABNORMAL = 0x03,
        NONE = 0xFF,
    }

    public enum FridgeReaderMask
    {
        MISS = 0x01,
        EXIST = 0x02,
        CHANGE = 0x04,
        ABNORMAL = 0x08,
        NONE = 0xFF,
    }

    public enum FridgeReaderTagType
    {
        MISS = 0x00,
        EXIST = 0x01,
        CHANGE = 0x02,
        ABNORMAL = 0x03,
    }

    public enum FridgeReaderVersion
    {
        V1 = 1,
        V2 = 2,
    }

}
