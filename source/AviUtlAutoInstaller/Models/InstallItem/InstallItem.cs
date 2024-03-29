﻿using System;
using System.Collections.Generic;

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

    public enum InstallStatus
    {
        NotInstall,
        Installed,
        Update
    }

    partial class InstallItem : NotificationObject
    {
        private bool _isSelect = false;
        /// <summary>
        /// インストール選択の有無
        /// </summary>
        public bool IsSelect
        {
            get { return _isSelect; }
            set
            {
                if (SetProperty(ref _isSelect, value))
                {
                    if (DependentAction != null && !string.IsNullOrEmpty(DependentName))
                    {
                        DependentAction(this);
                    }
                }
            }
        }

        public delegate void DependentDelegate(InstallItem item);
        /// <summary>
        /// 依存関係処理
        /// </summary>
        public static DependentDelegate DependentAction = null;

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

        private InstallStatus _isInstalled;
        /// <summary>
        /// インストール済みの有無
        /// </summary>
        public InstallStatus IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
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

        private bool _isSpecialItem;
        /// <summary>
        /// 単純にインストールできないアイテム
        /// </summary>
        public bool IsSpecialItem
        {
            get { return _isSpecialItem; }
            set { SetProperty(ref _isSpecialItem, value); }
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

        private uint _profileItemRevision;
        /// <summary>
        /// 
        /// </summary>
        public uint ProfileItemRevision
        {
            get { return _profileItemRevision; }
            set { SetProperty(ref _profileItemRevision, value); }
        }

        private uint _itemRevision = 0;
        /// <summary>
        /// リポジトリが持つこのアイテムのリビジョン番号
        /// </summary>
        public uint ItemRevision
        { 
            get { return _itemRevision; }
            set { SetProperty(ref _itemRevision, value); }
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

        private List<string> _installFileList = new();
        /// <summary>
        /// インストールファイルのリスト
        /// </summary>
        public List<string> InstallFileList
        {
            get { return _installFileList; }
            private set { SetProperty(ref _installFileList, value); }
        }

        private string _uninstallFile = string.Empty;
        /// <summary>
        /// アンインストールファイル
        /// </summary>
        public string UninstallFile
        {
            get { return _uninstallFile; }
            set
            {
                SetProperty(ref _uninstallFile, value);
                ConvertFileList(InstallFile, ref _uninstallFileList, ',');
            }
        }

        private List<string> _uninstallFileList = new();
        /// <summary>
        /// アンインストールファイルのリスト
        /// </summary>
        public List<string> UninstallFileList
        {
            get { return _uninstallFileList; }
            private set { SetProperty(ref _uninstallFileList, value); }
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

        private List<string> _externalFileList = new();
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

        private List<string> _externalFileURLList = new();
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

        private string _sectionType;
        /// <summary>
        /// ジャンル
        /// </summary>
        public string SectionType
        {
            get { return _sectionType; }
            set { SetProperty(ref _sectionType, value); }
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

        private string _description;
        /// <summary>
        /// 概要
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private string _downloadPage;
        /// <summary>
        /// ダウンロードページ
        /// </summary>
        public string DownloadPage
        {
            get { return _downloadPage; }
            set { SetProperty(ref _downloadPage, value); }
        }

        private string _guideURL;
        /// <summary>
        /// 使い方URL
        /// </summary>
        public string GuideURL
        {
            get { return _guideURL; }
            set { SetProperty(ref _guideURL, value); }
        }

        public static Dictionary<int, string> MakerTypeDic = new();

        /// <summary>
        /// ファイルタイプに対応する文字列
        /// </summary>
        private static readonly Dictionary<InstallFileType, string> _fileTypeDic = new()
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
        public static Dictionary<int, string> SectionTypeDic = new();

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
