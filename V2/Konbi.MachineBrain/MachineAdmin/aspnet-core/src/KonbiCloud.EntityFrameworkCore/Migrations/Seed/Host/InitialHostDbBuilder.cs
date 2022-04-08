using KonbiCloud.EntityFrameworkCore;

namespace KonbiCloud.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly KonbiCloudDbContext _context;

        public InitialHostDbBuilder(KonbiCloudDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}
