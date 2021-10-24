using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Network.Parser
{
    class GitHub : PaserBase
    {
        private static string _gitHubHome = "https://github.com";
        private HtmlParser _parser;

        public string[] Versions { get; private set; }
        public string ProjectName { get; private set; }
        public Dictionary<string, string> VersionLink { get; private set; }

        public GitHub()
        {
            Versions = new string[0];
            VersionLink = new Dictionary<string, string>();
            _parser = new HtmlParser();
        }

        /// <summary>
        /// HTMLファイルを解析
        /// </summary>
        /// <param name="htmlText">HTMLファイルの内容</param>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        public bool Parse(string htmlText)
        {
            VersionLink.Clear();
            if (string.IsNullOrWhiteSpace(htmlText))
            {
                return false;
            }

            try
            {
                var doc = _parser.ParseDocument(htmlText);
                if (!ParseProjectName(doc) || !ParseVersionList(doc) || !ParseVersionLinkList(doc))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// URIからHTMLテキストを取得して解析
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="System.Net.ProtocolViolationException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="System.Net.WebException"/>
        public bool Parse(Uri uri)
        {
            try
            {
                var web = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                web.GetResponse().Close();
            }
            catch
            {
                throw;
            }
            string htmlText = GetHtmlText(uri);

            return Parse(htmlText);
        }

        /// <summary>
        /// プロジェクト名を取得
        /// </summary>
        /// <param name="htmlDocument"></param>
        private bool ParseProjectName(AngleSharp.Html.Dom.IHtmlDocument htmlDocument)
        {
            ProjectName = "";

            var aNodes = htmlDocument.QuerySelectorAll("a[data-pjax='#js-repo-pjax-container']");
            foreach (var aNode in aNodes)
            {
                string text = aNode.TextContent;
                if (text.Length > 0)
                {
                    ProjectName = text;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// releaseのバージョンリストを取得
        /// </summary>
        /// <param name="htmlDocument"></param>
        private bool ParseVersionList(AngleSharp.Html.Dom.IHtmlDocument htmlDocument)
        {
            List<string> verList = new List<string>();
            var ulNodes = htmlDocument.QuerySelectorAll("div[class='css-truncate css-truncate-overflow']");
            foreach (var ulNode in ulNodes)
            {
                var spanNodes = ulNode.QuerySelectorAll("span[class='ml-1 wb-break-all']");
                foreach (var spanNode in spanNodes)
                {
                    string text = spanNode.TextContent;
                    text = text.Trim().Trim(new char[] { '\n' });
                    if (text.Length > 0)
                    {
                        verList.Add(text);
                    }
                }
            }

            Versions = verList.ToArray();

            if (Versions.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// リリースリンクを取得し、バージョンと紐付け
        /// </summary>
        /// <param name="htmlDocument"></param>
        private bool ParseVersionLinkList(AngleSharp.Html.Dom.IHtmlDocument htmlDocument)
        {
            List<string> versionLinkList = new List<string>();
            var divNodes = htmlDocument.QuerySelectorAll("div[class='Box Box--condensed mt-3']");
            foreach (var divNode in divNodes)
            {
                var liNodes = divNode.QuerySelectorAll("li");
                if (liNodes.Count() == 0)
                {
                    return false;
                }
                var aNodes = liNodes[0].QuerySelectorAll("a");
                foreach (var href in aNodes)
                {
                    string link = href.GetAttribute("href");
                    string protcol = link.Contains("https") ? "" : _gitHubHome;
                    versionLinkList.Add($"{protcol}{link}");
                }
            }

            try
            {
                for (int i = 0; i < Versions.Length; i++)
                {
                    VersionLink.Add(Versions[i], versionLinkList[i]);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
