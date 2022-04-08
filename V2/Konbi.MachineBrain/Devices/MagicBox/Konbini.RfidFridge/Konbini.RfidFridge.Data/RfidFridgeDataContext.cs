
using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Domain.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Data
{
    public class RfidFridgeDataContext : DbContext
    {
        public RfidFridgeDataContext() : base("Server=.\\SQLEXPRESS;Database=RfidFridge;Trusted_Connection=True;")
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer<RfidFridgeDataContext>(new RfidFridgeDBInitializer());
        }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
          
        }

        public DbSet<Machine> Machines { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Product> Products { get; set; }


        public override Task<int> SaveChangesAsync()
        {
            UpdateDateTime();

            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                //LogService.LogException(ex);
                return Task.FromResult(-1);
            }
        }

        public override int SaveChanges()
        {
            UpdateDateTime();

            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                //LogService.LogException(ex);
                return -1;
            }
        }

        private void UpdateDateTime()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            DateTime currentTime = DateTime.Now;

            foreach (var entry in modifiedEntries)
            {
                IAuditableEntity entity = entry.Entity as IAuditableEntity;
                if (entity != null)
                {
                    if (entry.State == System.Data.Entity.EntityState.Added)
                    {
                        entity.CreatedDate = currentTime;
                        entity.UpdatedDate = currentTime;
                    }
                    else
                    {
                        base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                        base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                    }
                    entity.UpdatedDate = currentTime;
                }
            }
        }
    }
}
