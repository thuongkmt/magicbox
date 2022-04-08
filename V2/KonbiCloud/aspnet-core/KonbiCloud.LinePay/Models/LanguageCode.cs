﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Language codes on the payment waiting screen
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LanguageCode
    {
        ja,
        ko,
        en,
        zh_Hans,
        zh_Hant,
        th
    }
}
