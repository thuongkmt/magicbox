using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Utils
{
    public class LocaTimeUtils
    {
        public DateTime getStartTodayLocalTime()
        {
            DateTime sgTimeNow = DateTime.Now;
            return new DateTime(sgTimeNow.Year, sgTimeNow.Month, sgTimeNow.Day, 0, 0, 0);
        }
        public string ToLocalStringTime(DateTime utcTime)
        {
            return utcTime.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
