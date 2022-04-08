
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KonbiCloud.Inventories.Dtos
{
	public class CreateOrEditInventoryDto : EntityDto<Guid?>
	{

		[Required]
		public string TagId { get; set; }


		public int? TrayLevel { get; set; }


		public double Price { get; set; }


		public Guid ProductId { get; set; }


	}

	public class RestockWithSessionInputDto
	{
		public RestockerInputDto Restock { get; set; }
		public UnloadByTagIdInputDto Unload { get; set; }
		//public int RestockSessionId { get; set; }
		public long StartDate { get; set; }
		public string RestockerName { get; set; }

	}

	public class RestockerInputDto
	{
		public RestockerInputDto()
		{
			Items = new List<CreateOrEditInventoryDto>();
		}

		public List<CreateOrEditInventoryDto> Items { get; set; }

		public string RestockerName { get; set; }

		public int RestockSessionId { get; set; }

	}


	public class NewRestockSessionInputDto
	{
		public string RestockerName { get; set; }
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	public class Inventory
	{
		public string tagId { get; set; }
		public int trayLevel { get; set; }
		public double price { get; set; }
		public string productId { get; set; }
		public Enums.TagState state { get; set; }
		public string id { get; set; }
	}

	public class RestockV2Dto
	{
		public RestockV2Dto()
        {
			inventory = new List<Inventory>();
		}
		public List<Inventory> inventory { get; set; }
		public DateTime? startDate { get; set; }
		public DateTime? endDate { get; set; }
		public int total { get; set; }
		public int leftOver { get; set; }
		public int sold { get; set; }
		public int error { get; set; }
		public string restockerName { get; set; }
		public string machineId { get; set; }
		public string id { get; set; }
	}
}

