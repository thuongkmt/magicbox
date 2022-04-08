using Newtonsoft.Json;

namespace LinePayCSharp
{
    /// <summary>
    /// Capture Response
    /// </summary>
    public class CaptureResponse : ResponseBase
    {
        /// <summary>
        /// Capture Information
        /// </summary>
        [JsonProperty("info")]
        public CaptureInfo Info { get; set; }
    }
}
