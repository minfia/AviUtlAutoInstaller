using System;
using System.Collections.Generic;

namespace AviUtlAutoInstaller.Models.Network.Parser
{
    class GitHub : PaserBase
    {
        /// <summary>
        /// バージョンリスト
        /// </summary>
        public string[] Versions { get; private set; }

        /// <summary>
        /// バージョンとリンクの紐付け
        /// </summary>
        public Dictionary<string, string> VersionLink { get; private set; }

        /// <summary>
        /// バージョンとリリースノートなどの記載部
        /// </summary>
        public Dictionary<string, string> VersionBody { get; private set; }

        public GitHub()
        {
            Versions = new string[0];
            VersionLink = new Dictionary<string, string>();
            VersionBody = new Dictionary<string, string>();
        }

        /// <summary>
        /// JSONファイルを解析
        /// </summary>
        /// <param name="jsonText">JSONファイルの内容</param>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        public bool Parse(string jsonText)
        {
            VersionLink.Clear();
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return false;
            }

            Json json = new();

            try
            {
                var dic = json.Parse(jsonText);

                if (!ParseVersionList(dic) || !ParseVersionLinkList(dic) || !ParseVersionBodyList(dic))
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
        /// URIからJSONテキストを取得して解析
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.Net.ProtocolViolationException"></exception>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public bool Parse(Uri uri)
        {
            string jsonText;
            try
            {
                jsonText = GetRestAPI(uri);
            }
            catch
            {
                throw;
            }

            return Parse(jsonText);
        }

        /// <summary>
        /// releaseのバージョンリストを取得
        /// </summary>
        /// <param name="list"></param>
        private bool ParseVersionList(List<Dictionary<string, object>> list)
        {
            List<string> verList = new();

            foreach (var item in list)
            {
                string ver = item["tag_name"].ToString();
                verList.Add(ver);
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
        /// <param name="list"></param>
        private bool ParseVersionLinkList(List<Dictionary<string, object>> list)
        {
            List<string> versionLinkList = new();

            foreach (var item in list)
            {
                List<Dictionary<string, object>> assetList = (List<Dictionary<string, object>>)item["assets"];

                foreach (var asset in assetList)
                {
                    // 複数assetがある場合は、現状最初のもののみ取得
                    string verURL = asset["browser_download_url"].ToString();
                    versionLinkList.Add(verURL);
                    break;
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

        /// <summary>
        /// リリースノートなどの記載部を取得し、バージョンと紐付け
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool ParseVersionBodyList(List<Dictionary<string, object>> list)
        {
            List<string> bodyList = new();

            foreach (var item in list)
            {
                string body = item["body"].ToString();
                bodyList.Add(body);
            }

            try
            {
                for (int i = 0; i < Versions.Length; i++)
                {
                    VersionBody.Add(Versions[i], bodyList[i]);
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
