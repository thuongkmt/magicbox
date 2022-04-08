using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO.GrabPay
{
    public class GrabPayMbChargeResponse
    {
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public string RefundId { get; set; }
    }
}
