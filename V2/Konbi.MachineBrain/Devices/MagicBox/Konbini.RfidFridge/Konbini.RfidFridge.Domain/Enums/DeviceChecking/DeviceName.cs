using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Enums.DeviceChecking
{
    public enum DeviceName
    {
        RFID_READER,
        LOCK_CONTROLLER,
        PAYMENT_TERMINAL,
        CARD_INSERT,
        QRCODE_READER
    }

    public enum DeviceStatus
    {
        CHECKING,
        OK,
        ERROR
    }
}
