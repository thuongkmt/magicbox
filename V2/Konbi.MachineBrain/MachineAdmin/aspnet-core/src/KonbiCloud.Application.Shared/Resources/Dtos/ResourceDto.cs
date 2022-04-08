using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Resources.Dtos
{
    public class ResourceDto : EntityDto<Guid>
    {
        public string FileName { get; set; }

        public string Thumbnail { get; set; }
    }
}
