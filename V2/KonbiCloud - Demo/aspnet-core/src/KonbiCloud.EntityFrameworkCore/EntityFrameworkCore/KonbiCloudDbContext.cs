using KonbiCloud.Plate;
using Abp.IdentityServer4;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;
using KonbiCloud.BlackVMCDiagnostic;
using KonbiCloud.Chat;
using KonbiCloud.Diagnostic;
using KonbiCloud.Editions;
using KonbiCloud.Employees;
using KonbiCloud.Friendships;
using KonbiCloud.Machines;
using KonbiCloud.MultiTenancy;
using KonbiCloud.MultiTenancy.Accounting;
using KonbiCloud.MultiTenancy.Payments;
using KonbiCloud.Products;
using KonbiCloud.Settings;
using KonbiCloud.Storage;
using KonbiCloud.MenuSchedule;
using KonbiCloud.Prices;
using KonbiCloud.SignalR;
using KonbiCloud.Transactions;

namespace KonbiCloud.EntityFrameworkCore
{
    public class KonbiCloudDbContext : AbpZeroDbContext<Tenant, Role, User, KonbiCloudDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<Session> Sessions { get; set; }

       
        public virtual DbSet<Disc> Discs { get; set; }

        public virtual DbSet<Plate.Plate> Plates { get; set; }

        public virtual DbSet<PlateCategory> PlateCategories { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<GeneralMessage> GeneralMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
      
        public DbSet<LoadoutItem> LoadoutItems { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<MachineError> MachineErrors { get; set; }
        public DbSet<VendingHistory> VendingHistories { get; set; }

        public DbSet<AlertConfiguration> AlertConfigurations { get; set; }
        public DbSet<VendingStatus> VendingStatues { get; set; }
        public DbSet<MachineErrorSolution> MachineErrorSolutions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<VMCDiagnostic> BlackVMCDiagnostic { get; set; }
        public DbSet<MachineStatus> MachineStatus { get; set; }
        public DbSet<HardwareDiagnostic> HardwareDiagnostics { get; set; }
        public DbSet<HardwareDiagnosticDetail> HardwareDiagnosticDetails { get; set; }
        public DbSet<PlateMenu> PlateMenus { get; set; }
        public DbSet<PriceStrategyCode> PriceStrategyCodes { get; set; }
        public DbSet<PriceStrategy> PriceStrategies { get; set; }
        public DbSet<DetailTransaction> Transactions { get; set; }
        public DbSet<Tray> Trays { get; set; }
        public DbSet<PlateMachineSyncStatus> PlateMachineSyncStatus { get; set; }
        public DbSet<PlateMenuMachineSyncStatus> PlateMenuMachineSyncStatus { get; set; }
        public DbSet<DishMachineSyncStatus> DishMachineSyncStatus { get; set; }

        public KonbiCloudDbContext(DbContextOptions<KonbiCloudDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
           
           
           
            modelBuilder.Entity<Session>(S =>
            {
                S.HasIndex(e => new { e.TenantId });
            });
 modelBuilder.Entity<Disc>(D =>
            {
                D.HasIndex(e => new { e.TenantId });
            });
 modelBuilder.Entity<Plate.Plate>(P =>
            {
                P.HasIndex(e => new { e.TenantId });
            });
 modelBuilder.Entity<PlateCategory>(P =>
            {
                P.HasIndex(e => new { e.TenantId });
            });
 modelBuilder.Entity<BinaryObject>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { e.PaymentId, e.Gateway });
            });

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}
