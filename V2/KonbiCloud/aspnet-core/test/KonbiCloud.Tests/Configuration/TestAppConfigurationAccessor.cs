using Abp.Dependency;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Configuration;

namespace KonbiCloud.Tests.Configuration
{
    public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
    {
        public IConfigurationRoot Configuration { get; }

        public TestAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(
                typeof(KonbiCloudTestModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }
    }
}
