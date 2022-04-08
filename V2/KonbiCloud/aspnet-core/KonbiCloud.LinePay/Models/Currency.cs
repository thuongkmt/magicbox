using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Supported currencies.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Currency
    {
        USD,
        JPY,
        TWD,
        THB
    }
}
