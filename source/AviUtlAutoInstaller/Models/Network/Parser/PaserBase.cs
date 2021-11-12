using System;
using System.Net.Http;

namespace AviUtlAutoInstaller.Models.Network.Parser
{
    class PaserBase
    {
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// 接続先のHTMLを文字列で取得
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected string GetHtmlText(Uri uri)
        {
            string text = "";

            using (var response = _httpClient.GetAsync(uri).Result)
            {
                text = response.Content.ReadAsStringAsync().Result;
            }

            return text;
        }

        /// <summary>
        /// 接続先のREST APIのJSONを取得
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.Net.ProtocolViolationException"></exception>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        protected string GetRestAPI(Uri uri)
        {
            string json = "";

            try
            {
                var web = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                web.UserAgent = ProductInfo.AppName;
                using (var stream = web.GetResponse().GetResponseStream())
                using (var sr = new System.IO.StreamReader(stream))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch
            {
                throw;
            }

            return json;
        }
    }
}
