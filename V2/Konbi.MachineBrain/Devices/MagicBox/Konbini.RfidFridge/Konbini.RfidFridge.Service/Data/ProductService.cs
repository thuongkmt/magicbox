using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Konbini.RfidFridge.Data;
using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Service.Base;

namespace Konbini.RfidFridge.Service.Data
{
    public class ProductService : EntityService<Product>, IProductService
    {
      
    }
}
