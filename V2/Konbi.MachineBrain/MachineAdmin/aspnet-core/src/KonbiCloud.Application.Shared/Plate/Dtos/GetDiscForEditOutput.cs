using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate.Dtos
{
    public class GetDiscForEditOutput
    {
		public CreateOrEditDiscDto Disc { get; set; }

		public string PlateName { get; set;}


    }
}