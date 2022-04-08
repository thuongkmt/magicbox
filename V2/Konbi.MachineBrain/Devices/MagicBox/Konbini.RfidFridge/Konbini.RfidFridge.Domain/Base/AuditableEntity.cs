using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Base
{
    public abstract class AuditableEntity<T> : Entity<T>, IAuditableEntity
    {
        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; }


        //[MaxLength(256)]
        [ScaffoldColumn(false)]
        public string CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? UpdatedDate { get; set; }

        //[MaxLength(256)]
        [ScaffoldColumn(false)]
        public string UpdatedBy { get; set; }

        [ScaffoldColumn(false)]
        public bool IsDeleted { get; set; }
    }
}
