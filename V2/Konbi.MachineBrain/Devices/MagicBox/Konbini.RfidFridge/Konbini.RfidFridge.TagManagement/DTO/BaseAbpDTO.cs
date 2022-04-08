using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public abstract class BaseAbpDTO<T>
    {
        public class Data
        {
            [JsonProperty("result")]
            public Result Result { get; set; }

            [JsonProperty("targetUrl")]
            public object TargetUrl { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error")]
            public object Error { get; set; }

            [JsonProperty("unAuthorizedRequest")]
            public bool UnAuthorizedRequest { get; set; }

            [JsonProperty("__abp")]
            public bool Abp { get; set; }
        }

        public class Result
        {
            [JsonProperty("totalCount")]
            public long TotalCount { get; set; }

            [JsonProperty("items")]
            public List<T> Items { get; set; }
        }


    }
}
