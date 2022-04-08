using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.CustomerKonbiWallet.Dtos
{
    public class WalletTransaction
    {
        public int transaction_id { get; set; }
        public int blog_id { get; set; }
        public int user_id { get; set; }
        public string type { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; }
        public string currency { get; set; }
        public string details { get; set; }
        public string deleted { get; set; }
        public DateTime date { get; set; }
        public string user_name { get; set; }
        public string created_by_name { get; set; }
    }
}
