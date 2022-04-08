namespace Konbini.RfidFridge.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Konbini.RfidFridge.Data.RfidFridgeDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Konbini.RfidFridge.Data.RfidFridgeDataContext";
        }

        protected override void Seed(Konbini.RfidFridge.Data.RfidFridgeDataContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
