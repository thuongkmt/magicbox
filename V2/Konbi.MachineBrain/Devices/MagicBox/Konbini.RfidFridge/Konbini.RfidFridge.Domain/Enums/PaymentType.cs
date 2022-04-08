using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Enums
{
    public enum PaymentType
    {
        NAYAX = 0,
        MAGIC = 1,
        NONE = 2,
        QR = 3,
        QR_MAGIC = 4,
        QR_NAYAX = 5,
        PAYTER = 6,

    }

    public enum MagicPaymentTerminalType
    {
        IUC = 0,
        R2 = 1,
        IM30 = 2,
        PAX = 3
    }


    public enum MagicPaymentCardInsertType
    {
        DEFAULT = 0,
        SLIM = 1
    }

    public enum CardPaymentType
    {
        NONE = -1,
        CPAS = 0,
        CREDITCARD = 1,
        GRABPAY = 2,
        WALLET = 3,
        CREDITCARD_WALLET = 4
    }

    public enum QrPaymentType
    {
        GRABPAY = 0,
        WALLET = 1,
        CREDITCARD_WALLET = 2,
        ALL = 3
    }

    public enum VALIDATE_ERROR_TYPE
    {
        NONE = -1,
        GENERAL_ERROR = 0,
        CANT_READ_CARD = 1,
        EZLINK_BALANCE_NOT_ENOUGHT = 2,
        EZLINK_BLACKLIST = 3,
        
    }

    public enum TerminalCommand
    {
        IUC_BL = 0
    }

    public enum CustomerAction
    {
        PRESS_START_BUTTON = 0
    }


}
