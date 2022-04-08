using Konbini.RfidFridge.TagManagement.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.Entities
{
    public class Settings : AuditableEntity<long>
    {
        public SettingKey Key { get; set; }
        public string Value { get; set; }
    }

}
