using System;
using System.Collections.Generic;

namespace KonbiCloud.Inventories
{
    public class SyncUnloadDto
    {
		public SyncUnloadDto()
		{
			Ids = new List<Guid>();
		}

		public List<Guid> Ids { get; set; }

		public string RestockerName { get; set; }

		public Guid MachineId { get; set; }

		public UnloadTopupDto NewTopup { get; set; }
	}

	public class UnloadTopupDto
	{
		public Guid Id { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public bool IsProcessing { get; set; }

		public int Total { get; set; }
	}
}
