namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupDetailDto
    {
        public string ProductName { get; set; }

        public int Total { get; set; }

        public int Sold { get; set; }

        public decimal SalesAmount { get; set; }

        public TopUpInventoryTypeEnum Type { get; set; }
    }

    public enum TopUpInventoryTypeEnum
    {
        NONE = 0,
        NEW_PRODUCT = 1,
        OLD_PRODUCT = 2,
        THROW_OUT_PRODUCT = 3
    }
}
