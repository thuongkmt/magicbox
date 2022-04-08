namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupDetailDto
    {
        public string ProductName { get; set; }

        public int Total { get; set; }

        public int Sold { get; set; }

        public decimal SalesAmount { get; set; }
    }
}
