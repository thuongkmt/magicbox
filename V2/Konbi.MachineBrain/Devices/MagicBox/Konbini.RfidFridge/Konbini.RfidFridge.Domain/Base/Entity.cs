using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.Base
{
    public abstract class Entity<T>: BaseEntity, IEntity<T>
    {
        [Key]
        public virtual T Id { get; set; }

        //public Guid? RowGuid { get; set; }
    }
}
