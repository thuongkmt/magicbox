using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinePayCSharp
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CreditCardBrand
    {
        VISA,
        MASTER,
        AMEX,
        DINERS,
        JCB
    }
}
