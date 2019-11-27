using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 网络请求处理模块
    /// </summary>
    class WebConnectActor
    {
        /// <summary>
        /// 通过post方式发送数据
        /// </summary>
        /// <param name="postString"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string postData(string postString, string url)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            //string postString = "";
            byte[] postData = Encoding.Default.GetBytes(postString);
            byte[] responseData = client.UploadData(url, "POST", postData);
            string srcString = Encoding.UTF8.GetString(responseData);
            return srcString;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Url">url</param>
        /// <param name="postDataStr">GET数据</param>
        /// <param name="cookie">GET容器</param>
        /// <param name="ispost">是否是POST方式</param>
        /// <returns></returns>
        public static string getDataWithCookie(string url, string param, string cookieStr, Encoding charset, bool ispost)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //if (cookie.Count == 0)
            //{
            //    request.CookieContainer = new CookieContainer();
            //    cookie = request.CookieContainer;
            //}
            //else
            //{
            //    request.CookieContainer = cookie;
            //}
            if (ispost) request.Method = "POST";
            else request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.Headers.Add("Cookie", cookieStr);

            if (ispost == true && param.Length > 0)
            {
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = charset.GetBytes(param);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            //request.ContentType = "image/JPEG;";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, charset);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            catch (Exception)
            {
                return "";

            }


        }

        /// <summary>
        /// 检查ssh验证结果是否接受的函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }

        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        /// <summary>
        /// 发送https请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="charset"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HttpWebResponse CreatePostHttpResponse(string url, Encoding charset, IDictionary<string, string> parameters = null)
        {
            HttpWebRequest request = null;
            //HTTPSQ请求  
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = DefaultUserAgent;
            //如果需要POST数据     
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = charset.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }


        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }

        public static string getResultFromLtp(string words)
        {
            string url = "http://api.ltp-cloud.com/analysis/?api_key=p6I9Q5O05HxSYyrKVXscqZanlBId5cENRW7FQJIb&text=" + UrlEncode(words) + "&pattern=dp&format=json";
            string res = getData(url, Encoding.UTF8);
            return res;
        }



        public static string getSecurityData(string url, string appcode, bool isPost = false)
        {
            String bodys = "";
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            httpRequest.Method = isPost?"POST":"GET";
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));

            return reader.ReadToEnd();
        }


        /// <summary>
        /// 用httpWebRequest方式访问一个url，获取页面内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string getData(string url, Encoding encoding = null)
        {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (encoding == null) encoding = Encoding.Default;
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), encoding);
                string responseContent = streamReader.ReadToEnd();
                httpWebResponse.Close();
                streamReader.Close();
                return responseContent;
            }
            catch (Exception)
            {
                return "";
            }

        }


    }
}
