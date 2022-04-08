using Abp.Domain.Entities;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Machines
{
    public class LoadoutItem : Entity<Guid>
    {
        public Machine Machine { get; set; }
        public Guid? ProductId { get; set; }
        public Products.Product Product { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int Capacity { get; set; }
        public string ItemLocation { get; set; }
        public bool IsDisable { get; set; }
    }

    public class LoadoutItemDto
    {
        public Guid Id { get; set; }
        public Guid MachineId { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int Capacity { get; set; }
        public string ItemLocation { get; set; }
        public bool IsDisable { get; set; }
    }
}
