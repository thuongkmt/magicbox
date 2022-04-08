using Konbini.RfidFridge.Domain.Base;

namespace Konbini.RfidFridge.Domain.Entities
{
    public class Machine : AuditableEntity<long>
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string Hotline { get; set; }

        public string NeaLicence { get; set; }

        public string CustomLine1 { get; set; }
        public string CustomLine2 { get; set; }
        public string CustomLine3 { get; set; }
        public string CustomLine4 { get; set; }
        public string CustomLine5 { get; set; }

    }
}
