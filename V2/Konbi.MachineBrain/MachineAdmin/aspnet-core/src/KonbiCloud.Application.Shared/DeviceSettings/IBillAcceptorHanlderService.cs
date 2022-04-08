using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.DeviceSettings
{
    public interface IBillAcceptorHanlderService
    {
        void Reset();
        void Enable();
        void Disable();
    }
}
