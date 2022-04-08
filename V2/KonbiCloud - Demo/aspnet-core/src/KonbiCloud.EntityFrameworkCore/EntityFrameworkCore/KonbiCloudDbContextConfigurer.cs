using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.EntityFrameworkCore
{
    public static class KonbiCloudDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<KonbiCloudDbContext> builder, string connectionString)
        {
            //builder.UseSqlServer(connectionString);
            builder.UseMySql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<KonbiCloudDbContext> builder, DbConnection connection)
        {
            //builder.UseSqlServer(connection);
            builder.UseMySql(connection);
        }
    }
}