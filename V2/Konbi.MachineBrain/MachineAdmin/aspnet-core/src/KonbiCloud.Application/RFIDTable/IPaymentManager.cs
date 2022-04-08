using Abp.Dependency;
using KonbiBrain.Common.Messages.Payment;
using KonbiCloud.Enums;
using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IPaymentManager : ISingletonDependency
    {
        PaymentType PaymentType { get; }
        event EventHandler<CommandEventArgs> DeviceFeedBack;
        Task<CommandState> ActivatePaymentAsync(TransactionInfo preTransaction);
        Task CancelPaymentAsync(TransactionInfo transaction);
    }
}
