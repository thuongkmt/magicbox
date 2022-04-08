using Newtonsoft.Json;

namespace LinePayCSharp
{
    /// <summary>
    /// Preapproved Payment Response
    /// </summary>
    public class PreApprovedPayResponse : ResponseBase
    {
        [JsonProperty("info")]
        public PreApprovedPayInfo Info { get; set; }       
    }
}
