using System;
using System.Collections.Generic;
using System.Text;
using Abp.Timing;

namespace KonbiCloud.Utils
{
    public class LocaTimeUtils
    {
        public DateTime getStartTodayLocalTime()
        {
            DateTime sgTimeNow = Clock.Now;
            return new DateTime(sgTimeNow.Year, sgTimeNow.Month, sgTimeNow.Day, 0, 0, 0);
        }
        public string ToLocalStringTime(DateTime utcTime)
        {
            return utcTime.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
