using KonbiCloud.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace KonbiCloud.Credit
{
	[Table("UserCredits")]
    public class UserCredit : FullAuditedEntity<Guid> 
    {

		public virtual decimal Value { get; set; }
		
		public virtual string Hash { get; set; }
		

		public virtual long? UserId { get; set; }
		public User User { get; set; }
		
    }
}