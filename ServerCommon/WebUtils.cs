using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using com.ideadynamo.foundation.threads;

namespace ServerCommon
{
    public delegate void Action2(string p1, string p2);

    public class WebAsyncRequestParams
    {
        public HttpWebRequest Request = null;
        public HttpListenerContext ClientContext = null;
    }

    public static class WebUtils
    {
        static MD5 _md5 = new MD5CryptoServiceProvider();
        static SHA256Managed _sha256 = new SHA256Managed();

        public static string ToBase64(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(buffer);
        }

        public static string FromBase64(string input)
        {
            byte[] buffer = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(buffer);
        }

        public static void HttpPost(string url, 
                                    byte[] parameters,
                                    Action<WebResponse, HttpListenerContext> callback, 
                                    Action<string, string> logcallback, 
                                    HttpListenerContext clientContext)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);   
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = parameters.Length;

            try
            {
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(parameters, 0, parameters.Length);
                }
            }
            catch (Exception err)
            {
                logcallback(err.Message, "Post request to server failed!");
            }

            try
            {
                using (WebResponse wr = req.GetResponse())
                {
                    callback(wr, clientContext);
                }
            }
            catch (Exception err)
            {
                logcallback(err.Message, "Post response from server failed");
            }
        }

        //public static void HttpPos( string url,
        //                            byte[] parameters)
        //{
        //    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
        //    req.Method = "POST";
        //    req.ContentType = "application/x-www-form-urlencoded";
        //    req.ContentLength = parameters.Length;

        //    try
        //    {
        //        using (Stream reqStream = req.GetRequestStream())
        //        {
        //            reqStream.Write(parameters, 0, parameters.Length);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        logcallback(err.Message, "Post request to server failed!");
        //    }

        //    try
        //    {
        //        using (WebResponse wr = req.GetResponse())
        //        {
        //            callback(wr, clientContext);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        logcallback(err.Message, "Post response from server failed");
        //    }
        //}

        public static void HttpPostAsync(string url, byte[] parameters, AsyncCallback callback, Action2 logcallback, HttpListenerContext httpClientContext)
        {
            WebAsyncRequestParams asyncParams = new WebAsyncRequestParams();
            asyncParams.Request = (HttpWebRequest)HttpWebRequest.Create(url);
            asyncParams.ClientContext = httpClientContext;
            asyncParams.Request.Method = "POST";
            asyncParams.Request.ContentType = "application/x-www-form-urlencoded";
            asyncParams.Request.ContentLength = parameters.Length;
            try
            {
                using (Stream reqStream = asyncParams.Request.GetRequestStream())
                {
                    reqStream.Write(parameters, 0, parameters.Length);
                }
            }
            catch (Exception err)
            {
                logcallback(err.Message, "Send request to server failed!");
            }

            try
            {
                asyncParams.Request.BeginGetResponse(callback, asyncParams.Request);
            }
            catch (Exception err)
            {
                logcallback(err.Message, "Get response from server failed");
            }
        }

        //public static void HttpGetAsync(string url, AsyncCallback callBack)
        //{
        //    WebAsyncRequestParams asyncParams = new WebAsyncRequestParams();
        //    asyncParams.Request = (HttpWebRequest)HttpWebRequest.Create(url);
        //    asyncParams.ClientContext = httpClientContext;
        //    asyncParams.Request.Method = "GET";
        //    asyncParams.Request.ContentType = "application/x-www-form-urlencoded";
        //    asyncParams.Request.ContentLength = parameters.Length;
        //    try
        //    {
        //        using (Stream reqStream = asyncParams.Request.GetRequestStream())
        //        {
        //            reqStream.Write(parameters, 0, parameters.Length);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        logcallback(err.Message, "Send request to server failed!");
        //    }

        //    try
        //    {
        //        asyncParams.Request.BeginGetResponse(callback, asyncParams.Request);
        //    }
        //    catch (Exception err)
        //    {
        //        logcallback(err.Message, "Get response from server failed");
        //    }
        //}

        public static void HttpGet4Login( string url,
                                    Action<WebResponse, HttpListenerContext, string[]> callback,
                                    Action<string, string> logcallback,
                                    HttpListenerContext clientContext,
                                    string[] accessToken)
        {
            HttpWebRequest req = null;
            if(url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                req = HttpWebRequest.Create(url) as HttpWebRequest;
                req.ProtocolVersion = HttpVersion.Version10;
            }
            else
                req = HttpWebRequest.Create(url) as HttpWebRequest;
            req.Method = "Get";
            try
            {
                using (WebResponse wr = req.GetResponse())
                {
                    callback(wr, clientContext, accessToken);
                }
            }
            catch (Exception err)
            {
                logcallback(err.Message, "Get response from server failed");
            }
        }

        public static void BuildParamList(StringBuilder sb, string name, string value, bool first)
        {
            if (!first)
            {
                sb.Append("&");
            }

            sb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(name, Encoding.UTF8), HttpUtility.UrlEncode(value, Encoding.UTF8));

        }

        public static string GetMD5(string input)
        {
            byte[] s = _md5.ComputeHash(UnicodeEncoding.UTF8.GetBytes(input));
            string md5raw = BitConverter.ToString(s).ToLower();
            md5raw = md5raw.Replace("-", "");
            return md5raw;
        }

        public static string GetHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = _sha256.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            foreach (var x in hash)
            {
                result.AppendFormat("{0:x2}", x);
            }
            return result.ToString();
        }

        public static String GetSignData(Dictionary<String, String> parameters)
        {
            StringBuilder content = new StringBuilder();
            List<String> keys = new List<String>(parameters.Keys);

            keys.Sort();
            for (int i = 0; i < keys.Count; i++)
            {
                String key = keys[i];
                if ("sign" == key)
                {
                    continue;
                }

                String value = parameters[key];
                if (value != null)
                {
                    content.AppendFormat("{0}{1}={2}", (i == 0 ? "" : "&"), key, value);
                }
                else
                {
                    content.AppendFormat("{0}{1}=", (i == 0 ? "" : "&"), key);
                }
            }
            return content.ToString();
        }

        private static StringBuilder _paramStrBuild = new StringBuilder();
    }
}
