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
    }
}
