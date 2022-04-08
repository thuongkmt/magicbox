using KonbiCloud.Credit;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace KonbiCloud.Credit
{
	[Table("CreditHistories")]
    public class CreditHistory : FullAuditedEntity<Guid> 
    {

		public virtual decimal Value { get; set; }
		
		[Required]
		public virtual string Message { get; set; }
		
		public virtual string Hash { get; set; }
		

		public virtual Guid? UserCreditId { get; set; }
		public UserCredit UserCredit { get; set; }
		
    }
}