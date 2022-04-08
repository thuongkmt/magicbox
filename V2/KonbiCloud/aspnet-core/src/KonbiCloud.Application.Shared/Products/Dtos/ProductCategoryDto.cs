
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class ProductCategoryDto : EntityDto<Guid>
    {
		public string Name { get; set; }

        public string Code { get; set; }

        public string Desc { get; set; }

        public DateTime CreationTime { get; set; }

    }
}