using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.MultiTenancy;
using KonbiCloud.Editions;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Prices;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Migrations.Seed.Prices
{
    public class ContractorPriceStrategyBuilder
    {
        private readonly KonbiCloudDbContext _context;

        public ContractorPriceStrategyBuilder(KonbiCloudDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateContractorPrice();
        }

        private void CreateContractorPrice()
        {
            //Default tenant
            var priceStrategyCode = _context.PriceStrategyCodes.IgnoreQueryFilters().FirstOrDefault(t => t.Code == "Contractor");
            if (priceStrategyCode == null)
            {
                priceStrategyCode = new PriceStrategyCode() {Code = "Contractor"};
                _context.PriceStrategyCodes.Add(priceStrategyCode);
                _context.SaveChanges();
            }
        }
    }
}
