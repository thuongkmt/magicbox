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
    /// <summary>
    /// Log parameters and result of function call
    /// </summary>
    public class FuncCallLogInterceptor: IInterceptor
    {
        public IKonbiBrainLogService LogService { get; set; }
        public void Intercept(IInvocation invocation)
        {
            try
            {
                LogService.LogInfo(string.Format("Calling method {0} {2} with parameters {1}... ",
                    invocation.Method.Name,
                    string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()),invocation.TargetType.Name));

                invocation.Proceed();

                LogService.LogInfo(
                    $"Done: {invocation.Method.Name} result was {JsonConvert.SerializeObject(invocation.ReturnValue)}.");
            }
            catch (Exception e)
            {
                LogService.LogInfo(e.Message);
            }
            
        }
    }
}
