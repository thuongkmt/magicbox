using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konbi.Common.Interfaces;
using Newtonsoft.Json;

namespace KonbiBrain.Common.Services
{
    public class FuncCallLogInterceptor: IInterceptor
    {
        public IKonbiBrainLogService LogService { get; set; }
        public void Intercept(IInvocation invocation)
        {
            try
            {
                LogService.LogInfo(string.Format("Calling method {0} with parameters {1}... ",
                    invocation.Method.Name,
                    string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())));

                invocation.Proceed();

                LogService.LogInfo(string.Format("Done: result was {0}.", JsonConvert.SerializeObject(invocation.ReturnValue)));
            }
            catch (Exception e)
            {
                LogService.LogInfo(e.Message);
            }
            
        }
    }
}
