using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.SharedModels
{
    public abstract class EntityBase
    {
        //long _ts { get; set; }
        public Guid Id { get; set; }
    }
}
