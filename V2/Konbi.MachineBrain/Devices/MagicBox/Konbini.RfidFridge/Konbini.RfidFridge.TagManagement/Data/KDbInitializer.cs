using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Konbini.RfidFridge.TagManagement.Data
{
    class KDbInitializer : SqliteCreateDatabaseIfNotExists<KDbContext>
    {
        public KDbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void Seed(KDbContext context)
        {
            base.Seed(context);
            //context.Transactions.Add(new Entities.Transaction() { Collected = 100 });
        }
    }
}
