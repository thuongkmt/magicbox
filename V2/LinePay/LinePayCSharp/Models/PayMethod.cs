using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinePayCSharp
{
    /// <summary>
    /// Payment method used
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayMethod
    {
        CREDIT_CARD, // Credit card
        BALANCE, // Balance
        DISCOUNT // Discount
    }
}
