using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Konbini.RfidFridge.Web.Admin.Controllers
{
    using Konbini.RfidFridge.Data.Core;
    using Konbini.RfidFridge.Domain.Entities;

    using Microsoft.EntityFrameworkCore;

    //using Konbini.RfidFridge.Service.Data;

    [Route("api/[controller]")]
    public class InventoryController : Controller
    {
     

        [HttpPost("[action]")]
        public bool Insert([FromBody] List<Inventory> itemsInventories)
        {
            //var a = item.Blah;
            using (var db = new RfidFridgeDataContext())
            {
                foreach (var item in itemsInventories)
                {
                    item.CreatedDate = DateTime.Now;
                    item.UpdatedDate = DateTime.Now;
                    db.Inventories.Add(item);
                }
                db.SaveChanges();
            }
            return true;
        }


        [HttpGet("[action]")]
        public IEnumerable<Inventory> GetAll()
        {
            using (var db = new RfidFridgeDataContext())
            {
                var p = db.Inventories.Include("Product").ToList().OrderByDescending(x => x.Id);
                return p;
            }
        }


        [HttpPost("[action]")]
        public void Delete([FromBody]int id)
        {
            using (var db = new RfidFridgeDataContext())
            {
                var item = db.Inventories.FirstOrDefault(x => x.Id == id);

                db.Attach(item);
                db.Entry(item).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        [HttpGet("[action]")]
        public string Ping()
        {
            return "pong";
        }
    }

    public class Test
    {
        public string Blah { get; set; }
    }
}
