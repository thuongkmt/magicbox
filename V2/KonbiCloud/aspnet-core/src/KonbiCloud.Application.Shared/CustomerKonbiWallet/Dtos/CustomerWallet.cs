using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using WooCommerceNET.WooCommerce.v2;

namespace KonbiCloud.CustomerKonbiWallet.Dtos
{
    public class CustomerWallet
    {
        [DataMember(EmitDefaultValue = false)]
        public string customer { get { return $"{first_name} {last_name}"; } }
        public static string Endpoint { get; }
        public decimal? total_spent { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? orders_count { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool? is_paying_customer { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public CustomerShipping shipping { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public CustomerBilling billing { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string password { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string username { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string avatar_url { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string role { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string first_name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string email { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_modified_gmt { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_modified { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_created_gmt { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? date_created { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long? id { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string last_name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<CustomerMeta> meta_data { get; set; }
        [DataMember(EmitDefaultValue = false, Name = "total_spent")]
        protected object total_spentValue { get; set; }
        public decimal? total_topup { get; set; }
        public decimal? wallet_balance { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? sign_up { get { return date_created; } }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? last_active { get; set; }
    }
}
