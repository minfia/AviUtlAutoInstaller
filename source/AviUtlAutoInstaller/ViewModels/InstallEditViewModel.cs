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
        private DelegateCommand _addItemCommand;
        private DelegateCommand _modifyItemCommand;
        private DelegateCommand _deleteItemCommand;

        public DelegateCommand AddItemCommand { get => _addItemCommand; }
        public DelegateCommand ModifyItemCommand { get => _modifyItemCommand; }
        public DelegateCommand DeleteItemCommand { get => _deleteItemCommand; }
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
        #endregion

        #region ユーザーアイテム
        public ObservableCollection<InstallItem> UserInstallList { get; }
        private InstallItem _userInstallItem;
        public InstallItem UserSelectItem
        {
            get { return _userInstallItem; }
            set { SetProperty(ref _userInstallItem, value); }
        }
        #endregion

        public InstallEditViewModel()
        {
            _installItemList = new InstallItemList();
            PreInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.Pre);
            UserInstallList = _installItemList.GetInstalItemList(InstallItemList.RepoType.User);
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
   }
}
