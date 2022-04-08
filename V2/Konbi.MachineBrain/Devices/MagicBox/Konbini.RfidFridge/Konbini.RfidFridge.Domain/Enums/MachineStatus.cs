using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Enums
{
    public enum MachineStatus
    {
        NONE = -1,
        IDLE,
        TRANSACTION_START,
        TRANSACTION_FAILED,
        TRANSACTION_DONE,
        STOPSALE,
        MANUAL_STOPSALE,
        TRANSACTION_DOOR_CLOSED,
        VALIDATE_CARD_FAILED,
        STOPSALE_DUE_TO_PAYMENT,
        TRANSACTION_WAITTING_FOR_PAYMENT,
        FAIL_TO_OPEN_THE_DOOR,
        UNSTABLE_TAGS_DIAGNOSTIC,
        UNSTABLE_TAGS_DIAGNOSTIC_TRACING,
        UNLOADING_PRODUCT,
        TRANSACTION_CONFIRM,
        TRANSACTION_REOPENDOOR,
        RESTOCKING_PRODUCT,
        UI_STARTED,
        DEVICE_CHECKING,
        DEVICE_CHECKING_ERROR,
        RESTART,
        PREAUTH_MAKE_PAYMENT,
        PREAUTH_REFUND
    }

    public enum DoorState
    {
        OPEN,
        CLOSE
    }

    public enum TagChangeEvent
    {
        ADDED = 0,
        REMOVED = 1
    }
}
