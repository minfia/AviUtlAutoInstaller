using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.IO;
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

        #region ファイルダイアログ(インストール先の選択)の設定
        private Action<bool, string> _folderDialogSelectInstallDirCallback;
        public Action<bool, string> FolderDialogSelectInstallDirCallback
        {
            get { return _folderDialogSelectInstallDirCallback; }
            private set { SetProperty(ref _folderDialogSelectInstallDirCallback, value); }
        }
        private void OnFolderDialogSelectInstallDirCallback(bool isOk, string installPath)
        {
            if (isOk)
            {
                if (!FileOperation.IsWritableOfDirectory(installPath))
                {
                    MessageBox.Show("書き込み権限がありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    FolderDialogSelectInstallDirCallback = null;
                    return;
                }

                SysConfig.InstallRootPath = File.Exists($"{installPath}\\aviutl.exe") ? $"{installPath}" : $"{installPath}\\AviUtl";
                InstallDirPath = SysConfig.InstallRootPath;
                Console.WriteLine($"{SysConfig.InstallExpansionDir}\n{SysConfig.InstallFileBackupDir}\n{SysConfig.AviUtlPluginDir}\n{SysConfig.AviUtlScriptDir}\n{SysConfig.AviUtlFigureDir}");
            }
            FolderDialogSelectInstallDirCallback = null;
        }
        #endregion

        private readonly DelegateCommand _selectInstallDir;
        private readonly DelegateCommand _installStartCommand;
        private readonly DelegateCommand installCancelCommand;

        public DelegateCommand SelectInstallDir { get => _selectInstallDir; }
        public DelegateCommand InstallStartCommand { get => _installStartCommand; }

        private string _installDirPath;
        public string InstallDirPath
        {
            get { return _installDirPath; }
            private set { SetProperty(ref _installDirPath, value); }
        }

        private bool _isCopyBackupFiles = false;
        public bool IsCopyBackupFiles
        {
            get { return _isCopyBackupFiles; }
            set { SetProperty(ref _isCopyBackupFiles, value); }
        }

        public MainViewModel()
        {
            InstallDirPath = SysConfig.InstallRootPath;
            SysConfig.InstallRootPath = InstallDirPath;
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

            _selectInstallDir = new DelegateCommand(_ => FolderDialogSelectInstallDirCallback = OnFolderDialogSelectInstallDirCallback);
            _installStartCommand = new DelegateCommand(
                _ =>
                {
                    Install();
                });
        }


        public bool Install()
        {
            SetupDirectory();

            InstallProfileRW installProfileRW = new InstallProfileRW();
            installProfileRW.FileWrite($"{SysConfig.InstallRootPath}\\InstallationList_{DateTime.Now:yyyyMMdd_HHmmss}.profile");
            return true;
        }

        public void SetupDirectory()
        {
            if (!File.Exists($"{InstallDirPath}\\aviutl.exe"))
            {
                Directory.CreateDirectory(SysConfig.InstallRootPath);
                Directory.CreateDirectory(SysConfig.AviUtlPluginDir);
                Directory.CreateDirectory(SysConfig.AviUtlFigureDir);
                Directory.CreateDirectory(SysConfig.AviUtlScriptDir);
            }
            if (IsCopyBackupFiles)
            {
                Directory.CreateDirectory(SysConfig.InstallFileBackupDir);
            }
            Directory.CreateDirectory(SysConfig.InstallExpansionDir);
        }
    }
}
