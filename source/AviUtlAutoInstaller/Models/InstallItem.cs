using AviUtlAutoInstaller.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    /// <summary>
    /// ファイルタイプ
    /// </summary>
    public enum InstallFileType
    {
        Tool,
        Main,
        Script,
        Plugin,
        Image,
        Encoder,
    }

    /// <summary>
    /// ダウンロード/インストールの優先度
    /// </summary>
    public enum InstallPriority
    {
        High,
        Low
    }

    class InstallItem : NotificationObject
    {
        private bool _isSelect = false;
        /// <summary>
        /// インストールの有無
        /// </summary>
        public bool IsSelect
        {
            get { return _isSelect; }
            set { SetProperty(ref _isSelect, value); }
        }

        /// <summary>
        /// アイテムセレクトの許可/不許可
        /// </summary>
        public bool IsItemSelectEnable { get; set; }

        private bool _isDownloadCompleted = false;
        /// <summary>
        /// ダウンロード完了の有無
        /// </summary>
        public bool IsDownloadCompleted
        {
            get { return _isDownloadCompleted; }
            set { SetProperty(ref _isDownloadCompleted, value); }
        }

        private int _no = 0;
        /// <summary>
        /// 項目番号
        /// </summary>
        public int No
        {
            get { return _no; }
            set { SetProperty(ref _no, value); }
        }

        private InstallPriority _priority = InstallPriority.Low;
        /// <summary>
        /// ダウンロード/インストール優先度
        /// </summary>
        public InstallPriority Priority
        {
            get { return _priority; }
            set { SetProperty(ref _priority, value); }
        }

        private string _name = string.Empty;
        /// <summary>
        /// 項目名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _commandName = string.Empty;
        /// <summary>
        /// コマンドラインでの指定名
        /// </summary>
        public string CommandName
        {
            get { return _commandName; }
            set { SetProperty(ref _commandName, value); }
        }

        private string _url = string.Empty;
        /// <summary>
        /// URL
        /// </summary>
        public string URL
        {
            get { return _url; }
            set
            {
                SetProperty(ref _url, value);
                if (_url == "")
                {
                    DownloadExecute = false;
                }
            }
        }

        private string _fileName = string.Empty;
        /// <summary>
        /// ダウンロードのするファイル名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        private InstallFileType _fileType = InstallFileType.Script;
        /// <summary>
        /// インストールするファイルの種類
        /// </summary>
        public InstallFileType FileType
        {
            get { return _fileType; }
            set
            {
                SetProperty(ref _fileType, value);
                FileTypeString = GetFileTypeString(value);
            }
        }
        private string _fileTypeString = GetFileTypeString(InstallFileType.Script);
        public string FileTypeString
        {
            get { return _fileTypeString; }
            set { SetProperty(ref _fileTypeString, value); }
        }

        private string _version = string.Empty;
        /// <summary>
        /// ファイルバージョン
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        private string _scriptDirName = string.Empty;
        /// <summary>
        /// スクリプトディレクトリ名(FileType.Script時のみ有効)
        /// </summary>
        public string ScriptDirName
        {
            get { return _scriptDirName; }
            set { SetProperty(ref _scriptDirName, value); }
        }

        private string _installFile = string.Empty;
        /// <summary>
        /// 追加ファイル
        /// </summary>
        public string InstallFile
        {
            get { return _installFile; }
            set
            {
                SetProperty(ref _installFile, value);
                ConvertAppendFileList(',');
            }
        }

        private List<string> _installFileList = new List<string>();
        /// <summary>
        /// 追加ファイルのリスト
        /// </summary>
        public List<string> InstallFileList
        {
            get { return _installFileList; }
            private set { SetProperty(ref _installFileList, value); }
        }

        private string _nicoVideoID = string.Empty;
        /// <summary>
        /// ニコニコ動画のID
        /// </summary>
        public string NicoVideoID
        {
            get { return _nicoVideoID; }
            set { SetProperty(ref _nicoVideoID , value); }
        }

        private bool _downloadExecute = true;
        /// <summary>
        /// 個別ダウンロードの実行可能状態<br/>
        /// true:実行可能, false:実行不可
        /// </summary>
        public bool DownloadExecute
        {
            get { return _downloadExecute; }
            set { SetProperty(ref _downloadExecute, value); }
        }

        /// <summary>
        /// ファイルタイプに対応する文字列
        /// </summary>
        private static Dictionary<InstallFileType, string> _fileTypeDic = new Dictionary<InstallFileType, string>()
        {
            { InstallFileType.Tool, "ツール" },
            { InstallFileType.Main, "メイン" },
            { InstallFileType.Script, "スクリプト" },
            { InstallFileType.Plugin, "プラグイン" },
            { InstallFileType.Image, "画像" },
            { InstallFileType.Encoder, "エンコーダー" },
        };

        /// <summary>
        /// ファイルタイプから対応する文字列を取得
        /// </summary>
        /// <param name="fileType">ファイルタイプ</param>
        /// <returns>対応する文字列</returns>
        public static string GetFileTypeString(InstallFileType fileType)
        {
            if (Enum.GetValues(typeof(InstallFileType)).Length < (int)fileType)
            {
                return "";
            }

            return _fileTypeDic[fileType];
        }

        /// <summary>
        /// AppendFileからAppendFileListへ変換
        /// </summary>
        /// <param name="splitChar">分割文字</param>
        /// <returns></returns>
        private void ConvertAppendFileList(char splitChar)
        {
            if (char.IsWhiteSpace(splitChar))
            {
                return;
            }
            _installFileList.Clear();
            string[] array = InstallFile.Split(splitChar);
            foreach (string s in array)
            {
                InstallFileList.Add(s.Trim());
            }
        }

    }
}
