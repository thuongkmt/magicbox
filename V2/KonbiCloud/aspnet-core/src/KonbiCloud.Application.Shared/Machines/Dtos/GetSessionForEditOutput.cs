using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Machines.Dtos
{
    public class GetSessionForEditOutput
    {
		public CreateOrEditSessionDto Session { get; set; }


    }
}