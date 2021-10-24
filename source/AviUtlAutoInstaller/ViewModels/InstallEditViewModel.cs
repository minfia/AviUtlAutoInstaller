using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using AviUtlAutoInstaller.Models.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AviUtlAutoInstaller.ViewModels
{
    /// <summary>
    /// コマンドの選択タイプ
    /// </summary>
    public enum SelectCommandType
    {
        Button,
        DataGrid,
    }

    class InstallEditViewModel : NotificationObject
    {
        private enum InstallResult
        {
            OK,
            NG,
            NotDownload,
            DownloadFailed,
            NotSupportInstall,
        }

        private enum UninstallResult
        {
            OK,
            NG,
            IsMain,
        }

        #region ユーザータブ
        private DelegateCommand _openUserRepoCommand;
        private DelegateCommand _saveUserRepoCommand;
        private DelegateCommand _addItemCommand;
        private DelegateCommand _modifyItemCommand;
        private DelegateCommand<SelectCommandType> _deleteItemCommand;

        public DelegateCommand OpenUserRepoCommand { get => _openUserRepoCommand; }
        public DelegateCommand SaveUserRepoCommand { get => _saveUserRepoCommand; }
        public DelegateCommand AddItemCommand { get => _addItemCommand; }
        public DelegateCommand ModifyItemCommand { get => _modifyItemCommand; }
        public DelegateCommand<SelectCommandType> DeleteItemCommand { get => _deleteItemCommand; }

        private Action<bool, string> _openUserRepoCallback;
        public Action<bool, string> OpenUserRepoCallback
        {
            get { return _openUserRepoCallback; }
            set { SetProperty(ref _openUserRepoCallback, value); }
        }

        private Action<bool, string> _saveUserRepoCallback;
        public Action<bool, string> SaveUserRepoCallback
        {
            get { return _saveUserRepoCallback; }
            set { SetProperty(ref _saveUserRepoCallback, value); }
        }

        #endregion

        #region コンテキストメニュー
        private DelegateCommand _singleInstallCommand;
        private DelegateCommand _singleUninstallCommand;
        private DelegateCommand _itemPropertyCommand;
        public DelegateCommand SingleInstallCommand { get => _singleInstallCommand; }
        public DelegateCommand SingleUninstallCommand { get => _singleUninstallCommand; }
        public DelegateCommand ItemPropertyCommand { get => _itemPropertyCommand; }
        private Visibility _isVisiblePreRepoInstallContextMenu = Visibility.Collapsed;
        public Visibility IsVisiblePreRepoInstallContextMenu
        { 
            get { return _isVisiblePreRepoInstallContextMenu; }
            private set { SetProperty(ref _isVisiblePreRepoInstallContextMenu, value); }
        }
        private Visibility _isVisiblePreRepoUninstallContextMenu = Visibility.Collapsed;
        public Visibility IsVisiblePreRepoUninstallContextMenu
        { 
            get { return _isVisiblePreRepoUninstallContextMenu; }
            private set { SetProperty(ref _isVisiblePreRepoUninstallContextMenu, value); }
        }
        private ItemPropertyViewModel _itemPropertyViewModel = new ItemPropertyViewModel();
        public ItemPropertyViewModel ItemPropertyViewModel { get { return _itemPropertyViewModel; } }
        private Action<bool> _itemPropertyViewCallback;
        public Action<bool> ItemPropertyViewCallback
        {
            get { return _itemPropertyViewCallback; }
            private set { SetProperty(ref _itemPropertyViewCallback, value); }
        }
        private void OnItemPropertyView(bool result)
        {
            ItemPropertyViewCallback = null;
        }
        #endregion

        #region InstallItemEditViewの設定
        private InstallItemEditViewModel _installItemEditViewModel = new InstallItemEditViewModel();
        public InstallItemEditViewModel InstallItemEditViewModel { get { return _installItemEditViewModel; } }
        private Action<bool> _installItemEditViewCallback;
        public Action<bool> InstallItemEditViewCallback
        {
            get { return _installItemEditViewCallback; }
            private set { SetProperty(ref _installItemEditViewCallback, value); }
        }
        private void OnInstallItemEditView(bool result)
        {
            InstallItemEditViewCallback = null;
        }
        #endregion

        private InstallItemList _installItemList;

        private Queue<InstallItem> downloadQueue = new Queue<InstallItem>();
        private Dictionary<InstallItem, string> downloadFailedList = new Dictionary<InstallItem, string>();

        #region プリインストールアイテム
        public ObservableCollection<InstallItem> PreInstallList { get; }
        public CollectionViewSource PreInstallFilterList { get; private set; }
        private InstallItem _preSelectItem;
        public InstallItem PreSelectItem
        {
            get { return _preSelectItem; }
            set { SetProperty(ref _preSelectItem, value); }
        }
        private bool _preSelectAllCheck;
        /// <summary>
        /// プリインストールアイテム全選択/解除
        /// </summary>
        public bool PreSelectAllCheck
        {
            get { return _preSelectAllCheck; }
            set
            {
                SetProperty(ref _preSelectAllCheck, value);
                foreach (InstallItem item in PreInstallList)
                {
                    if (item.CommandName == "aviutl" || item.CommandName == "exedit")
                    {
                        continue;
                    }
                    item.IsSelect = value;
                }
            }
        }
        #endregion

        #region ユーザーアイテム
        public ObservableCollection<InstallItem> UserInstallList { get; }
        public CollectionViewSource UserInstallFilterList { get; private set; }
        private InstallItem _userInstallItem;
        public InstallItem UserSelectItem
        {
            get { return _userInstallItem; }
            set { SetProperty(ref _userInstallItem, value); }
        }
        private bool _userSelectAllCheck;
        /// <summary>
        /// ユーザーアイテム全選択/解除
        /// </summary>
        public bool UserSelectAllCheck
        {
            get { return _userSelectAllCheck; }
            set
            {
                SetProperty(ref _userSelectAllCheck, value);
                foreach (InstallItem item in UserInstallList)
                {
                    item.IsSelect = value;
                }
            }
        }
        #endregion

        #region フィルタ
        /// <summary>
        /// フィルタを適応する種類
        /// </summary>
        private enum FilterType
        {
            Status,
            Name,
            MakerName,
            ScriptDirName,
            FileType,
            Section,
        }

        private Dictionary<int, InstallItemList.RepoType> _selectTab = new Dictionary<int, InstallItemList.RepoType>()
        {
            { 0, InstallItemList.RepoType.Pre },
            { 1, InstallItemList.RepoType.User },
        };

        private int _tabControlSelectIndex = 0;
        /// <summary>
        /// 選択されたタブのIndex
        /// </summary>
        public int TabControlSelectIndex
        {
            get { return _tabControlSelectIndex; }
            set
            {
                SetProperty(ref _tabControlSelectIndex, value);
                // 選択されたタブのIndexに合わせてフィルタ値を復元
                NameFilter = _nameFilterList[value];
                ScriptDirNameFilter = _scriptDirNameFilterList[value];
                if (_selectTab[value] == InstallItemList.RepoType.Pre)
                {
                    IsFileTypeFilterVisible = Visibility.Collapsed;
                    IsSectionFilterVisible = Visibility.Visible;
                    IsMakerFilterVisible = Visibility.Visible;
                }
                else
                {
                    IsFileTypeFilterVisible = Visibility.Visible;
                    IsSectionFilterVisible = Visibility.Collapsed;
                    IsMakerFilterVisible = Visibility.Collapsed;
                }
                FileTypeFilterSelectIndex = _fileTypeFilterList[value];
                SectionFilterSelectIndex = _sectionFilterList[value];
                MakerFilterSelectIndex = _makerFilterList[value];
            }
        }

        #region 状態フィルタ
        private readonly Dictionary<int, string> _statusFilter = new Dictionary<int, string>()
        {
            { 0, "全て" },
            { 1, "インストール済み" },
            { 2, "未インストール" },
            { 3, "選択された項目" },
        };
        private readonly int[] _statusFilterList = new int[(int)InstallItemList.RepoType.MAX] { 0, 0 };
        public Dictionary<int, string> StatusFilter { get { return _statusFilter; } }
        private string _statusSelectValue;
        private int _statusFilterSelectIndex;
        public int StatusFilterSelectIndex
        {
            get { return _statusFilterSelectIndex; }
            set
            {
                SetProperty(ref _statusFilterSelectIndex, value);
                string[] itemNameList = StatusFilter.Values.ToArray();
                _statusSelectValue = itemNameList[value];
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.Status, value);
            }
        }
        /// <summary>
        /// 状態に合わせたインストール項目の表示状態
        /// </summary>
        /// <param name="installItem"></param>
        /// <param name="status"></param>
        /// <returns>true: 表示, false: 非表示</returns>
        private bool IsStatusFilterVisible(InstallItem installItem, string status)
        {
            /*
             * 表示条件
             * 1. "全て" -> 全状態
             * 2. "インストール済み" -> InstallItem.IsInstalled = true
             * 3. "未インストール" -> InstallItem.IsInstalled = false
             * 4. "選択された項目" -> InstallItem.IsSelect = true
             */
            if (StatusFilter[0] == status) return true;
            if (StatusFilter[1] == status && installItem.IsInstalled) return true;
            if (StatusFilter[2] == status && !installItem.IsInstalled) return true;
            if (StatusFilter[3] == status && installItem.IsSelect) return true;
            return false;
        }
        #endregion

        #region 項目名のフィルタ
        /// <summary>
        /// 各Listの項目名フィルタ値保持用
        /// </summary>
        private readonly string[] _nameFilterList = new string[(int)InstallItemList.RepoType.MAX] { "", "" };
        private string _nameFilter = "";
        /// <summary>
        /// 項目名のフィルタ値
        /// </summary>
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                SetProperty(ref _nameFilter, value);
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.Name, value);
            }
        }
        #endregion

        #region 製作者のフィルタ
        /// <summary>
        /// コンボボックスに表示する内容
        /// </summary>
        public Dictionary<int, string> MakerFilter { get; } = InstallItem.MakerTypeDic;
        private string _makerSelectValue;
        /// <summary>
        /// 各Listの製作者フィルタ値保持用
        /// </summary>
        private readonly int[] _makerFilterList = new int[(int)InstallItemList.RepoType.MAX] { 0, 0 };
        private int _makerFilterSelectIndex;
        /// <summary>
        /// 製作者のフィルタ値
        /// </summary>
        public int MakerFilterSelectIndex
        {
            get { return _makerFilterSelectIndex; }
            set
            {
                SetProperty(ref _makerFilterSelectIndex, value);
                string[] itemNameList = MakerFilter.Values.ToArray();
                _makerSelectValue = itemNameList[value];
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.MakerName, value);
            }
        }
        private Visibility _isMakerFilterVisible = Visibility.Visible;
        /// <summary>
        /// 製作者のフィルタの表示設定
        /// </summary>
        public Visibility IsMakerFilterVisible
        {
            get { return _isMakerFilterVisible; }
            set { SetProperty(ref _isMakerFilterVisible, value); }
        }
        #endregion

        #region スクリプトフォルダ名のフィルタ
        /// <summary>
        /// 各Listのスクリプトフォルダ名フィルタ値保持用
        /// </summary>
        private readonly string[] _scriptDirNameFilterList = new string[(int)InstallItemList.RepoType.MAX] { "", "" };
        private string _scriptDirNameFilter = "";
        /// <summary>
        /// スクリプトフォルダ名のフィルタ値
        /// </summary>
        public string ScriptDirNameFilter
        {
            get { return _scriptDirNameFilter; }
            set
            {
                SetProperty(ref _scriptDirNameFilter, value);
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.ScriptDirName, value);
            }
        }
        #endregion

        #region ファイルタイプのフィルタ
        /// <summary>
        /// ファイルタイプのフィルタ用定数
        /// </summary>
        private const int FileTypeAll = int.MaxValue;
        /// <summary>
        /// コンボボックスに表示する内容
        /// </summary>
        public Dictionary<int, string> FileTypeFilter { get; } = new Dictionary<int, string>()
        {
            { FileTypeAll, "全て" },
            { (int)InstallFileType.Script, InstallItem.GetFileTypeString(InstallFileType.Script) },
            { (int)InstallFileType.Plugin, InstallItem.GetFileTypeString(InstallFileType.Plugin) },
        };
        private string _fileTypeSelectValue;
        /// <summary>
        /// 各Listのファイルタイプフィルタ値保持用
        /// </summary>
        private readonly int[] _fileTypeFilterList = new int[(int)InstallItemList.RepoType.MAX] { 0, 0 };
        private int _fileTypeFilterSelectIndex;
        /// <summary>
        /// ファイルタイプのフィルタ値
        /// </summary>
        public int FileTypeFilterSelectIndex
        {
            get { return _fileTypeFilterSelectIndex; }
            set
            {
                SetProperty(ref _fileTypeFilterSelectIndex, value);
                string[] itemNameList = FileTypeFilter.Values.ToArray();
                _fileTypeSelectValue = itemNameList[value];
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.FileType, value);
            }
        }
        private Visibility _IsFileTypeFilterVisible = Visibility.Collapsed;
        /// <summary>
        /// ファイルタイプのフィルタの表示設定
        /// </summary>
        public Visibility IsFileTypeFilterVisible
        {
            get { return _IsFileTypeFilterVisible; }
            set { SetProperty(ref _IsFileTypeFilterVisible, value); }
        }
        #endregion

        #region ジャンルタイプのフィルタ
        /// <summary>
        /// コンボボックスに表示する内容
        /// </summary>
        public Dictionary<int, string> SectionFilter { get; } = InstallItem.SectionTypeDic;
        private string _sectionSelectValue;
        /// <summary>
        /// 各Listのジャンルタイプフィルタ値保持用
        /// </summary>
        private readonly int[] _sectionFilterList = new int[(int)InstallItemList.RepoType.MAX] { 0, 0 };
        private int _sectionFilterSelectIndex;
        /// <summary>
        /// ジャンルタイプのフィルタ値
        /// </summary>
        public int SectionFilterSelectIndex
        {
            get { return _sectionFilterSelectIndex; }
            set
            {
                SetProperty(ref _sectionFilterSelectIndex, value);
                string[] itemNameList = SectionFilter.Values.ToArray();
                _sectionSelectValue = itemNameList[value];
                UpdateFilterData(_selectTab[TabControlSelectIndex], FilterType.Section, value);
            }
        }
        private Visibility _IsSectionFilterVisible = Visibility.Visible;
        /// <summary>
        /// ジャンルタイプのフィルタの表示設定
        /// </summary>
        public Visibility IsSectionFilterVisible
        {
            get { return _IsSectionFilterVisible; }
            set { SetProperty(ref _IsSectionFilterVisible, value); }
        }
        #endregion

        /// <summary>
        /// 選択されたタブに合わせてフィルタ更新
        /// </summary>
        /// <param name="selectTab"></param>
        /// <param name="filterType"></param>
        /// <param name="data"></param>
        private void UpdateFilterData(InstallItemList.RepoType selectTab, FilterType filterType, object data)
        {
            if (InstallItemList.RepoType.MAX < selectTab)
            {
                return;
            }
            switch (filterType)
            {
                case FilterType.Status:
                    _statusFilterList[(int)selectTab] = (int)data;
                    break;
                case FilterType.Name:
                    _nameFilterList[(int)selectTab] = (string)data;
                    break;
                case FilterType.MakerName:
                    _makerFilterList[(int)selectTab] = (int)data;
                    break;
                case FilterType.ScriptDirName:
                    _scriptDirNameFilterList[(int)selectTab] = (string)data;
                    break;
                case FilterType.FileType:
                    _fileTypeFilterList[(int)selectTab] = (int)data;
                    break;
                case FilterType.Section:
                    _sectionFilterList[(int)selectTab] = (int)data;
                    break;
            }
            if (InstallItemList.RepoType.Pre == selectTab)
            {
                PreInstallFilterList.View.Refresh();
            }
            else if (InstallItemList.RepoType.User == selectTab)
            {
                UserInstallFilterList.View.Refresh();
            }
        }

        /// <summary>
        /// フィルタ適用イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterEvent(object sender, FilterEventArgs e)
        {
            InstallItem installItem = e.Item as InstallItem;
            string status = StatusFilter[StatusFilter.First(x => x.Value.Contains(_statusSelectValue)).Key];
            int fileType = FileTypeFilter.First(x => x.Value.Contains(_fileTypeSelectValue)).Key;
            string sectionType = SectionFilter[SectionFilter.First(x => x.Value.Contains(_sectionSelectValue)).Key];
            string makerName = MakerFilter[MakerFilter.First(x => x.Value.Contains(_makerSelectValue)).Key];
            if (installItem.Name.Contains(NameFilter) && installItem.ScriptDirName.Contains(ScriptDirNameFilter) &&
                IsStatusFilterVisible(installItem, status) &&
                ((int)installItem.FileType == fileType || FileTypeAll == fileType) &&
                (installItem.SectionType == sectionType || SectionFilter[0] == sectionType) &&
                (installItem.MakerName == makerName || MakerFilter[0] == makerName))
            {
                // 表示
                e.Accepted = true;
            }
            else
            {
                // 非表示
                e.Accepted = false;
            }
        }
        #endregion

        private DelegateCommand _downloadCommand;
        public DelegateCommand DownloadCommand { get => _downloadCommand; }
        private int _downloadValue = 0;
        public int DownloadValue
        {
            get { return _downloadValue; }
            private set { SetProperty(ref _downloadValue, value); }
        }

        private string _statusBarText = "";
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { SetProperty(ref _statusBarText, value); }
        }

        private bool _downloading = false;

        public InstallEditViewModel()
        {
            _installItemList = new InstallItemList();
            _statusSelectValue = StatusFilter[0];
            _fileTypeSelectValue = FileTypeFilter[FileTypeAll];
            _sectionSelectValue = SectionFilter[0];
            _makerSelectValue = MakerFilter[0];

            PreInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.Pre);
            UserInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.User);

            PreInstallFilterList = new CollectionViewSource();
            PreInstallFilterList.Source = PreInstallList;
            PreInstallFilterList.Filter += FilterEvent;
            UserInstallFilterList = new CollectionViewSource();
            UserInstallFilterList.Source = UserInstallList;
            UserInstallFilterList.Filter += FilterEvent;

            _openUserRepoCommand = new DelegateCommand(
                _ =>
                {
                    OpenUserRepoCallback = OnOpenUserRepoCallback;
                });
            _saveUserRepoCommand = new DelegateCommand(
                _ =>
                {
                    SaveUserRepoCallback = OnSaveUserRepoCallback;
                });
            _addItemCommand = new DelegateCommand(
                _ =>
                {
                    _installItemEditViewModel.Title = "追加";
                    _installItemEditViewModel.EditType = InstallItemEditViewModel.EditShowType.Add;
                    InstallItemEditViewCallback = OnInstallItemEditView;
                });
            _modifyItemCommand = new DelegateCommand(
                _ =>
                {
                    if (UserSelectItem == null)
                    {
                        return;
                    }
                    _installItemEditViewModel.Title = "変更";
                    _installItemEditViewModel.EditType = InstallItemEditViewModel.EditShowType.Modify;
                    _installItemEditViewModel.SetModifyItem(UserSelectItem);
                    InstallItemEditViewCallback = OnInstallItemEditView;
                });
            _deleteItemCommand = new DelegateCommand<SelectCommandType>(
                (type) =>
                {
                    List<InstallItem> deleteItemList = new List<InstallItem>();
                    if (SelectCommandType.Button == type)
                    {
                        bool isAllfalse = true;
                        foreach (InstallItem item in UserInstallList)
                        {
                            if (item.IsSelect)
                            {
                                isAllfalse = false;
                                deleteItemList.Add(item);
                            }
                        }
                        if (isAllfalse)
                        {
                            return;
                        }
                    }
                    else if (SelectCommandType.DataGrid == type)
                    {
                        deleteItemList.Add(UserSelectItem);
                    }
                    
                    string buildDisplyItemList = "";
                    foreach (InstallItem deleteItem in deleteItemList)
                    {
                        buildDisplyItemList += $"{deleteItem.Name}\n";
                    }
                    if (MessageBox.Show($"{buildDisplyItemList}を削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    InstallItemList.DeleteInstallItem(InstallItemList.RepoType.User, deleteItemList);
                });
            _downloadCommand = new DelegateCommand(async _ => await OnDownload());
            _singleInstallCommand = new DelegateCommand(
                async _ =>
                {
                    if (SysConfig.IsInstalledAviUtl && MessageBox.Show("インストールしますか？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var res = await OnInstall();
                        string message = "インストールが完了しました。";
                        string title = "情報";
                        var image = MessageBoxImage.Information;
                        switch (res)
                        {
                            case InstallResult.NG:
                                message = "インストールに失敗しました。";
                                image = MessageBoxImage.Error;
                                break;
                            case InstallResult.DownloadFailed:
                                message = "ファイルのダウンロードに失敗しました。";
                                image = MessageBoxImage.Error;
                                break;
                            case InstallResult.NotDownload:
                                message = "事前にファイルのダウンロードが必要です。";
                                image = MessageBoxImage.Error;
                                break;
                            case InstallResult.NotSupportInstall:
                                message = "このバージョンではインストールに対応していません。";
                                image = MessageBoxImage.Error;
                                break;
                            default:
                                break;
                        }

                        MessageBox.Show(message, title, MessageBoxButton.OK, image);
                    }
                });
            _singleUninstallCommand = new DelegateCommand(
                async _ =>
                {
                    if (SysConfig.IsInstalledAviUtl && MessageBox.Show("アンインストールしますか？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var res = await OnUninstall();
                        string message = "アンインストールが完了しました。";
                        string title = "情報";
                        var image = MessageBoxImage.Information;
                        switch (res)
                        {
                            case UninstallResult.NG:
                                message = "アンインストールに失敗しました。";
                                image = MessageBoxImage.Error;
                                break;
                            case UninstallResult.IsMain:
                                message = "基本構成のためアンインストールできません。";
                                image = MessageBoxImage.Error;
                                break;
                        }

                        MessageBox.Show(message, title, MessageBoxButton.OK, image);
                    }
                });
            _itemPropertyCommand = new DelegateCommand(
                _ =>
                {
                    if (PreSelectItem == null)
                    {
                        return;
                    }
                    _itemPropertyViewModel.Title = PreSelectItem.Name;
                    _itemPropertyViewModel.SetInstallItem(PreSelectItem);
                    ItemPropertyViewCallback = OnItemPropertyView;
                });
        }

        private void InitializeValue()
        {
            PreSelectItem = null;
            UserSelectItem = null;
            TabControlSelectIndex = 0;
            _nameFilter = "";
            _scriptDirNameFilter = "";
            _fileTypeFilterSelectIndex = 0;
            for (int i = 0; i < (int)InstallItemList.RepoType.MAX; i++)
            {
                _nameFilterList[i] = "";
                _scriptDirNameFilterList[i] = "";
                _fileTypeFilterList[i] = 0;
            }
        }

        private void OnOpenUserRepoCallback(bool isOk, string filePath)
        {
            if (isOk)
            {
                try
                {
                    UserRepoFileRW userRepoFileRead = new UserRepoFileRW();

                    userRepoFileRead.FileRead(filePath);
                    UserSelectAllCheck = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    OpenUserRepoCallback = null;
                }
            }
            OpenUserRepoCallback = null;
        }

        private void OnSaveUserRepoCallback(bool isOk, string filePath)
        {
            if (isOk)
            {
                UserRepoFileRW userRepoFileWrite = new UserRepoFileRW();

                userRepoFileWrite.FileWrite(filePath);
            }
            SaveUserRepoCallback = null;
        }

        private async Task OnDownload()
        {
            _downloading = true;
            InstallItem selectItem = null;
            if (InstallItemList.RepoType.Pre == _selectTab[TabControlSelectIndex])
            {
                selectItem = PreSelectItem;
            }
            else if (InstallItemList.RepoType.User == _selectTab[TabControlSelectIndex])
            {
                selectItem = UserSelectItem;
            }

            if ((selectItem == null) || string.IsNullOrEmpty(selectItem.URL))
            {
                return;
            }

            Downloader downloader = new Downloader($"{SysConfig.CacheDirPath}");
            selectItem.DownloadExecute = false;
            downloadQueue.Enqueue(selectItem);

            if (1 < downloadQueue.Count)
            {
                return;
            }

            List<string> url = new List<string>();
            List<string> fileName = new List<string>();
            while (downloadQueue.Count != 0)
            {
                DownloadValue = 0;
                InstallItem headItem = downloadQueue.Peek();

                url.Add(headItem.URL);
                fileName.Add(headItem.DownloadFileName);

                for (int i = 0; i < headItem.ExternalFileURLList.Count; i++)
                {
                    url.Add(headItem.ExternalFileURLList[i]);
                    fileName.Add(headItem.ExternalFileList[i]);
                }

                int count = url.Count;

                Func<string, string, DownloadResult> func = new Func<string, string, DownloadResult>(downloader.DownloadStart);

                for (int i = 0; i < count; i++)
                {


                    var task = Task.Run(() => func(url[i], fileName[i]));
                    StatusBarText = $"{headItem.Name}をダウンロード中...";

                    Task updateTask = Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(1);
                            if (downloader.DownloadFileSize != 0)
                            {
                                DownloadValue = (int)((double)downloader.DownloadCompleteSize / downloader.DownloadFileSize * 100);
                            }
                        } while (task.Status != TaskStatus.RanToCompletion);
                    });

                    var res = await task;
                    Console.WriteLine($"download res: {res}");

                    string message = GetDownloadResultMessage(res);
                    headItem.DownloadExecute = true;
                    if (message != "" && i == 0)
                    {
                        downloadFailedList.Add(headItem, message);
                    }
                }

                url.Clear();
                fileName.Clear();
                downloadQueue.Dequeue();
            }

            StatusBarText = "";

            if (downloadFailedList.Count != 0)
            {
                string message = "";
                foreach (var pair in downloadFailedList)
                {
                    message += $"{pair.Key.Name}\n\t{pair.Value}\n";
                }

                MessageBox.Show(message, "ダウンロード失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                downloadFailedList.Clear();
            }
            _downloading = false;
        }

        /// <summary>
        /// ダウンロード結果に応じてメッセージボックスに出力する文字列を返す
        /// </summary>
        /// <param name="result">ダウンロード結果のメッセージ</param>
        /// <returns></returns>
        private string GetDownloadResultMessage(DownloadResult result)
        {
            string message = "";
            switch (result)
            {
                case DownloadResult.Connection404Error:
                    message = "無効なダウンロード先です、URLを再設定してください。";
                    break;
                case DownloadResult.ConnectionTimeoutError:
                    message = "タイムアウトしました、再度ダウンロードしてください。";
                    break;
                case DownloadResult.ConnectionError:
                    message = "接続に失敗しました。\n再度ダウンロードしてください。";
                    break;
                case DownloadResult.DownloadSizeGetError:
                case DownloadResult.DownloadFileNameGetError:
                case DownloadResult.DownloadError:
                    message = "ダウンロードに失敗しました。";
                    break;
                case DownloadResult.GDriveVirus:
                    message = "ファイルがウィルスに感染している可能性があるため、ダウンロードが出来ませんでした。";
                    break;
                default:
                    break;
            }

            return message;
        }

        /// <summary>
        /// 単体インストール処理
        /// </summary>
        /// <returns></returns>
        private async Task<InstallResult> OnInstall()
        {
            InstallItem selectItem = null;
            if (InstallItemList.RepoType.Pre == _selectTab[TabControlSelectIndex])
            {
                selectItem = PreSelectItem;
            }
            else if (InstallItemList.RepoType.User == _selectTab[TabControlSelectIndex])
            {
                selectItem = UserSelectItem;
            }

            // ダウンロード
            if (selectItem == null)
            {
                return InstallResult.NG;
            }
            if (!File.Exists($"{SysConfig.CacheDirPath}\\{selectItem.DownloadFileName}") && string.IsNullOrEmpty(selectItem.URL))
            {
                return InstallResult.NotDownload;
            }
            else if (!File.Exists($"{SysConfig.CacheDirPath}\\{selectItem.DownloadFileName}"))
            {
                await OnDownload();
            }
            if (File.Exists($"{SysConfig.CacheDirPath}\\{selectItem.DownloadFileName}"))
            {
                selectItem.IsDownloadCompleted = true;
            }
            if (!selectItem.IsDownloadCompleted)
            {
                return InstallResult.DownloadFailed;
            }

            // インストール
            Directory.CreateDirectory(SysConfig.InstallExpansionDir);
            {
                InstallItemList installItemList = new InstallItemList();
                var func = new Func<InstallItem, string[], bool>(InstallItem.Install);
                var installFileList = installItemList.GenerateInstalList(_selectTab[TabControlSelectIndex], selectItem);
                var task = Task.Run(() => func(selectItem, installFileList.ToArray()));
                var res = await task;
                selectItem.IsInstallCompleted = res;
            }
            if (!selectItem.IsInstallCompleted)
            {
                return (selectItem.IsSpecialItem) ? InstallResult.NotSupportInstall : InstallResult.NG;
            }
            if (selectItem.IsInstallCompleted)
            {
                if ("sm".Length < selectItem.NicoVideoID.Length)
                {
                    ContentsTreeRW.AddContents(selectItem.NicoVideoID);
                    ContentsTreeRW contentsTreeRW = new ContentsTreeRW();
                    contentsTreeRW.Write($"{SysConfig.InstallRootPath}");
                }

                selectItem.IsInstallCompleted = false;
                InstallProfileRW.AddContents(selectItem);
                InstallProfileRW installProfileRW = new InstallProfileRW();
                installProfileRW.FileWrite($"{SysConfig.InstallRootPath}");
                installProfileRW.FileRead($"{SysConfig.InstallRootPath}\\{InstallProfileRW.ReloadFileName}");
            }

            if (!string.IsNullOrWhiteSpace(selectItem.ExternalFile))
            {
                var exFunc = new Func<string[], bool>(InstallItem.ExternalInstall);
                var exTask = Task.Run(() => exFunc(selectItem.ExternalFileList.ToArray()));
                var res = await exTask;
            }
            Directory.Delete(SysConfig.InstallExpansionDir, true);

            return InstallResult.OK;
        }

        private async Task<UninstallResult> OnUninstall()
        {
            InstallItem selectItem = null;
            if (InstallItemList.RepoType.Pre == _selectTab[TabControlSelectIndex])
            {
                selectItem = PreSelectItem;
            }
            else if (InstallItemList.RepoType.User == _selectTab[TabControlSelectIndex])
            {
                selectItem = UserSelectItem;
            }

            if (selectItem == null)
            {
                return UninstallResult.NG;
            }

            if (selectItem.FileType == InstallFileType.Main || selectItem.CommandName.Equals("exedit"))
            {
                return UninstallResult.IsMain;
            }

            {
                var func = new Func<InstallItem, bool>(InstallItem.Uninstall);
                var task = Task.Run(() => func(selectItem));
                var res = await task;
                selectItem.IsSelect = false;

                if (res)
                {
                    if ("sm".Length < selectItem.NicoVideoID.Length)
                    {
                        ContentsTreeRW.DeleteContents(selectItem.NicoVideoID);
                        ContentsTreeRW contentsTreeRW = new ContentsTreeRW();
                        contentsTreeRW.Write($"{SysConfig.InstallRootPath}");
                    }

                    InstallProfileRW.DeleteContents(selectItem);
                    InstallProfileRW installProfileRW = new InstallProfileRW();
                    installProfileRW.FileWrite($"{SysConfig.InstallRootPath}");
                    installProfileRW.FileRead($"{SysConfig.InstallRootPath}\\{InstallProfileRW.ReloadFileName}");
                }
                else
                {
                    return UninstallResult.NG;
                }
            }

            return UninstallResult.OK;
        }

        public Func<bool> ClosingCallback
        {
            get { return OnExit; }
        }

        private bool OnExit()
        {
            if (_downloading)
            {
                return false;
            }
            InitializeValue();

            return true;
        }

        public void UnForcus(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.Captured == null)
            {
                if (InstallItemList.RepoType.Pre == _selectTab[TabControlSelectIndex])
                {
                    PreSelectItem = null;
                }
                else if (InstallItemList.RepoType.User == _selectTab[TabControlSelectIndex])
                {
                    UserSelectItem = null;
                }
            }
        }

        public void VisibleSettingContextMenu(object sender, EventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            InstallItem item = (InstallItem)grid.CurrentItem;
            if ((item == null) || !SysConfig.IsInstalledAviUtl)
            {
                return;
            }
            IsVisiblePreRepoInstallContextMenu = item.IsInstalled ? Visibility.Collapsed : Visibility.Visible;
            IsVisiblePreRepoUninstallContextMenu = item.IsInstalled ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
