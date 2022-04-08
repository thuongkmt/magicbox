using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Payments
{
    public interface IPaymentDeviceService
    {
        bool EnablePayments();
        void DisablePayments();
    }
}
