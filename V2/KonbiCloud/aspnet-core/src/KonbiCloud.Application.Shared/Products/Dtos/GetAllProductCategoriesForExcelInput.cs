namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductCategoriesForExcelInput
    {
		public string Filter { get; set; }

		public string NameFilter { get; set; }

        public string CodeFilter { get; set; }

        public string DescFilter { get; set; }
    }
}