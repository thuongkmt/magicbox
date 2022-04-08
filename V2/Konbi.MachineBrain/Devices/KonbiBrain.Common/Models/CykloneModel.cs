using System.Collections.Generic;
using KonbiBrain.Common.Enums;

namespace KonbiBrain.Common.Models
{

    public class WebApiTransCompletionResult
    {
        public int? transactionId { get; set; }
        public int quantum { get; set; }
        public int collected { get; set; }
        public int balance { get; set; }
        public bool isCancelled { get; set; }
    }

    public class WebApiCashlessTransCompletionResult : WebApiTransCompletionResult
    {
        public object cashlessInfomation { get; set; }
    }

    public class WebApiDepositResult
    {
        public int id { get; set; }
        public int? transactionId { get; set; }
        public int target { get; set; }
        public int collected { get; set; }
        public int denomination { get; set; }
    }

    public class WebApiDispenseResult
    {
        public int id { get; set; }
        public int? transactionId { get; set; }
        public BankType banktype { get; set; }
        public int denomination { get; set; }
        public int dispensed { get; set; }
    }

    //public class WebApiIucResponseResult
    //{
    //    public int id { get; set; }
    //    public int? transactionId { get; set; }
    //    public IucResponseType type { get; set; }
    //    public string code { get; set; }
    //    public dynamic message { get; set; }
    //}

    public class WebApiBoolResult
    {
        public WebApiBoolResult(bool value)
        {
            result = value;
        }

        public bool result { get; set; }
    }

    public class WebApiResponseResult
    {
        public bool success = false;
        public List<ResponseError> errors = new List<ResponseError>();
        public Dictionary<string, object> successProps = new Dictionary<string, object>();

        public WebApiResponseResult(bool success)
        {
            this.success = success;
        }

        public void addSuccessProp(string key, object o = null)
        {
            successProps.Add(key, o);
        }

        public void addError(string error, string errorDescription = "")
        {
            errors.Add(new ResponseError(error, errorDescription));
        }

        public class ResponseError
        {
            public string error;
            public string errorDescription;

            public ResponseError(string error, string errorDescription)
            {
                this.error = error;
                this.errorDescription = errorDescription;
            }
        }
    }
}
