using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.Interface
{
    public interface IRfidReaderInterface
    {
        bool Connect();
        void StartRecord();
        void StopRecord();
        List<string> GetTags();
        Action<List<string>> OnTagsRecord { get; set; }
    }
}
