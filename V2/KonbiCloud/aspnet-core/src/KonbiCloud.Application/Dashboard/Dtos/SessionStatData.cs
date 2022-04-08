using System.Collections.Generic;

namespace KonbiCloud.Dashboard.Dto
{
    public class SessionStatData
    {
        public string SessionName { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TotalSale { get; set; }

        public SessionStatData(string sessionName, int totalTransaction, decimal totalSale)
        {
            SessionName = sessionName;
            TotalTransaction = totalTransaction;
            TotalSale = totalSale;
        }
    }

    public class SessionStatDataOutput
    {
        public List<SessionStatData> SessionStat { get; set; }

        public SessionStatDataOutput(List<SessionStatData> sessionStat)
        {
            SessionStat = sessionStat;
        }
    }
}