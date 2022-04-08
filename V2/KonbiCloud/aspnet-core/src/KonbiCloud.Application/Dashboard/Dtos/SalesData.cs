using System.Collections.Generic;

namespace KonbiCloud.Dashboard.Dto
{
    public class SalesData
    {
        public string Period { get; set; }
        public decimal Sales { get; set; }
        public int Trans { get; set; }

        public SalesData(string period, decimal sales, int trans)
        {
            Period = period;
            Sales = sales;
            Trans = trans;
        }
    }

    public class SalesDataOuput
    {
        public List<SalesData> SalesSummary { get; set; }
        public SalesDataOuput(List<SalesData> salesSummary)
        {
            SalesSummary = salesSummary;
        }
    }
}