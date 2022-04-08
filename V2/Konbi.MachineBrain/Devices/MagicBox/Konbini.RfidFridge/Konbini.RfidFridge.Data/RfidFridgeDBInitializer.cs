using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Data
{
    public class RfidFridgeDBInitializer : CreateDatabaseIfNotExists<RfidFridgeDataContext>
    {
        public override void InitializeDatabase(RfidFridgeDataContext context)
        {
            base.InitializeDatabase(context);
        }
    }
}
