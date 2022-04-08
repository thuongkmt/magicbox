namespace KonbiCloud.Inventories.Dtos
{
    using KonbiCloud.Products.Dtos;

    public class GetInventoryForViewDto
    {
        public InventoryDto Inventory { get; set; }

        public string ProductName { get; set; }

        public bool IsSold { get; set; }
    }

    public class GetInventoryForWebApiDto
    {
        public InventoryDto Inventory { get; set; }

        public ProductDto Product { get; set; }
    }
}