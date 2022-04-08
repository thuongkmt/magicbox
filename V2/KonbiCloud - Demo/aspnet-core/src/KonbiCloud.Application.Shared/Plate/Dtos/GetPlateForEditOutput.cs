using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate.Dtos
{
    public class GetPlateForEditOutput
    {
		public CreateOrEditPlateDto Plate { get; set; }

		public string PlateCategoryName { get; set;}


    }
}