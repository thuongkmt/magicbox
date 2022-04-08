using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Enums
{
    public enum CommandState
    {
        Sending, // is sending out
        Cancelled,
        Failed, // sent unsuccessfully
        SendSuccess, // Message has sent completely
        Received, // Message has been delivered to target device
        TimeOut // no ACK message from target device within time.
    }
}
