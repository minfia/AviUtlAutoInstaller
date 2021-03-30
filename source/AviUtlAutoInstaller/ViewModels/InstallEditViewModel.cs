using AviUtlAutoInstaller.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    item.IsInstall = value;
                }
            }
        }
        #endregion

        #region ユーザーアイテム
        public ObservableCollection<InstallItem> UserInstallList { get; }
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
                    item.IsInstall = value;
                }
            }
        }
        #endregion

        public InstallEditViewModel()
        {
            _installItemList = new InstallItemList();
            PreInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.Pre);
            UserInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.User);

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
                    if (UserSelectItem == null)
                    {
                        return;
                    }
                    // TODO: 警告表示
                    InstallItemList.DeleteInstallItem(InstallItemList.RepoType.User, UserSelectItem);
                });
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
   }
}
