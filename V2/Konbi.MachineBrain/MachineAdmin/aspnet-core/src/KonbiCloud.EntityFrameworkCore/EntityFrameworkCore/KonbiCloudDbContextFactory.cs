using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Configuration;
using KonbiCloud.Web;

namespace KonbiCloud.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class KonbiCloudDbContextFactory : IDesignTimeDbContextFactory<KonbiCloudDbContext>
    {
        public KonbiCloudDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<KonbiCloudDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder(), addUserSecrets: true);

            KonbiCloudDbContextConfigurer.Configure(builder, configuration.GetConnectionString(KonbiCloudConsts.ConnectionStringName));

            return new KonbiCloudDbContext(builder.Options);
        }
    }
}