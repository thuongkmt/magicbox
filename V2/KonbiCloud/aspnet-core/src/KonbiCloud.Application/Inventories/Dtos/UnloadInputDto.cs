using System;
using System.Collections.Generic;

namespace KonbiCloud.Inventories
{
	public class UnloadInputDto
	{
		public UnloadInputDto()
		{
			Ids = new List<Guid>();
		}

		public List<Guid> Ids { get; set; }
		public string RestockerName { get; set; }
		public Guid MachineId { get; set; }
	}
}
