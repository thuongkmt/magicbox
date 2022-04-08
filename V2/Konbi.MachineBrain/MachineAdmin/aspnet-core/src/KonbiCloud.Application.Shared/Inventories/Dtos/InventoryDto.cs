
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryDto : EntityDto<Guid>
    {
        public string TagId { get; set; }

        public int? TrayLevel { get; set; }

        public double Price { get; set; }

        public Guid ProductId { get; set; }
    }
}