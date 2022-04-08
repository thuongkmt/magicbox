using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Enums
{
    public enum TransactionAction
    {
        START = 0,
        TAKE_OUT_ITEM = 1,
        PUT_BACK_ITEM = 2,
        END = 99,
    }
}
