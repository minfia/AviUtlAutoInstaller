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
    /// セクションタイプ
    /// </summary>
    public enum InstallSectionType
    {
        /// <summary>
        /// 本体
        /// </summary>
        Main,
        /// <summary>
        /// エンコーダ
        /// </summary>
        Encoder,
        /// <summary>
        /// ファイル入力
        /// </summary>
        FileInput,
        /// <summary>
        /// エフェクト
        /// </summary>
        Effect,
        /// <summary>
        /// エフェクト(音声)
        /// </summary>
        EffectAudio,
        /// <summary>
        /// フィルタ
        /// </summary>
        Filter,
        /// <summary>
        /// シーンチェンジ
        /// </summary>
        SceneChange,

        /// <summary>
        /// アプリ補助
        /// </summary>
        AppAssist = 250,

        /// <summary>
        /// その他
        /// </summary>
        Other = 255
    }

    /// <summary>
    /// ダウンロード/インストールの優先度
    /// </summary>
    public enum InstallPriority
    {
        High,
        Low
    }

    partial class InstallItem : NotificationObject
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

        private bool _isInstallCompleted;
        /// <summary>
        /// インストール完了の有無
        /// </summary>
        public bool IsInstallCompleted
        {
            get { return _isInstallCompleted; }
            set { SetProperty(ref _isInstallCompleted, value); }
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

        private string _makerName;
        /// <summary>
        /// 製作者名
        /// </summary>
        public string MakerName
        {
            get { return _makerName; }
            set { SetProperty(ref _makerName, value); }
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

        private string _downloadFileName = string.Empty;
        /// <summary>
        /// ダウンロードのするファイル名
        /// </summary>
        public string DownloadFileName
        {
            get { return _downloadFileName; }
            set { SetProperty(ref _downloadFileName, value); }
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
        /// インストールファイル
        /// </summary>
        public string InstallFile
        {
            get { return _installFile; }
            set
            {
                SetProperty(ref _installFile, value);
                ConvertFileList(InstallFile, ref _installFileList, ',');
            }
        }

        private List<string> _installFileList = new List<string>();
        /// <summary>
        /// インストールファイルのリスト
        /// </summary>
        public List<string> InstallFileList
        {
            get { return _installFileList; }
            private set { SetProperty(ref _installFileList, value); }
        }

        private string _externalFile;
        /// <summary>
        /// VisualStdio再頒布パッケージ等の外部ファイル
        /// </summary>
        public string ExternalFile
        {
            get { return _externalFile; }
            set
            {
                SetProperty(ref _externalFile, value);
                ConvertFileList(ExternalFile, ref _externalFileList, ',');
            }
        }

        private List<string> _externalFileList = new List<string>();
        /// <summary>
        /// VisualStdio再頒布パッケージ等の外部ファイルのリスト
        /// </summary>
        public List<string> ExternalFileList
        {
            get { return _externalFileList; }
            private set { SetProperty(ref _externalFileList, value); }
        }

        private string _externalFileURL;
        /// <summary>
        /// VisualStdio再頒布パッケージ等の外部ファイルのURL
        /// </summary>
        public string ExternalFileURL
        {
            get { return _externalFileURL; }
            set
            {
                SetProperty(ref _externalFileURL, value);
                ConvertFileList(ExternalFileURL, ref _externalFileURLList, ',');
            }
        }

        private List<string> _externalFileURLList = new List<string>();
        /// <summary>
        /// VisualStdio再頒布パッケージ等の外部ファイルのURLリスト
        /// </summary>
        public List<string> ExternalFileURLList
        {
            get { return _externalFileURLList; }
            private set { SetProperty(ref _externalFileURLList, value); }
        }

        private string _nicoVideoID = string.Empty;
        /// <summary>
        /// ニコニコ動画のID
        /// </summary>
        public string NicoVideoID
        {
            get { return _nicoVideoID; }
            set { SetProperty(ref _nicoVideoID, value); }
        }

        private string _dependentName;
        /// <summary>
        /// プラグインやスクリプトの依存元の名前(CommandName)
        /// </summary>
        public string DependentName
        {
            get { return _dependentName; }
            set { SetProperty(ref _dependentName, value); }
        }

        private InstallSectionType _sectionType = InstallSectionType.Other;
        public InstallSectionType SectionType
        {
            get { return _sectionType; }
            set
            {
                SetProperty(ref _sectionType, value);
                SectionTypeString = GetSectionTypeString(value);
            }
        }

        private string _sectionTypeString = GetSectionTypeString(InstallSectionType.Other);
        public string SectionTypeString
        {
            get { return _sectionTypeString; }
            set { SetProperty(ref _sectionTypeString, value); }
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
        /// セクションタイプに対応する文字列
        /// </summary>
        private static Dictionary<InstallSectionType, string> _sectionTypeDic = new Dictionary<InstallSectionType, string>()
        {
            { InstallSectionType.Main, "本体" },
            { InstallSectionType.Encoder, "エンコーダ" },
            { InstallSectionType.FileInput, "ファイル入力" },
            { InstallSectionType.Effect, "エフェクト" },
            { InstallSectionType.EffectAudio, "エフェクト(音声)" },
            { InstallSectionType.Filter, "フィルタ" },
            { InstallSectionType.SceneChange, "シーンチェンジ" },

            { InstallSectionType.AppAssist, "アプリ補助" },

            { InstallSectionType.Other, "その他" }
        };

        /// <summary>
        /// セクションタイプから対応する文字列を取得
        /// </summary>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        public static string GetSectionTypeString(InstallSectionType sectionType)
        {
            if (InstallSectionType.AppAssist != sectionType && 
                InstallSectionType.Other != sectionType &&
                Enum.GetValues(typeof(InstallSectionType)).Length < (int)sectionType)
            {
                return "";
            }

            return _sectionTypeDic[sectionType];
        }

        /// <summary>
        /// FileからFileListへ変換
        /// </summary>
        /// <param name="splitChar">分割文字</param>
        /// <returns></returns>
        private void ConvertFileList(string Item, ref List<string> itemList, char splitChar)
        {
            if (char.IsWhiteSpace(splitChar))
            {
                return;
            }
            itemList.Clear();
            string[] array = Item.Split(splitChar);
            if (string.IsNullOrWhiteSpace(array[0]))
            {
                return;
            }
            foreach (string s in array)
            {
                itemList.Add(s.Trim());
            }
        }
    }
}
