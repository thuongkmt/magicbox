using KonbiCloud.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dto
{
    public class ChangeTagStateDto
    {
        public List<Guid> Ids { get; set; }
        public TagState State { get; set; }
    }
}
