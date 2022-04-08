using Newtonsoft.Json;

namespace Common.TruePayment
{
    public class AccountRequest
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("tmn_account")]
        public string TmnAccount { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }
    }
    public class OtpResponse : ResponseBase<OtpResponse>
    {
        [JsonProperty("otp_ref")]
        public string OtpRef { get; set; }

        [JsonProperty("linking_agreement")]
        public string LinkAgreement { get; set; }

        [JsonProperty("agreement_id")]
        public string AgreementId { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }
    }

    public class TokenRequest
    {
        [JsonProperty("otp_ref")]
        public string OtpRef { get; set; }

        [JsonProperty("otp_code")]
        public string OtpCode { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("tmn_account")]
        public string TmnAccount { get; set; }

        [JsonProperty("agreement_id")]
        public string AgreementId => "online_merchant";

        [JsonProperty("grant_type")]
        public string GrantType => "password";

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }
    }

    public class TokenResponse : ResponseBase<TokenResponse>
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class PaymentRequest
    {
        [JsonProperty("isv_payment_ref")]
        public string Isv_payment_ref { get; set; } //Unique reference number

        [JsonProperty("merchant_id")]
        public string Merchant_id { get; set; }

        [JsonProperty("currency")]
        public string Currency => "THB";

        [JsonProperty("request_amount")]
        public string Request_amount { get; set; }

        [JsonProperty("payment_method")]
        public string Payment_method => "BALANCE"; //only: BALANCE

        [JsonProperty("payment_code")]
        public string Payment_code { get; set; } // Customer payment code (from barcode scanner)

        [JsonProperty("description")]
        public string Description { get; set; } // max 20

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata Data { get; set; }

        public class Metadata
        {

            [JsonProperty("shop_id", NullValueHandling = NullValueHandling.Ignore)]
            public string ShopId { get; set; }

            [JsonProperty("user_mobile", NullValueHandling = NullValueHandling.Ignore)]
            public string UserMobile { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }
        }
    }

    public class PaymentResponse : ResponseBase<PaymentResponse>
    {
        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }
    }

    public class PaymentCodeResponse : ResponseBase<PaymentCodeResponse>
    {
        [JsonProperty("payment_code")]
        public string PaymentCode { get; set; }
    }

    public class RefundRequest
    {
        [JsonProperty("isv_refund_ref")]
        public string IsvRefundRef { get; set; }

        [JsonProperty("request_amount")]
        public string RequestAmount { get; set; }
    }

    public class RefundResponse : ResponseBase<RefundResponse>
    {
        [JsonProperty("refund_id")]
        public string RefundId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ResponseBase<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("status")]
        public TrueMoneyResponseStatus Status { get; set; }
    }
    public class TrueMoneyResponseStatus
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
