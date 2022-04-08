using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class TagIdDto
    {
        public TagIdDto()
        {

        }
        public TagIdDto(string id, int trayLevel)
        {
            this.TrayLevel = trayLevel;
            this.Id = id;
        }
        public int TrayLevel { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }

    public class StateTagIdDto : TagIdDto
    {
        public StateTagIdDto()
        {

        }
        public StateTagIdDto(string id, int trayLevel, TagChangeEvent tagChangeEvent, ProductDto productDto)
        {
            this.TrayLevel = trayLevel;
            this.Id = id;
            State = tagChangeEvent;
            Product = productDto;
        }
        public ProductDto Product { get; set; }
        public TagChangeEvent State { get; set; }
        public override string ToString()
        {
            return Id;
        }
    }
}
