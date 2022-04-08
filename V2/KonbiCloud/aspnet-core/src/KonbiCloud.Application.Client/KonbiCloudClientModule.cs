using Abp.Modules;
using Abp.Reflection.Extensions;

namespace KonbiCloud
{
    public class KonbiCloudClientModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudClientModule).GetAssembly());
        }
    }
}
