using Konbini.RfidFridge.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Util
{
    public class GrabPayService
    {
        public const string HTTP_GET = "GET";

        /// <summary>
        /// Parse URL.
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="type">type</param>
        /// <returns></returns>
        public string ParseURL(string url, string type)
        {
            string regexPattern = @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
                                  + @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
                                  + @"(?<q1>\?(?<q0>[^#]*))?"
                                  + @"(?<f1>#(?<f0>.*))?";
            Regex re = new Regex(regexPattern, RegexOptions.ExplicitCapture);
            Match m = re.Match(url);

            return m.Groups[type].Value;
        }

        /// <summary>
        /// Get Path.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetPath(string url)
        {
            return ParseURL(url, "p0");
        }

        /// <summary>
        /// Get Query String.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetQueryString(string url)
        {
            return ParseURL(url, "q1");
        }

        /// <summary>
        /// Generate HMAC Signature.
        /// </summary>
        /// <param name="partnerID"></param>
        /// <param name="partnerSecret"></param>
        /// <param name="httpMethod"></param>
        /// <param name="requestURL"></param>
        /// <param name="contentType"></param>
        /// <param name="requestBody"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public string GenerateHMACSignature(string partnerID, string partnerSecret, string httpMethod, string requestURL, string contentType, string requestBody, string timestamp)
        {
            var hashedPayload = "";
            var requestPath = GetPath(requestURL);
            var queryString = GetQueryString(requestURL);

            if (httpMethod == HTTP_GET || String.IsNullOrEmpty(requestBody))
            {
                hashedPayload = "";
            }
            else
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
                    hashedPayload = Convert.ToBase64String(bytes);
                }
            }

            var requestData = httpMethod + "\n"
                + contentType + "\n"
                + timestamp + "\n"
                + requestPath + queryString + "\n"
                + hashedPayload + "\n";
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(partnerSecret));
            var hmacDigest = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(requestData)));
            var authHeader = partnerID + ':' + hmacDigest;

            return authHeader;
        }

        /// <summary>
        /// Random String.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string RandomString(int length)
        {
            Random random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generate MsgID.
        /// </summary>
        /// <returns></returns>
        public string GenerateMsgID()
        {
            return RandomString(32);
        }

        /// <summary>
        /// Generate PartnerTxID.
        /// </summary>
        /// <returns></returns>
        public string GeneratePartnerTxID()
        {
            return "partner-" + RandomString(24);
        }
    }
}
