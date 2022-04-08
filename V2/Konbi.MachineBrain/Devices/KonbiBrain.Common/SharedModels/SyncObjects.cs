using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.SharedModels
{
    public class Field
    {
        public string From { get; set; }
        public string To { get; set; }
    }
    public class Row : List<Data>
    {

    }
    public class Data
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}
