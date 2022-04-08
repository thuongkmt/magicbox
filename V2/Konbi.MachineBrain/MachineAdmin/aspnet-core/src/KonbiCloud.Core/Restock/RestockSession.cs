using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace KonbiCloud.Restock
{
	[Table("RestockSessions")]
    public class RestockSession : Entity 
    {

		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime EndDate { get; set; }
		
		public virtual int Total { get; set; }
		
		public virtual int LeftOver { get; set; }
		
		public virtual int Sold { get; set; }
		
		public virtual int Error { get; set; }
		
		public virtual bool IsProcessing { get; set; }
		
		public virtual string RestockerName { get; set; }
		
		public virtual int Restocked { get; set; }
		
		public virtual int Unloaded { get; set; }
		

    }
}