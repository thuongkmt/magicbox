using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace KonbiCloud.Localization
{
    public static class KonbiCloudLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(
                    KonbiCloudConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(KonbiCloudLocalizationConfigurer).GetAssembly(),
                        "KonbiCloud.Localization.KonbiCloud"
                    )
                )
            );
        }
    }
}