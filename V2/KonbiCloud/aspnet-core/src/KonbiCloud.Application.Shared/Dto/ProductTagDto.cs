using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dto
{
    public class ProductTagsDto
    {
        //public double Price { get; set; }
        public List<KeyValuePair<string, Guid>> TagProductMaps { get; set; }
    }
}
