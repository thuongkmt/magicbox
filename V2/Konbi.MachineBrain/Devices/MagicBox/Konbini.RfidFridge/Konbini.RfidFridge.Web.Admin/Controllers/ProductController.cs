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

    [Route("api/[controller]")]
    public class ProductController : Controller
    {


        [HttpGet("[action]")]
        public IEnumerable<Product> GetAll()
        {
            using (var db = new RfidFridgeDataContext())
            {
                var p = db.Products.ToList().OrderByDescending(x => x.Id);
                return p;
            }
        }

        [HttpPost("[action]")]
        public bool Insert([FromBody] Product product)
        {
            using (var db = new RfidFridgeDataContext())
            {
                product.CreatedDate = DateTime.Now;
                product.UpdatedDate = DateTime.Now;
                db.Products.Add(product);

                db.SaveChanges();
            }
            return true;
        }

        [HttpPost("[action]")]
        public bool Import([FromBody] List<Product> products)
        {
            //var a = item.Blah;
            using (var db = new RfidFridgeDataContext())
            {
                foreach (var item in products)
                {
                    item.CreatedDate = DateTime.Now;
                    item.UpdatedDate = DateTime.Now;
                    db.Products.Add(item);
                }
                db.SaveChanges();
            }
            return true;
        }

        [HttpGet("[action]")]
        public Product Get(int id)
        {
            using (var db = new RfidFridgeDataContext())
            {
                return db.Products.FirstOrDefault(x => x.Id == id);
            }
        }

        [HttpPost("[action]")]
        public void Delete([FromBody]int id)
        {
            using (var db = new RfidFridgeDataContext())
            {
                 var product = db.Products.FirstOrDefault(x => x.Id == id);

                db.Attach(product);
                db.Entry(product).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        [HttpPost("[action]")]
        public void Update([FromBody]Product product)
        {
            using (var db = new RfidFridgeDataContext())
            {
                db.Attach(product);
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        [HttpGet("[action]")]
        public string Ping()
        {
            return "pong";
        }
    }
}
