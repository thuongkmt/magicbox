using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    public interface ITransactionService
    {

        void Add(List<InventoryDto> items, TransactionStatus status, object saleResponse, PaymentType paymentType,  CardPaymentType card, List<string> transactionImages);
    }
}
