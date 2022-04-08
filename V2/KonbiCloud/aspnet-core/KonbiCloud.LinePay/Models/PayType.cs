using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Payment types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayType
    {
        NORMAL, // Single payment (Default Value)
        PREAPPROVED // Preapproved payment
    }
}
