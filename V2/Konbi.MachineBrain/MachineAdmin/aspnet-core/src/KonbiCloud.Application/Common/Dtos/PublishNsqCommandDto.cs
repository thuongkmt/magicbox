using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common.Dtos
{
    public class PublishNsqCommandDto
    {
        public string Topic { get; set; }
        public object CommandObj { get; set; }
    }
}
