using Konbini.RfidFridge.TagManagement.Entities;
using Konbini.RfidFridge.TagManagement.Interface;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace Konbini.RfidFridge.TagManagement.Data
{
    public class KDbContext : DbContext, IKDbContext
    {
        public KDbContext() : base("KDbContext")
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>();
            var initializer = new KDbInitializer(modelBuilder);
            Database.SetInitializer(initializer);
        }

        public IDbSet<Settings> Settings { get; set; }

        public override int SaveChanges()
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            string currentUserName = string.Empty;

            DateTime currentTime = DateTime.Now;

            foreach (var entry in modifiedEntries)
            {
                IAuditableEntity entity = entry.Entity as IAuditableEntity;
                if (entity != null)
                {
                    if (entry.State == System.Data.Entity.EntityState.Added)
                    {
                        entity.CreatedBy = currentUserName;
                        entity.CreatedDate = currentTime;
                    }
                    else
                    {
                        base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                        base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                    }

                    entity.UpdatedBy = currentUserName;
                    entity.UpdatedDate = currentTime;
                }
            }

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
    }
}