using System;
using System.Collections.Generic;

namespace KonbiCloud.PlateMenus.Dtos
{
    public class ImportResult
    {
        public string ErrorList { get; set; }
        public int ErrorCount { get; set; }
        public int SuccessCount { get; set; }
    }

    public class ImportData
    {
        public string PlateCode { get; set; }
        public string Price { get; set; }
        public string SelectedDate { get; set; }
        public string SessionName { get; set; }
    }

    public class ReplicateInput
    {
        public DateTime DateFilter { get; set; }
        public string SessionId { get; set; }
        public int Days { get; set; }
    }
}