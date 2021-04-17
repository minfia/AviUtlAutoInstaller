using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            if (isOk)
            {
                try
                {
                    InstallProfileRW installProfileRW = new InstallProfileRW();

                    installProfileRW.FileRead(filePath);
                }
                catch (Exception e)
                {
                    // TODO: エラー表示
                }
                finally
                {
                    OpenDialogInstallSettingFileCallback = null;
                }
            }
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
            if (isOk)
            {
                try
                {
                    UserRepoFileRW userRepoFileRead = new UserRepoFileRW();

                    userRepoFileRead.FileRead(filePath);
                }
                catch (Exception e)
                {
                    // TODO: エラー表示
                }
                finally
                {
                    OpenDialogUserRepoFileCallback = null;
                }
            }
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

        #endregion

        private readonly DelegateCommand selectInstallDir;
        private readonly DelegateCommand _installStartCommand;
        private readonly DelegateCommand installCancelCommand;

        public DelegateCommand InstallStartCommand { get => _installStartCommand; }

        public MainViewModel()
        {
            _openDialogInstallSettingFileCommand = new DelegateCommand(_ => OpenDialogInstallSettingFileCallback = OnOpenDialogInstallSettingFileCallback);
            _openDialogUserRepoFileCommand = new DelegateCommand(_ => OpenDialogUserRepoFileCallback = OnOpenDialogUserRepoFileCallback);
            _exitCommand = new DelegateCommand(_ => OnExit());
            _installEditCommand = new DelegateCommand(
                _ =>
                {
                    InstallEditView window = new InstallEditView();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                });
            _updateCheckCommand = new DelegateCommand(
                _ =>
                {
                    UpdateCheckView window = new UpdateCheckView();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                });
            _aboutCommand = new DelegateCommand(
                _ =>
                {
                    AboutView window = new AboutView();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                });

            _installStartCommand = new DelegateCommand(
                _ =>
                {
                    InstallProfileRW installProfileRW = new InstallProfileRW();
                    installProfileRW.FileWrite($".\\InstallationList_{DateTime.Now:yyyyMMdd_HHmmss}.profile");
                });
        }
    }
}
