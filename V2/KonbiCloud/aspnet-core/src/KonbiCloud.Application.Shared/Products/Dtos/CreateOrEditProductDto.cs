using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KonbiCloud.Products.Dtos
{
    public class CreateOrEditProductDto : EntityDto<Guid?>
    {
        public CreateOrEditProductDto()
        {
            CategoryIds = new List<Guid>();
        }

        [Required]
        public string Name { get; set; }

        [Required]
		public string SKU { get; set; }
		
		public string Barcode { get; set; }

        public string ShortDesc { get; set; }

        public string Desc { get; set; }

        public string Tag { get; set; }

        public string ImageUrl { get; set; }
        public string TagPrefix { get; set; }

        public string CategoryNames { get; set; }

        public List<Guid> CategoryIds { get; set; }

        public double? Price { get; set; }
    }
}