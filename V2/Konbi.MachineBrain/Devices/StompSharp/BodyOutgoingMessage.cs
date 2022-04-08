using StompSharp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompSharp
{
    public class BodyOutgoingMessage : IOutgoingMessage
    {
        private readonly byte[] _body;

        public byte[] Body
        {
            get { return _body; }
        }

        public IEnumerable<IHeader> Headers
        {
            get
            {
                return Enumerable.Empty<IHeader>();
            }
        }

        public BodyOutgoingMessage(byte[] body)
        {
            _body = body;
        }
    }
}
