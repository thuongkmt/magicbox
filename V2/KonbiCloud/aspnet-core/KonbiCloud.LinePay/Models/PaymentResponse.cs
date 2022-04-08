﻿using Newtonsoft.Json;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Get Payment Details API Response
    /// </summary>
    public class PaymentResponse : ResponseBase
    {
        [JsonProperty("info")]
        public PaymentInfo[] Info { get; set; }        
    }
}
