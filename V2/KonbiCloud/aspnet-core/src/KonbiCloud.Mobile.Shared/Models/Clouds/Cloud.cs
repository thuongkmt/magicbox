using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Models
{
    public class Cloud
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CloudUrl { get; set; }
        public string TenantName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool LoginSuccess { get; set; }
        public bool NoLogout { get; set; }
    }
}
