using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KonbiCloud.LinePay.Models
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
