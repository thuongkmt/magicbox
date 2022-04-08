
namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductsForExcelInput
    {
        public string Filter { get; set; }

        public string SKUFilter { get; set; }

        public string NameFilter { get; set; }

        public double? MaxPriceFilter { get; set; }

        public double? MinPriceFilter { get; set; }

        public string DescFilter { get; set; }

        public string ShortDescFilter { get; set; }

        public string ImageUrlFilter { get; set; }
    }
}