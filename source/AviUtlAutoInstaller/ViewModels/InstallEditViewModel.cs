using AviUtlAutoInstaller.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AviUtlAutoInstaller.ViewModels
{
    class InstallEditViewModel : NotificationObject
    {
        #region ユーザータブ
        private DelegateCommand _openUserRepoCommand;
        private DelegateCommand _saveUserRepoCommand;
        private DelegateCommand _addItemCommand;
        private DelegateCommand _modifyItemCommand;
        private DelegateCommand _deleteItemCommand;

        public DelegateCommand OpenUserRepoCommand { get => _openUserRepoCommand; }
        public DelegateCommand SaveUserRepoCommand { get => _saveUserRepoCommand; }
        public DelegateCommand AddItemCommand { get => _addItemCommand; }
        public DelegateCommand ModifyItemCommand { get => _modifyItemCommand; }
        public DelegateCommand DeleteItemCommand { get => _deleteItemCommand; }

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
            Name,
            ScriptDirName,
            FileType,
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
                }
                else
                {
                    IsFileTypeFilterVisible = Visibility.Visible;
                }
                FileTypeFilterSelectIndex = _fileTypeFilterList[value];
            }
        }


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

        #region スクリプトフォルダ名のフィルタ
        /// <summary>
        /// 各Listのスクリプトフォルダ名フィルタ値保持用
        /// </summary>
        private readonly string[] _scriptDirNameFilterList = new string[(int)InstallItemList.RepoType.MAX] { "", "" };
        private string _scriptDirNameFilter = "";
        /// <summary>
        /// スクリプトフォルダ名のフィルタ値w
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
                case FilterType.Name:
                    _nameFilterList[(int)selectTab] = (string)data;
                    break;
                case FilterType.ScriptDirName:
                    _scriptDirNameFilterList[(int)selectTab] = (string)data;
                    break;
                case FilterType.FileType:
                    _fileTypeFilterList[(int)selectTab] = (int)data;
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
            int fileType = FileTypeFilter.First(x => x.Value.Contains(_fileTypeSelectValue)).Key;
            if (installItem.Name.Contains(NameFilter) && installItem.ScriptDirName.Contains(ScriptDirNameFilter) &&
                ((int)installItem.FileType == fileType || FileTypeAll == fileType))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }
        #endregion

        public InstallEditViewModel()
        {
            _installItemList = new InstallItemList();
            _fileTypeSelectValue = FileTypeFilter[FileTypeAll];

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
            _deleteItemCommand = new DelegateCommand(
                _ =>
                {
                    bool isAllfalse = true;
                    List<InstallItem> deleteItemList = new List<InstallItem>();
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
        }

        private void InitializeValue()
        {
            UserSelectItem = null;
            _tabControlSelectIndex = 0;
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
                    // TODO: エラー表示
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

        public Func<bool> ClosingCallback
        {
            get { return OnExit; }
        }

        private bool OnExit()
        {
            InitializeValue();

            return true;
        }
   }
}
