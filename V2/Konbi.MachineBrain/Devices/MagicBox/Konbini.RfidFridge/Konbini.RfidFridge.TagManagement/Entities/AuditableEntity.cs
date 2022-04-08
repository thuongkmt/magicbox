using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Konbini.RfidFridge.TagManagement.Interface;

namespace Konbini.RfidFridge.TagManagement.Entities
{
    public abstract class AuditableEntity<T> : IAuditableEntity
    {
        public virtual T Id { get; set; }
        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; }

        [MaxLength(256)]
        [ScaffoldColumn(false)]
        public string CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        public DateTime UpdatedDate { get; set; }

        [MaxLength(256)]
        [ScaffoldColumn(false)]
        public string UpdatedBy { get; set; }

        [ScaffoldColumn(false)]
        public bool IsDeleted { get; set; }
    }
}
