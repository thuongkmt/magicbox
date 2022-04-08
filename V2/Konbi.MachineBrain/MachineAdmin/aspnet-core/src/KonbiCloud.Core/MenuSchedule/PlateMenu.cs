﻿using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using KonbiCloud.Products;
using System.ComponentModel.DataAnnotations.Schema;
using KonbiCloud.Machines;
using KonbiCloud.Prices;
using System.Collections.Generic;
using KonbiCloud.Sessions;

namespace KonbiCloud.MenuSchedule
{
    public class PlateMenu : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public Guid PlateId { get; set; }
        [ForeignKey("PlateId")]
        public virtual Plate.Plate Plate { get; set; }

        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public decimal? Price { get; set; }

        public ICollection<PriceStrategy> PriceStrategies { get; set; }

        public Guid SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; }

        public DateTime SelectedDate { get; set; }

        public decimal? ContractorPrice { get; set; }
    }
}
