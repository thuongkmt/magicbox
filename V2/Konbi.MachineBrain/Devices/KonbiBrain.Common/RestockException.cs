using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Exceptions
{
    public class RestockException: System.Exception
    {
        public RestockException() : base() { }

        public RestockException(string message) : base(message)
        {

        }

        public RestockException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
