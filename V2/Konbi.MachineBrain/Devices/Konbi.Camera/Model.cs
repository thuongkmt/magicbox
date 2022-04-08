using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.Camera
{
    public class ComboBoxItem
    {
        public ComboBoxItem(WebCameraId id)
        {
            _id = id;
        }

        private readonly WebCameraId _id;
        public WebCameraId Id
        {
            get { return _id; }
        }

        public override string ToString()
        {
            // Generates the text shown in the combo box.
            return _id.Name;
        }
    }

    public class CompressItem
    {
        public string Name { get; set; }
        public Int64 Value { get; set; }
    }
}
