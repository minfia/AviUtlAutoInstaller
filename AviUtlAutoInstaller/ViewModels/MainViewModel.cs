using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.ViewModels
{
    class MainViewModel : NotificationObject
    {
        #region メニューバーCommand
        private DelegateCommand _openDialogInstallSettingFileCommand;
        private DelegateCommand _openDialogUserRepoFileCommand;
        private DelegateCommand _exitCommand;
        private DelegateCommand _installEditCommand;
        private DelegateCommand _updateCheckCommand;
        private DelegateCommand _aboutCommand;

        public DelegateCommand OpenDialogInstallSettingFileCommand { get => _openDialogInstallSettingFileCommand; }
        public DelegateCommand OpenDialogUserRepoFileCommand { get => _openDialogUserRepoFileCommand; }
        public DelegateCommand ExitCommand { get => _exitCommand; }
        public DelegateCommand InstallEditCommand { get => _installEditCommand; }
        public DelegateCommand UpdateCheckCommand { get => _updateCheckCommand; }
        public DelegateCommand AboutCommand { get => _aboutCommand; }

        #region ファイルダイアログ(インストール設定ファイル)の設定
        private Action<bool, string> _openDialogInstallSettingFileCallback;
        public Action<bool, string> OpenDialogInstallSettingFileCallback
        { 
            get { return _openDialogInstallSettingFileCallback; }
            private set { SetProperty(ref _openDialogInstallSettingFileCallback, value); }
        }
        private void OnOpenDialogInstallSettingFileCallback(bool isOk, string filePath)
        {
            OpenDialogInstallSettingFileCallback = null;
        }
        #endregion

        #region ファイルダイアログ(ユーザーリポジトリファイル)の設定
        private Action<bool, string> _openDialogUserRepoFileCallback;
        public Action<bool, string> OpenDialogUserRepoFileCallback
        { 
            get { return _openDialogUserRepoFileCallback; }
            private set { SetProperty(ref _openDialogUserRepoFileCallback, value); }
        }
        private void OnOpenDialogUserRepoFileCallback(bool isOk, string filePath)
        {
            OpenDialogUserRepoFileCallback = null;
        }
        #endregion

        #region アプリケーション終了の設定
        public Func<bool> ClosingCallback
        {
            get { return OnExit; }
        }
        private bool OnExit()
        {
            App.Current.Shutdown();
            return true;
        }
        #endregion

        #region インストール一覧の表示設定
        private InstallEditViewModel _instalEditViewModel = new InstallEditViewModel();
        public InstallEditViewModel InstallEditViewModel { get { return _instalEditViewModel; } }
        private Action<bool> _installEditViewCallback;
        public Action<bool> InstallEditViewCallback
        {
            get { return _installEditViewCallback; }
            private set { SetProperty(ref _installEditViewCallback, value); }
        }
        private void OnInstallEditView(bool result)
        {
            InstallEditViewCallback = null;
        }
        #endregion

        #region アップデートチェックの表示設定
        private UpdateCheckViewModel _updateCheckViewModel = new UpdateCheckViewModel();
        public UpdateCheckViewModel UpdateCheckViewModel { get { return _updateCheckViewModel; } }
        private Action<bool> _updateCheckViewCallback;
        public Action<bool> UpdateCheckViewCallback
        {
            get { return _updateCheckViewCallback; }
            private set { SetProperty(ref _updateCheckViewCallback, value); }
        }
        private void OnUpdateCheckView(bool result)
        {
            UpdateCheckViewCallback = null;
        }
        #endregion

        #region バージョン情報の表示設定
        private AboutViewModel _aboutViewModel = new AboutViewModel();
        public AboutViewModel AboutViewModel { get { return _aboutViewModel; } }
        private Action<bool> _aboutViewCallback;
        public Action<bool> AboutViewCallback
        {
            get { return _aboutViewCallback; }
            private set { SetProperty(ref _aboutViewCallback, value); }
        }
        private void OnAboutView(bool result)
        {
            AboutViewCallback = null;
        }
        #endregion
        #endregion

        private readonly DelegateCommand selectInstallDir;
        private readonly DelegateCommand installStartCommand;
        private readonly DelegateCommand installCancelCommand;

        public MainViewModel()
        {
            _openDialogInstallSettingFileCommand = new DelegateCommand(_ => OpenDialogInstallSettingFileCallback = OnOpenDialogInstallSettingFileCallback);
            _openDialogUserRepoFileCommand = new DelegateCommand(_ => OpenDialogUserRepoFileCallback = OnOpenDialogUserRepoFileCallback);
            _exitCommand = new DelegateCommand(_ => OnExit());
            _installEditCommand = new DelegateCommand(_ => InstallEditViewCallback = OnInstallEditView);
            _updateCheckCommand = new DelegateCommand(_ => UpdateCheckViewCallback = OnUpdateCheckView);
            _aboutCommand = new DelegateCommand(_ => AboutViewCallback = OnAboutView);
        }
    }
}
