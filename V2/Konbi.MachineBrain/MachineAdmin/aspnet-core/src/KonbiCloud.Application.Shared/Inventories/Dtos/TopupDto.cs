using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class TopupDto
    {
        public int? TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Total { get; set; }
        public int LeftOver { get; set; }
        public int Sold { get; set; }
        public int Error { get; set; }
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Is this entity Deleted?
        /// </summary>
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        public virtual long? DeleterUserId { get; set; }

        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        public virtual DateTime? DeletionTime { get; set; }

        /// <summary>
        /// Last modification date of this entity.
        /// </summary>
        public virtual DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// Last modifier user of this entity.
        /// </summary>
        public virtual long? LastModifierUserId { get; set; }

        /// <summary>
        /// Creation time of this entity.
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// Creator of this entity.
        /// </summary>
        public virtual long? CreatorUserId { get; set; }

        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        public virtual Guid Id { get; set; }
    }
    public class TagsInput
    {
        public Guid MachineId { get; set; }
        public IList<string> Tags { get; set; }
    }

    public class ProductInfo
    {
        public string Tag { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
    }
}
