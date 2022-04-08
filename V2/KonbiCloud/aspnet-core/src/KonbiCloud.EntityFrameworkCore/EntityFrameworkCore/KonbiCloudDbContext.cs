using KonbiCloud.Inventories;
using Abp.IdentityServer4;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;
using KonbiCloud.Chat;
using KonbiCloud.Diagnostic;
using KonbiCloud.Editions;
using KonbiCloud.Friendships;
using KonbiCloud.Machines;
using KonbiCloud.MultiTenancy;
using KonbiCloud.MultiTenancy.Accounting;
using KonbiCloud.MultiTenancy.Payments;
using KonbiCloud.Products;
using KonbiCloud.Settings;
using KonbiCloud.Storage;
using KonbiCloud.SignalR;
using KonbiCloud.Transactions;
using KonbiCloud.Resources;
using KonbiCloud.TemperatureLogs;
using KonbiCloud.Restock;

namespace KonbiCloud.EntityFrameworkCore
{
    public class KonbiCloudDbContext : AbpZeroDbContext<Tenant, Role, User, KonbiCloudDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }

        public virtual DbSet<RestockSession> RestockSessions { get; set; }

        public virtual DbSet<ProductMachinePrice> ProductMachinePrices { get; set; }

        public virtual DbSet<TemperatureLog> TemperatureLogs { get; set; }

        public virtual DbSet<ProductTransaction> ProductTransactions { get; set; }

        public virtual DbSet<Topup> Topups { get; set; }

        public virtual DbSet<InventoryItem> Inventories { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductCategory> ProductCategories { get; set; }

        public virtual DbSet<ProductCategoryRelation> ProductCategoryRelations { get; set; }

        public virtual DbSet<Session> Sessions { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<GeneralMessage> GeneralMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<Resource> Resources { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        //public DbSet<LoadoutItem> LoadoutItems { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<MachineError> MachineErrors { get; set; }
        public DbSet<VendingHistory> VendingHistories { get; set; }
        public DbSet<AlertConfiguration> AlertConfigurations { get; set; }
        public DbSet<VendingStatus> VendingStatues { get; set; }
        public DbSet<MachineErrorSolution> MachineErrorSolutions { get; set; }
        public DbSet<HardwareDiagnostic> HardwareDiagnostics { get; set; }
        public DbSet<HardwareDiagnosticDetail> HardwareDiagnosticDetails { get; set; }
        public DbSet<DetailTransaction> Transactions { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<UserMachine> UserMachines { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<LoadoutItem> LoadoutItems { get; set; }
        public DbSet<RestockSessionHistory> RestockSessionHistory { get; set; }

        public DbSet<CashlessDetail> CashlessDetails { get; set; }

        public DbSet<TopupHistory> TopupInventory { get; set; }
        public DbSet<CurrentInventory> CurrentInventory { get; set; }

        public KonbiCloudDbContext(DbContextOptions<KonbiCloudDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionDetail>(t =>
            {
                t.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CurrentInventory>(i =>
                       {
                           i.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<RestockSession>(r =>
            {
                r.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<InventoryItem>(i =>
                       {
                           i.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<Resource>(i =>
            {
                i.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<Product>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<ProductCategory>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<ProductTag>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<Session>(S =>
                       {
                           S.HasIndex(e => new { e.TenantId });
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

            modelBuilder.Entity<TopupHistory>()
                .HasKey(c => new { c.TopupId, c.InventoryId });

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}