using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class ValidateByAuthCodeDto
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("balanceGreaterThan")]
        public int BalanceGreaterThan { get; set; }
        [JsonProperty("isCardLinked")]
        public bool IsCardLinked { get; set; }
    }

    public class ChargeWithTokenDto
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
