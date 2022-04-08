using Microsoft.Extensions.Configuration;

namespace KonbiCloud.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
