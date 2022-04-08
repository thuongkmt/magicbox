using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Konbini.SharedModels
{
    public class RestApiObjectList<T> where T:EntityBase
    {
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]

        public List<T> Result { get; set; }
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]

        public bool Success { get; set; }

        [JsonProperty("unAuthorizedRequest", NullValueHandling = NullValueHandling.Ignore)]

        public bool UnAuthorizedRequest { get; set; }
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]

        public string Error { get; set; }
    }
}
