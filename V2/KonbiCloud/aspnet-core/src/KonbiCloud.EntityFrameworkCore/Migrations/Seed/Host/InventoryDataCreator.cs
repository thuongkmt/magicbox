using Abp.Timing;
using KonbiCloud.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace KonbiCloud.Migrations.Seed.Host
{
    public class InventoryDataCreator
    {
        private readonly KonbiCloudDbContext _context;

        public InventoryDataCreator(KonbiCloudDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
           // CreateMachines();
           // CreateTopups();
           // CreateProducts();
           // CreateInventories();
           // CreateTransactions();
        }

        private void CreateMachines()
        {
            if (_context.Machines.IgnoreQueryFilters().Any()) return;

            for (var i = 1; i <= 50; i++)
            {
                _context.Machines.Add(new Machines.Machine() { Name = string.Format("Magic Box {0}", i), IsOffline = false });
            }

            _context.SaveChanges();
        }

        private void CreateTopups()
        {
            if (_context.Topups.IgnoreQueryFilters().Any()) return;

            for (var i = 1; i <= 50; i++)
            {
                var machine = _context.Machines.IgnoreQueryFilters().FirstOrDefault(m => m.Name == string.Format("Magic Box {0}", i));
                if (machine != null)
                {
                    _context.Topups.Add(new Inventories.Topup()
                    {
                        StartDate = Clock.Now.AddDays(-1),
                        EndDate = Clock.Now.AddDays(1),
                        MachineId = machine.Id,
                        Total = 200,
                        Sold = 50,
                        Error = 0
                    });
                }
            }
            _context.SaveChanges();
        }

        private void CreateProducts()
        {
            if (_context.Products.IgnoreQueryFilters().Any()) return;

            for (var i = 1; i <= 100; i++)
            {
                _context.Products.Add(new Products.Product()
                {
                    SKU = string.Format("0000-00{0}", i),
                    Name = string.Format("Crunch Ice Cream {0}", i),
                    Price = 1.1 + i,
                    Desc = string.Format("this description for Crunch ice cream {0}", i),
                });
            }
            _context.SaveChanges();
        }

        private void CreateInventories()
        {
            if (_context.Inventories.IgnoreQueryFilters().Any()) return;

            for (var i = 1; i <= 50; i++)
            {
                var machine = _context.Machines.IgnoreQueryFilters().FirstOrDefault(m => m.Name == string.Format("Magic Box {0}", i));

                if (machine != null)
                {
                    var topup = _context.Topups.IgnoreQueryFilters().FirstOrDefault(t => t.MachineId == machine.Id);
                    var prod = _context.Products.IgnoreQueryFilters().FirstOrDefault(p => p.Name == string.Format("Crunch Ice Cream {0}", i));

                    if (topup != null && prod != null)
                    {
                        _context.Inventories.Add(new Inventories.InventoryItem()
                        {
                            TagId = "Tag",
                            ProductId = prod.Id,
                            TopupId = topup.Id,
                            MachineId = machine.Id,
                            Price = 5
                        });
                    }
                }
            }
            _context.SaveChanges();
        }

        private void CreateTransactions()
        {
            //if (_context.ProductTransactions.IgnoreQueryFilters().Any()) return;

            //var prodList = _context.Products.IgnoreQueryFilters().ToList();
            //if(prodList != null)
            //{
            //    foreach(var prod in prodList)
            //    {
            //        _context.ProductTransactions.Add(new Transactions.ProductTransaction() { Product = prod, Amount =(decimal)1.5 });
            //    }
            //    _context.SaveChanges();
            //}

            //var inventoris = _context.Inventories.IgnoreQueryFilters().ToList();
        }

    }
}
