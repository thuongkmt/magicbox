using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.GetStarted.Dtos
{
    public class GetStartedDataOutput
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
        public string StepTitle { get; set; }
        public string StepSubTitle { get; set; }
        public string StepActionUrl { get; set; }
        public int StepDoneFlg { get; set; }
    }
}
