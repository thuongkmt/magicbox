using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO.Tera
{
    public class CreditCardWalletResponse
    {
        [JsonProperty("is_success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("failure_code")]
        public string FailureCode { get; set; }

        [JsonProperty("failure_message")]
        public string FailureMessage { get; set; }
    }


    public class CreditCardWalletValidateQrResponse
    {
        [JsonProperty("expired")]
        public bool Expired { get; set; }
    }
}
