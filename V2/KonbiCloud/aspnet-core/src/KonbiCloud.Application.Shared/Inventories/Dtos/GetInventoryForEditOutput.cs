using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Inventories.Dtos
{
    public class GetInventoryForEditOutput
    {
		public CreateOrEditInventoryDto Inventory { get; set; }

		public string ProductName { get; set;}


    }
}