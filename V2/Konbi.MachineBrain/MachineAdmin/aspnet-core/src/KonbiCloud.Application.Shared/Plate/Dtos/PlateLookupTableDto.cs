using Abp.Application.Services.Dto;

namespace KonbiCloud.Plate.Dtos
{
    public class PlateLookupTableDto
    {
		public string Id { get; set; }

		public string DisplayName { get; set; }

        public string Code { get; set; }
    }
}