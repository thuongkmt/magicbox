//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Text;
//using Abp.Domain.Entities;
//using KonbiCloud.Products;
//using Newtonsoft.Json;

//namespace KonbiCloud.Machines
//{
//    public class ItemInventory : Entity<Guid>, IMustHaveTenant
//    {
//        public int LocationCode { get; set; }
//        public virtual Product Product { get; set; }
//        [ForeignKey("Product")]
//        public Guid? ProductId { get; set; }
//        public decimal Price { get; set; }
//        public int Quantity { get; set; }
//        public int CurrentQuantity { get; set; }
//        //public bool IsBlank { get; set; }
//        [JsonIgnore]
//        public virtual PlannedSessionInventory PlannedSessionInventory { get; set; }
//        public int TenantId { get; set; }
//        [JsonIgnore]
//        public virtual Machine Machine { get; set; }

//        //ProductName: string = '';
//        //ImageLocation: string = '';

//        public string ProductName { get
//            {
//                return Product != null ? Product.Name : "";
//            }
//        }

//        public bool IsBlank
//        {
//            //get;set;
//            get
//            {
//                return ProductId == null ? true : false;
//            }
//            set
//            {

//            }
//        }

//        public string ImageLocation
//        {
//            get
//            {
//                return Product != null ? Product.ImageUrl : "";
//            }
//        }
//    }
//}
