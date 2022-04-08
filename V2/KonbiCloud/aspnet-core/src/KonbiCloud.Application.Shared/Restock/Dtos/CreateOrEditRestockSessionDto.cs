
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Restock.Dtos
{
    public class CreateOrEditRestockSessionDto : EntityDto<int?>
    {

		public DateTime StartDate { get; set; }
		
		
		public DateTime EndDate { get; set; }
		
		
		public int Total { get; set; }
		
		
		public int LeftOver { get; set; }
		
		
		public int Sold { get; set; }
		
		
		public int Error { get; set; }
		
		
		public bool IsProcessing { get; set; }
		
		
		public string RestockerName { get; set; }
		
		
		public int Restocked { get; set; }
		
		
		public int Unloaded { get; set; }
		

    }
}