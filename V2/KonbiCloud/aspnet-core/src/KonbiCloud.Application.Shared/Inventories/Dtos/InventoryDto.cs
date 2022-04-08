
using System;
using Abp.Application.Services.Dto;
using KonbiCloud.Enums;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryDto : EntityDto<Guid>
    {
		public string TagId { get; set; }

		public int? TrayLevel { get; set; }

		public double Price { get; set; }


		 public Guid ProductId { get; set; }
		public TagState State { get; set; }

	}
}