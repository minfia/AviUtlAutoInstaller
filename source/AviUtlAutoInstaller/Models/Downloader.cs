using AngleSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AviUtlAutoInstaller.Models
{
    enum DownloadResult
    {
        /// <summary>
        /// 完了
        /// </summary>
        Complete,
        /// <summary>
        /// 404エラー
        /// </summary>
        Connection404Error,
        /// <summary>
        /// 接続タイムアウトエラー
        /// </summary>
        ConnectionTimeoutError,
        /// <summary>
        /// 接続エラー
        /// </summary>
        ConnectionError,
        /// <summary>
        /// ダウンロードサイズ取得エラー
        /// </summary>
        DownloadSizeGetError,
        /// <summary>
        /// ファイル名取得エラー
        /// </summary>
        DownloadFileNameGetError,
        /// <summary>
        /// ダウンロードエラー
        /// </summary>
        DownloadError,
        /// <summary>
        /// 対象ファイルがウィルスに感染している可能性あり(GoogleDriveのみ)
        /// </summary>
        GDriveVirus,
    }

    class Downloader
    {
        private string _filePath;
        /// <summary>
        /// ダウンロード完了サイズ
        /// </summary>
        public ulong DownloadCompleteSize { get; private set; }
        /// <summary>
        /// ダウンロードするファイルサイズ
        /// </summary>
        public ulong DownloadFileSize { get; private set; }

        public Downloader(string filePath)
        {
            _filePath = filePath;
        }

        public DownloadResult DownloadStart(string url, string fileName)
        {
            DownloadResult result;
            if (url.Contains("drive.google"))
            {
                result = GoogleDriveDownload(url, fileName);
            }
            else
            {
                result = GeneralDownload(url, fileName);
            }

            if ((DownloadResult.Complete != result) && (File.Exists($"{_filePath}\\{fileName}")))
            {
                File.Delete($"{_filePath}\\{fileName}");
            }
            return result;
        }

        private DownloadResult GeneralDownload(string url, string fileName)
        {
            DownloaderBase dl = new DownloaderBase();
            DownloadResult res;

            if ((res = dl.IsConnectURL(url)) != DownloadResult.Complete)
            {
                return res;
            }
            dl.GetContentType(out string contentType);
            Console.WriteLine($"Content-Type: {contentType}");
            if (!dl.GetDownloadFileSize(out ulong size) || (size <= 0))
            {
                return DownloadResult.DownloadSizeGetError;
            }
            DownloadFileSize = size;
            if (string.IsNullOrEmpty(fileName))
            {
                if (!dl.GetFileName(out fileName) || string.IsNullOrEmpty(fileName))
                {
                    return DownloadResult.DownloadFileNameGetError;
                }
            }

            Task task = Task.Run(() =>
            {
                res = dl.Download(DownloadFileSize, _filePath, fileName);
            });
            DownloadCompleteSize = 0;
            do
            {
                if (DownloadCompleteSize != dl.DownloadCompleteSize)
                {
                    Console.WriteLine($"{dl.DownloadCompleteSize}/{DownloadFileSize}");
                    DownloadCompleteSize = dl.DownloadCompleteSize;
                }
            } while (task.Status != TaskStatus.RanToCompletion);
            
            return res;
        }

        private DownloadResult GoogleDriveDownload(string url, string fileName)
        {
            GDriveDownloader dl = new GDriveDownloader();
            DownloadResult res;

            if ((res = dl.IsConnectURL(url)) != DownloadResult.Complete)
            {
                return res;
            }

            dl.GetContentType(out string contentType);
            Console.WriteLine($"ContentType: {contentType}");
            while (contentType.Contains("utf-8"))
            {
                if ((res = dl.IsConnectURL(url)) != DownloadResult.Complete)
                {
                    return res;
                }
                dl.GetContentType(out contentType);
                Console.WriteLine($"ContentType: {contentType}");
            }
            if (!dl.GetDownloadFileSize(out ulong size) || (size <= 0))
            {
                return DownloadResult.DownloadSizeGetError;
            }
            DownloadFileSize = size;
            if (string.IsNullOrEmpty(fileName))
            {
                if (!dl.GetFileName(out fileName) || string.IsNullOrEmpty(fileName))
                {
                    return DownloadResult.DownloadFileNameGetError;
                }
            }

            Task task = Task.Run(() =>
            {
                res = dl.Download(DownloadFileSize, _filePath, fileName);
            });

            DownloadCompleteSize = 0;
            do
            {
                if (DownloadCompleteSize != dl.DownloadCompleteSize)
                {
                    Console.WriteLine($"{dl.DownloadCompleteSize}/{DownloadFileSize}");
                    DownloadCompleteSize = dl.DownloadCompleteSize;
                }
            } while (task.Status != TaskStatus.RanToCompletion);

            return res;
        }


        private class DownloaderBase
        {

            private readonly int _blockSize = 1024;
            private static HttpClient _httpClient = new HttpClient();
            private Uri _uri;
            private WebHeaderCollection _header;
            private protected CookieCollection _cookieCollection;
            /// <summary>
            /// ダウンロード完了サイズ
            /// </summary>
            public ulong DownloadCompleteSize { get; private protected set; }
            /// <summary>
            /// ダウンロードするファイルサイズ
            /// </summary>
            public ulong DownloadFileSize { get; private set; }

            public DownloaderBase()
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 |
                                                       SecurityProtocolType.Tls12 |
                                                       SecurityProtocolType.Tls13;
            }

            /// <summary>
            /// ヘッダを取得
            /// </summary>
            /// <param name="uri">取得するURI</param>
            /// <returns>DownloadResult</returns>
            private DownloadResult GetHeader(Uri uri)
            {
                HttpWebRequest webRequest = null;
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    CookieContainer cookieContainer = new CookieContainer();
                    webRequest.CookieContainer = new CookieContainer();
                    webRequest.CookieContainer.Add(cookieContainer.GetCookies(webRequest.RequestUri));
                    WebResponse webResponse = webRequest.GetResponse();
                    _header = ((HttpWebResponse)webResponse).Headers;
                    _cookieCollection = ((HttpWebResponse)webResponse).Cookies;
                }
                catch (WebException e)
                { 
                    Console.WriteLine(e.Message);
                    return WebExceptionPaser(e);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return DownloadResult.ConnectionError;
                }
                finally
                {
                    if (webRequest != null)
                    {
                        // 連続呼び出しでエラーになる場合があるのでその対処
                        webRequest.Abort();
                    }
                }

                return DownloadResult.Complete;
            }

            private DownloadResult WebExceptionPaser(WebException webException)
            {
                if (webException.Response == null)
                {
                    // WebExceptionStatus
                    var status = webException.Status;
                    switch (status)
                    {
                        case WebExceptionStatus.Success:
                            return DownloadResult.Complete;
                        case WebExceptionStatus.Timeout:
                            return DownloadResult.ConnectionTimeoutError;
                    }
                }
                else
                {
                    // HttpWebResponce Status
                    var response = webException.Response as HttpWebResponse;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return DownloadResult.Complete;
                        case HttpStatusCode.NotFound:
                            return DownloadResult.Connection404Error;
                    }
                }
                return DownloadResult.ConnectionError;
            }

            /// <summary>
            /// チャンクのサイズを取得
            /// </summary>
            /// <param name="size">取得したサイズ</param>
            /// <returns>成否</returns>
            private bool GetChunkSize(out ulong size)
            {
                size = 0;
                bool isChunked;
                isChunked = _header.Get("Transfer-Encoding")?.Contains("chunked") ?? false;
                if (!isChunked)
                {
                    return false;
                }

                WebRequest webRequest = WebRequest.Create(_uri);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                WebResponse webResponse = webRequest.GetResponse();

                Stream stream = ((HttpWebResponse)webResponse).GetResponseStream();
                byte[] buf = new byte[0x2000];
                int readSize;
                ulong totalSize = 0;
                do
                {
                    readSize = stream.Read(buf, 0, buf.Length);
                    totalSize += (ulong)readSize;
                } while (readSize > 0);
                size = totalSize;

                return true;
            }

            /// <summary>
            /// ContentLengthを取得
            /// </summary>
            /// <param name="size">取得したサイズ</param>
            /// <returns>成否</returns>
            private bool GetContentLength(out ulong size)
            {
                size = 0;

                try
                {
                    string contentLength = _header.Get("Content-Length");
                    if (contentLength == null)
                    {
                        return false;
                    }
                    size = ulong.Parse(contentLength);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 指定URLに接続できるか確認する
            /// </summary>
            /// <param name="url"></param>
            /// <returns>DownloadResult</returns>
            public virtual DownloadResult IsConnectURL(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return DownloadResult.ConnectionError;
                }

                DownloadResult res = DownloadResult.Complete;
                try
                {
                    _uri = new Uri(url);
                    res = GetHeader(_uri);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return res;
                }

                return res;
            }

            /// <summary>
            /// ダウンロードするファイルのサイズを取得
            /// </summary>
            /// <param name="size">取得したサイズ</param>
            /// <returns>成否</returns>
            public bool GetDownloadFileSize(out ulong size)
            {
                size = 0;
                if (_header == null)
                {
                    Console.WriteLine("Header is null.");
                    return false;
                }

                if (GetContentLength(out size) && (size != 0))
                {
                    DownloadFileSize = size;
                    return true;
                }
                else if (GetChunkSize(out size) && (size != 0))
                {
                    DownloadFileSize = size;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// ダウンロードするコンテンツタイプを取得
            /// </summary>
            /// <param name="contentType">取得したコンテンツタイプ</param>
            /// <returns>成否</returns>
            public bool GetContentType(out string contentType)
            {
                contentType = "";
                if (_header == null)
                {
                    return false;
                }

                contentType = _header.Get("Content-Type");

                return true;
            }

            /// <summary>
            /// ダウンロードするファイル名を取得
            /// </summary>
            /// <param name="fileName">取得したファイル名</param>
            /// <returns>成否</returns>
            public virtual bool GetFileName(out string fileName)
            {
                fileName = "";
                if (_header == null)
                {
                    return false;
                }

                string contentDisposition;
                string index = "filename=";
                contentDisposition = _header.Get("Content-Disposition");

                if (string.IsNullOrEmpty(contentDisposition) || !contentDisposition.Contains(index))
                {
                    return false;
                }

                int indexPos = contentDisposition.IndexOf(index);
                fileName = contentDisposition.Substring((indexPos + index.Length), (contentDisposition.Length - (indexPos + index.Length)));

                return true;
            }

            /// <summary>
            /// ファイルのダウンロード
            /// </summary>
            /// <param name="downloadSize">ダウンロードするファイルのサイズ</param>
            /// <param name="downloadPath">ダウンロード先のパス</param>
            /// <param name="fileName">ダウンロードするファイル名</param>
            /// <returns>ダウンロード結果</returns>
            public virtual DownloadResult Download(ulong downloadSize, string downloadPath, string fileName)
            {
                ServicePoint sp;

                sp = ServicePointManager.FindServicePoint(_uri);
                sp.ConnectionLeaseTimeout = 60 * 1000;

                try
                {
                    using (HttpResponseMessage responseMessage = _httpClient.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead).Result)
                    using (Stream inputStream = responseMessage.Content.ReadAsStreamAsync().Result)
                    using (FileStream fs = new FileStream($"{downloadPath}\\{fileName}", FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] temp = new byte[_blockSize];
                        int readSize;
                        if (downloadSize < (ulong)_blockSize)
                        {
                            // ダウンロードファイルのサイズがブロックサイズより小さい場合は、ダウンロードサイズに合わせる
                            readSize = (int)downloadSize;
                        }
                        else
                        {
                            readSize = _blockSize;
                        }
                        ulong reaminingSize = downloadSize;
                        while (true)
                        {
                            int buffer = inputStream.Read(temp, 0, readSize);
                            if (buffer == 0)
                            {
                                break;
                            }
                            fs.Write(temp, 0, buffer);
                            DownloadCompleteSize += (ulong)buffer;
                            reaminingSize -= (ulong)buffer;
                            if (reaminingSize < (ulong)readSize)
                            {
                                readSize = (int)reaminingSize;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return DownloadResult.DownloadError;
                }

                return DownloadResult.Complete;
            }

            /// <summary>
            /// 指定文字列から範囲(文字列)を指定して切り抜く
            /// </summary>
            /// <param name="src">切り抜く文字列</param>
            /// <param name="start">開始文字列</param>
            /// <param name="end">終了文字列</param>
            /// <param name="dst">切り抜いた文字列</param>
            /// <returns>成否</returns>
            protected bool SubString(string src, string start, string end, out string dst)
            {
                dst = "";
                int indexStart = src.IndexOf(start);
                if (indexStart == -1)
                {
                    return false;
                }
                indexStart += start.Length;

                int indexEnd = 0;
                if (!string.IsNullOrEmpty(end))
                {
                    indexEnd = src.IndexOf(end);
                    if (indexEnd == -1)
                    {
                        return false;
                    }
                }

                dst = (end == null) ? src.Substring(indexStart) : src.Substring(indexStart, (indexEnd - indexStart));

                return true;
            }
        }

        private class GDriveDownloader : DownloaderBase
        {
            string _fileId;

            private enum VirusCheckResult
            {
                /// <summary>
                /// ウィルスチェック警告
                /// </summary>
                VirusWarning,
                /// <summary>
                /// ウィルス感染エラー
                /// </summary>
                VirusInfection,
                /// <summary>
                /// 通常ファイル
                /// </summary>
                NormalFile,
                /// <summary>
                /// エラー
                /// </summary>
                CheckError,
            }

            /// <summary>
            /// DLしたファイルが、ウィルスチェックに関する警告ページかチェック
            /// </summary>
            /// <param name="filePath">チェックするファイルのパス</param>
            /// <returns>チェック結果</returns>
            private VirusCheckResult CheckVirusRelatedHTML(string filePath)
            {
                string htmlText;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    htmlText = sr.ReadToEnd();
                }
                if (string.IsNullOrEmpty(htmlText))
                {
                    return VirusCheckResult.CheckError;
                }

                using (var document = BrowsingContext.New(Configuration.Default).OpenAsync(req => req.Content(htmlText)))
                {
                    var elements = document.Result.QuerySelectorAll("p");
                    foreach (var element in elements)
                    {
                        if (element.TextContent.Contains("Google Drive can't scan this file for viruses."))
                        {
                            Console.WriteLine("Can't virus check.");
                            return VirusCheckResult.VirusWarning;
                        }
                        else if (element.TextContent.Contains("infecton"))
                        {
                            Console.WriteLine("virus infection.");
                            // TODO: GoogleDriveのウィルスチェックに引っかかることがあったら更新する
                            return VirusCheckResult.VirusInfection;
                        }
                    }
                }

                return VirusCheckResult.NormalFile;
            }

            /// <summary>
            /// 指定URLに接続できるか確認する
            /// </summary>
            /// <param name="shareURL">共有URL</param>
            /// <returns>成否</returns>
            public override DownloadResult IsConnectURL(string shareURL)
            {
                string[][] strArray = new string[][] {  new string[] {"uc?id=", null},    // 直接URL
                                                        new string[] {"/d/", "/view"},    // 共有URL
                                                        new string[] {"open?id=", null},  // 共有URL
                                                     };

                string url;
                foreach (string[] str in strArray)
                {
                    string temp;
                    if (SubString(shareURL, str[0], str[1], out temp))
                    {
                        _fileId = temp;
                        break;
                    }
                }
                url = $"https://drive.google.com/uc?id={_fileId}";

                return base.IsConnectURL(url);
            }

            /// <summary>
            /// ダウンロードするファイル名を取得
            /// </summary>
            /// <param name="fileName">取得したファイル名</param>
            /// <returns>成否</returns>
            public override bool GetFileName(out string fileName)
            {
                fileName = "";
                string contentType;
                if (!base.GetFileName(out contentType))
                {
                    return false;
                }
                string encodeFileName;
                if (!SubString(contentType, "''", null, out encodeFileName))
                {
                    return false;
                }
                string encodeType;
                if (!SubString(contentType, "filename*=", "''", out encodeType))
                {
                    return false;
                }

                Encoding encoding;
                try
                {
                    encoding = Encoding.GetEncoding(encodeType);
                }
                catch
                {
                    encoding = Encoding.GetEncoding("utf-8");
                }
                fileName = HttpUtility.UrlDecode(encodeFileName, encoding);

                return true;
            }

            public override DownloadResult Download(ulong downloadSize, string downloadPath, string fileName)
            {
                string filePath = $"{downloadPath}\\{fileName}";
                Cookie cookie = null;

                foreach (Cookie c in _cookieCollection)
                {
                    if (c.Domain.Contains(".drive.google.com"))
                    {
                        cookie = c;
                        break;
                    }
                }

                DownloadResult res;
                if (cookie != null)
                {
                    string url = $"https://drive.google.com/uc?export=download&confirm={cookie.Value}&id={_fileId}";
#if DEBUG
                    Console.WriteLine("DL URL: " + url);
#endif
                    if ((res = IsConnectURL(url)) != DownloadResult.Complete)
                    {
                        return res;
                    }
                    if (!GetDownloadFileSize(out ulong dlSize))
                    {
                        return DownloadResult.DownloadSizeGetError;
                    }
                    downloadSize = dlSize;
                }

                base.DownloadCompleteSize = 0;

                while (true)
                {
                    res = base.Download(downloadSize, downloadPath, fileName);
                    if (!GetFileName(out string name) || string.IsNullOrEmpty(name))
                    {
                        return DownloadResult.DownloadFileNameGetError;
                    }
                    if (!string.IsNullOrEmpty(HttpUtility.UrlDecode(name)))
                    {
                        break;
                    }
                }
                if (base.DownloadCompleteSize != downloadSize)
                {
                    return DownloadResult.DownloadError;
                }

                if (CheckVirusRelatedHTML($"{downloadPath}\\{fileName}") == VirusCheckResult.VirusWarning)
                {
                    return DownloadResult.DownloadError;
                }
                GC.Collect();

                return res;
            }
        }
    }
}
