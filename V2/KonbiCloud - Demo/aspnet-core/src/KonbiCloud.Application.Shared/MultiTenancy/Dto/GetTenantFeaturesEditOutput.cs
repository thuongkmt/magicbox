using System.Collections.Generic;
using Abp.Application.Services.Dto;
using KonbiCloud.Editions.Dto;

namespace KonbiCloud.MultiTenancy.Dto
{
    public class GetTenantFeaturesEditOutput
    {
        public List<NameValueDto> FeatureValues { get; set; }

        public List<FlatFeatureDto> Features { get; set; }
    }
}