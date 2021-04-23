using AviUtlAutoInstaller.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AviUtlAutoInstaller.ViewModels
{
    class InstallItemEditViewModel : NotificationObject
    {
        /// <summary>
        /// 表示タイプ
        /// </summary>
        public enum EditShowType
        {
            /// <summary>
            /// 追加
            /// </summary>
            Add,
            /// <summary>
            /// 変更
            /// </summary>
            Modify,
        };

        public string Title { get; set; }
        public EditShowType EditType { get; set; }

        /// <summary>
        /// 変更前インストールアイテム保持用
        /// </summary>
        private InstallItem _installItem;

        #region Acceptボタンの設定
        private DelegateCommand _acceptCommand;
        public DelegateCommand AcceptCommand
        {
            get { return _acceptCommand; }
        }
        #endregion

        #region 設定項目の設定
        private string _name;
        /// <summary>
        /// 項目名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private string _url;
        /// <summary>
        /// ダウンロード先URL
        /// </summary>
        public string URL
        {
            get { return _url; }
            set
            {
                if (SetProperty(ref _url, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private string _fileName;
        /// <summary>
        /// ダウンロードするファイル名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (SetProperty(ref _fileName, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private InstallFileType _fileType;
        /// <summary>
        /// インストールするファイルの種類
        /// </summary>
        public InstallFileType FileType
        {
            get { return _fileType; }
            set
            {
                SetProperty(ref _fileType, value);

            }
        }

        private string _version;
        /// <summary>
        /// バージョン
        /// </summary>
        public string Version
        {
            get { return _version; }
            set
            {
                if (SetProperty(ref _version, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private Visibility _isScriptDirNameVisible;
        /// <summary>
        /// スクリプトフォルダ名の表示/非表示設定
        /// </summary>
        public Visibility IsScriptDirNameVisible
        {
            get { return _isScriptDirNameVisible; }
            set { SetProperty(ref _isScriptDirNameVisible, value); }
        }
        private string _scriptDirName;
        /// <summary>
        /// スクリプトフォルダ名
        /// </summary>
        public string ScriptDirName
        {
            get { return _scriptDirName; }
            set
            {
                if (SetProperty(ref _scriptDirName, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private string _appendFile;
        /// <summary>
        /// 追加ファイル
        /// </summary>
        public string AppendFile
        {
            get { return _appendFile; }
            set
            {
                if (SetProperty(ref _appendFile, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }

        private string _nicoVideoID;
        /// <summary>
        /// ニコニコ動画ID
        /// </summary>
        public string NicoVideoID
        {
            get { return _nicoVideoID; }
            set
            {
                if (SetProperty(ref _nicoVideoID, value))
                {
                    AcceptCommand.RaiseCanExetuteChanged();
                }
            }
        }
        #endregion

        #region エラー関係の設定
        private string _nameError;
        /// <summary>
        /// 項目名エラー表示
        /// </summary>
        public string NameError
        {
            get { return _nameError; }
            set { SetProperty(ref _nameError, value); }
        }

        private string _urlError;
        /// <summary>
        /// URLエラー表示
        /// </summary>
        public string URLError
        {
            get { return _urlError; }
            set { SetProperty(ref _urlError, value); }
        }

        private string _fileNameError;
        /// <summary>
        /// ダウンロードファイル名エラー表示
        /// </summary>
        public string FileNameError
        {
            get { return _fileNameError; }
            set { SetProperty(ref _fileNameError, value); }
        }

        private string _versionError;
        /// <summary>
        /// バージョンエラー表示
        /// </summary>
        public string VersionError
        {
            get { return _versionError; }
            set { SetProperty(ref _versionError, value); }
        }

        private string _scriptDirNameError;
        /// <summary>
        /// スクリプトフォルダ名エラー表示
        /// </summary>
        public string ScriptDirNameError
        {
            get { return _scriptDirNameError; }
            set { SetProperty(ref _scriptDirNameError, value); }
        }

        private string _appendFileError;
        /// <summary>
        /// 追加ファイルエラー表示
        /// </summary>
        public string AppendFileError
        {
            get { return _appendFileError; }
            set { SetProperty(ref _appendFileError, value); }
        }

        private string _nicoVideoIDError;
        /// <summary>
        /// ニコニコ動画IDエラー表示
        /// </summary>
        public string NicoVideoIDError
        {
            get { return _nicoVideoIDError; }
            set { SetProperty(ref _nicoVideoIDError, value); }
        }
        #endregion

        #region ファイルタイプコンボボックスの設定
        private KeyValuePair<InstallFileType, string> _fileTypeSelectItem;
        /// <summary>
        /// ファイルタイプの選択値
        /// </summary>
        public KeyValuePair<InstallFileType, string> FileTypeSelectItem
        {
            get { return _fileTypeSelectItem; }
            set
            {
                SetProperty(ref _fileTypeSelectItem, value);
                if (_fileTypeSelectItem.Key == InstallFileType.Plugin)
                {
                    IsScriptDirNameVisible = Visibility.Collapsed;
                }
                else
                {
                    IsScriptDirNameVisible = Visibility.Visible;
                }
            }
        }
        /// <summary>
        /// ファイルタイプの選択文字列
        /// </summary>
        public string FileTypeSelectValue { get; set; }
        private Dictionary<InstallFileType, string> _fileTypeDic;
        /// <summary>
        /// ファイルタイプと文字列の辞書
        /// </summary>
        public Dictionary<InstallFileType, string> FileTypeDic
        {
            get { return _fileTypeDic; }
        }
        #endregion

        public InstallItemEditViewModel()
        {
            _fileTypeDic = new Dictionary<InstallFileType, string>()
            {
                { InstallFileType.Script, InstallItem.GetFileTypeString(InstallFileType.Script) },
                { InstallFileType.Plugin, InstallItem.GetFileTypeString(InstallFileType.Plugin) },
            };
            _acceptCommand = new DelegateCommand(
                (window) =>
                {
                    OnAccept(EditType);
                    OnExit((Window)window);
                },
                _ =>
                {
                    return CheckInputData();
                });
            InitializeItemValue();
        }


        /// <summary>
        /// インストール項目の初期化
        /// </summary>
        private void InitializeItemValue()
        {
            Name = "";
            URL = "";
            FileName = "";
            FileTypeSelectItem = FileTypeDic.First();
            FileType = FileTypeDic.First(x => x.Value == FileTypeDic[FileTypeSelectItem.Key]).Key;
            Version = "";
            ScriptDirName = "";
            AppendFile = "";
            NicoVideoID = "";
            IsScriptDirNameVisible = Visibility.Visible;
        }

        /// <summary>
        /// 変更するインストール項目を設定
        /// </summary>
        /// <param name="item">変更するインストール項目</param>
        public void SetModifyItem(InstallItem item)
        {
            _installItem = item;
            Name = item.Name;
            URL = item.URL;
            FileName = item.FileName;
            FileTypeSelectItem = FileTypeDic.First(x => x.Value == FileTypeDic[item.FileType]);
            Version = item.Version;
            ScriptDirName = item.ScriptDirName;
            AppendFile = item.InstallFile;
            NicoVideoID = item.NicoVideoID;
        }

        /// <summary>
        /// 追加/変更処理
        /// </summary>
        /// <param name="editType">編集の種類</param>
        private void OnAccept(EditShowType editType)
        {
            InstallFileType type = FileTypeDic.First(x => x.Value == FileTypeDic[FileTypeSelectItem.Key]).Key;
            switch (editType)
            {
                case EditShowType.Add:
                    InstallItemList.AddInstallItem(InstallItemList.RepoType.User,
                                                   new InstallItem { Name = _name, URL = _url,
                                                                     FileName = _fileName, FileType = type,
                                                                     Version = _version,
                                                                     ScriptDirName = _scriptDirName, InstallFile = _appendFile,
                                                                     NicoVideoID = _nicoVideoID });
                    break;
                case EditShowType.Modify:
                    InstallItemList.ModifyInstallItem(InstallItemList.RepoType.User, _installItem,
                                                      new InstallItem { Name = _name, URL = _url,
                                                                        FileName = _fileName, FileType = type,
                                                                        Version = _version,
                                                                        ScriptDirName = _scriptDirName, InstallFile = _appendFile,
                                                                        NicoVideoID = _nicoVideoID });
                    break;
            }
        }

        #region 終了処理関係
        public Func<bool> ClosingCallback
        {
            get { return OnExit; }
        }

        private bool OnExit(Window window)
        {
            OnExit();
            window.Close();
            return true;
        }

        private bool OnExit()
        {
            InitializeItemValue();

            return true;
        }
        #endregion

        #region エラー処理関係
        /// <summary>
        /// 入力データのチェック
        /// </summary>
        /// <returns></returns>
        private bool CheckInputData()
        {
            CheckNameData();
            CheckUrlData();
            CheckFileNameData();
            CheckScriptDirNameData();
            CheckVersionData();
            CheckAppendFileData();
            CheckNicoVideoIDData();

            if (!string.IsNullOrEmpty(NameError) || !string.IsNullOrEmpty(URLError) ||
                !string.IsNullOrEmpty(FileNameError) || !string.IsNullOrEmpty(ScriptDirNameError) ||
                !string.IsNullOrEmpty(VersionError) || !string.IsNullOrEmpty(AppendFileError) ||
                !string.IsNullOrEmpty(NicoVideoIDError))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Nameデータのエラーチェック
        /// </summary>
        private void CheckNameData()
        {
            NameError = "";
            if (string.IsNullOrWhiteSpace(Name))
            {
                NameError = "名前を入力してください";
            }
            else if (CheckDuplicate(DuplicateType.Name, Name))
            {
                NameError = "同じ名前は登録できません";
            }
            else if (CheckStartAndEndWhiteSpace(Name))
            {
                NameError = "前後に空白は使用できません";
            }
        }

        /// <summary>
        /// URLデータのエラーチェック
        /// </summary>
        private void CheckUrlData()
        {
            URLError = "";
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            else if (CheckDuplicate(DuplicateType.URL, URL))
            {
                URLError = "同じURLは登録できません";
            }
            try
            {
                Uri uri = new Uri(URL);
            }
            catch
            {
                URLError = "無効なURLです";
            }
        }

        /// <summary>
        /// FileNameデータのエラーチェック
        /// </summary>
        private void CheckFileNameData()
        {
            FileNameError = "";
            if (CheckStartAndEndWhiteSpace(FileName))
            {
                FileNameError = "ファイル名を入力してください";
            }
            else if (CheckDuplicate(DuplicateType.FileName, FileName))
            {
                FileNameError = "同じファイル名は登録できません";
            }
            else if (CheckInvalidChar(FileName))
            {
                FileNameError = "\\ / : * ? \" < > | は使用できません";
            }
            else if (CheckFileNameAndExtension(FileName))
            {
                FileNameError = "無効なファイル名です";
            }
            else if (CheckStartAndEndWhiteSpace(FileName))
            {
                FileNameError = "前後に空白は使用できません";
            }
        }

        /// <summary>
        /// ScriptDirNameデータのエラーチェック
        /// </summary>
        private void CheckScriptDirNameData()
        {
            ScriptDirNameError = "";
            if (FileTypeSelectItem.Key == InstallFileType.Plugin)
            {
                return;
            }
            if (!ScriptDirName.Equals("") && string.IsNullOrWhiteSpace(ScriptDirName))
            {
                ScriptDirNameError = "空白は使用できません";
            }
            else if (CheckStartAndEndWhiteSpace(ScriptDirName))
            {
                ScriptDirNameError = "前後に空白は使用できません";
            }
            else if (CheckInvalidChar(ScriptDirName))
            {
                ScriptDirNameError = "\\ / : * ? \" < > | は使用できません";
            }
        }

        /// <summary>
        /// Versionデータのエラーチェック
        /// </summary>
        private void CheckVersionData()
        {
            VersionError = "";
            if (string.IsNullOrWhiteSpace(Version))
            {
                VersionError = "バージョンを入力してください";
            }
            else if(CheckStartAndEndWhiteSpace(Version))
            {
                VersionError = "前後に空白は使用できません";
            }
        }

        /// <summary>
        /// AppendFileデータのエラーチェック
        /// </summary>
        private void CheckAppendFileData()
        {
            AppendFileError = "";
            if (string.IsNullOrEmpty(AppendFile))
            {
                return;
            }
            if (CheckStartAndEndWhiteSpace(AppendFile))
            {
                AppendFileError = "前後に空白は使用できません";
            }
            string[] array = AppendFile.Split(',');
            HashSet<string> duplicate = new HashSet<string>();
            foreach (string str in array)
            {
                if (CheckFileNameAndExtension(str))
                {
                    AppendFileError = "無効なファイル名です";
                    break;
                }
                else if(!duplicate.Add(str.Trim()))
                {
                    AppendFileError = $"ファイル名が重複しています ({str.Trim()})";
                    break;
                }
                else if (CheckInvalidChar(str.Trim()))
                {
                    if (Path.GetFileNameWithoutExtension(str.Trim()).Equals("*"))
                    {
                        continue;
                    }
                    AppendFileError = "\\ / : * ? \" < > | は使用できません";
                    break;
                }
            }
        }

        /// <summary>
        /// NicoVideoIDデータのエラーチェック
        /// </summary>
        private void CheckNicoVideoIDData()
        {
            NicoVideoIDError = "";
            if (string.IsNullOrEmpty(NicoVideoID))
            {
                return;
            }
            if (CheckStartAndEndWhiteSpace(NicoVideoID))
            {
                NicoVideoIDError = "前後を空白は使用できません";
            }
            else if ((NicoVideoID.IndexOf("sm") < 0) || !ulong.TryParse(NicoVideoID.Substring(2), out _))
            {
                NicoVideoIDError = "有効なIDは、smから始まり、以降は数字です";
            }
        }

        /// <summary>
        /// 重複チェックの項目選択
        /// </summary>
        private enum DuplicateType
        {
            /// <summary>
            /// 項目名
            /// </summary>
            Name,
            /// <summary>
            /// URL
            /// </summary>
            URL,
            /// <summary>
            /// ファイル名
            /// </summary>
            FileName,
        };

        /// <summary>
        /// 重複チェック
        /// </summary>
        /// <param name="type">重複チェック対象</param>
        /// <param name="str">チェック対象</param>
        /// <returns></returns>
        private bool CheckDuplicate(DuplicateType type, string str)
        {
            switch(type)
            {
                case DuplicateType.Name:
                    if (((EditShowType.Add == EditType) && InstallItemList.CheckDuplicateName(InstallItemList.RepoType.User, str)) ||
                        ((EditShowType.Modify == EditType) && (_installItem.Name != str) && InstallItemList.CheckDuplicateName(InstallItemList.RepoType.User, str)))
                    {
                        return true;
                    }
                    break;
                case DuplicateType.URL:
                    if (((EditShowType.Add == EditType) && InstallItemList.CheckDuplicateURL(InstallItemList.RepoType.User, str)) ||
                        ((EditShowType.Modify == EditType) && (_installItem.URL != str) && InstallItemList.CheckDuplicateURL(InstallItemList.RepoType.User, str)))
                    {
                        return true;
                    }
                    break;
                case DuplicateType.FileName:
                    if (((EditShowType.Add == EditType) && InstallItemList.CheckDuplicateFileName(InstallItemList.RepoType.User, str)) ||
                        ((EditShowType.Modify == EditType) && (_installItem.FileName != str) && InstallItemList.CheckDuplicateFileName(InstallItemList.RepoType.User, str)))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// ファイル/ディレクトリ名に使用できない文字がないかチェック
        /// </summary>
        /// <param name="str">チェック対象</param>
        /// <returns></returns>
        private bool CheckInvalidChar(string str)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (str.IndexOfAny(invalidChars) != -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 有効なファイル名/拡張子かチェック
        /// </summary>
        /// <param name="str">チェック対象</param>
        /// <returns></returns>
        private bool CheckFileNameAndExtension(string str)
        {
            if (Path.GetFileNameWithoutExtension(str).Equals("") || Path.GetExtension(str).Equals(""))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 前後のスペースチェック
        /// </summary>
        /// <param name="str">チェック対象</param>
        /// <returns></returns>
        private bool CheckStartAndEndWhiteSpace(string str)
        {
            string trim_name = str.Trim();
            if (str.Length != trim_name.Length)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
