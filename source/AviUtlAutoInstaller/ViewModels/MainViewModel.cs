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

        private bool _isInstallButtonEnable = true;
        public bool IsInstallButtonEnable
        {
            get { return _isInstallButtonEnable; }
            private set { SetProperty(ref _isInstallButtonEnable, value); }
        }

        private enum ProcessState
        {
            Download,
            Install,
        };

        private Dictionary<ProcessState, string> _processStateDic = new Dictionary<ProcessState, string>()
        {
            { ProcessState.Download, "ダウンロード" },
            { ProcessState.Install, "インストール" },
        };

        private string _stateName;
        /// <summary>
        /// 処理状態の名前
        /// </summary>
        public string StateName
        {
            get { return _stateName; }
            private set { SetProperty(ref _stateName, value); }
        }

        private int _stateItemNow;
        /// <summary>
        /// 処理中の数
        /// </summary>
        public int StateItemNow
        {
            get { return _stateItemNow; }
            private set { SetProperty(ref _stateItemNow, value); }
        }

        private int _stateItemMax;
        /// <summary>
        /// 処理の最大数
        /// </summary>
        public int StateItemMax
        {
            get { return _stateItemMax; }
            private set { SetProperty(ref _stateItemMax, value); }
        }

        private Visibility _progressVisiblity = Visibility.Collapsed;
        public Visibility ProgressVisiblity
        {
            get { return _progressVisiblity; }
            private set { SetProperty(ref _progressVisiblity, value); }
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
                async _ =>
                {
                    ProgressVisiblity = Visibility.Visible;
                    IsInstallButtonEnable = false;
                    await InstallAsync();
                    IsInstallButtonEnable = true;
                    //ProgressVisiblity = Visibility.Collapsed;
                });
        }

        /// <summary>
        /// インストール処理
        /// </summary>
        /// <returns></returns>
        private async Task<bool> InstallAsync()
        {
            SetupDirectory();
            var installList = MakeInstallList();
            await FileDownloadAsync(installList);
            var verifyResultList = VerifyInstallFile(installList);
            if (verifyResultList.Count != 0)
            {
                Console.WriteLine("missing download file.");
            }

            InstallProfileRW installProfileRW = new InstallProfileRW();
            installProfileRW.FileWrite($"{SysConfig.InstallRootPath}\\InstallationList_{DateTime.Now:yyyyMMdd_HHmmss}.profile");
            return true;
        }

        /// <summary>
        /// インストール一覧を作成
        /// </summary>
        /// <returns>インストール一覧</returns>
        private List<InstallItem> MakeInstallList()
        {
            List<InstallItem> installItems = new List<InstallItem>();
            InstallItemList installItemList = new InstallItemList();
            for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
            {
                foreach (InstallItem item in installItemList.GetInstalItemList(i))
                {
                    if (item.IsSelect)
                    {
                        installItems.Add(item);
                    }
                }
            }

            return installItems;
        }

        /// <summary>
        /// インストールに必要なディレクトリを作成
        /// </summary>
        private void SetupDirectory()
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

        private string _downloadFileName;
        /// <summary>
        /// ダウンロードするファイル名
        /// </summary>
        public string DownloadFileName
        {
            get { return _downloadFileName; }
            private set { SetProperty(ref _downloadFileName, value); }
        }

        private int _downloadSizeNow;
        /// <summary>
        /// ダウンロード済みのファイルサイズ
        /// </summary>
        public int DownloadSizeNow
        {
            get { return _downloadSizeNow; }
            private set { SetProperty(ref _downloadSizeNow, value); }
        }

        private int _downloadSizeMax;
        /// <summary>
        /// ダウンロードするファイルサイズ
        /// </summary>
        public int DownloadSizeMax
        {
            get { return _downloadSizeMax; }
            private set { SetProperty(ref _downloadSizeMax, value); }
        }

        private Visibility _downloadVisiblity = Visibility.Visible;
        /// <summary>
        /// ダウンロードプログレスバーの表示設定
        /// </summary>
        public Visibility DownloadVisiblity
        {
            get { return _downloadVisiblity; }
            private set { SetProperty(ref _downloadVisiblity, value); }
        }

        /// <summary>
        /// インストールするファイルをダウンロード
        /// </summary>
        /// <param name="installItems">インストール一覧</param>
        /// <returns></returns>
        public async Task FileDownloadAsync(List<InstallItem> installItems)
        {
            DownloadVisiblity = Visibility.Visible;

            StateName = _processStateDic[ProcessState.Download];
            StateItemMax = installItems.Count;
            StateItemNow = 0;
            Downloader downloader = new Downloader($"{SysConfig.CacheDirPath}");

            foreach (InstallItem item in installItems)
            {
                List<string> itemUrls = new List<string>();
                List<string> itemFileNames= new List<string>();
                itemUrls.Add(item.URL);
                itemUrls.AddRange(item.ExternalFileURLList);
                itemFileNames.Add(item.FileName);
                itemFileNames.AddRange(item.ExternalFileList);

                Func<string, string, DownloadResult> func = new Func<string, string, DownloadResult>(downloader.DownloadStart);
                for (int i = 0; i < itemUrls.Count; i++)
                {
                    if (File.Exists($"{SysConfig.CacheDirPath}\\{itemFileNames[i]}"))
                    {
                        continue;
                    }
                    DownloadFileName = itemFileNames[i];
                    var task = Task.Run(() => func(itemUrls[i], itemFileNames[i]));

                    Task updateTask = Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(1);
                            DownloadSizeMax = (int)downloader.DownloadFileSize;
                            if (downloader.DownloadFileSize != 0)
                            {
                                DownloadSizeNow = (int)downloader.DownloadCompleteSize;
                            }
                        } while (task.Status != TaskStatus.RanToCompletion);
                    });

                    var res = await task;
                    Console.WriteLine($"download result: {res}");
                }
                StateItemNow++;
            }
            DownloadFileName = "";
            DownloadSizeNow = 0;
            DownloadSizeMax = 0;
            DownloadVisiblity = Visibility.Collapsed;
            StateName = "";
            StateItemMax = 0;
            StateItemNow = 0;
        }

        /// <summary>
        /// インストールに必要なものがダウンロードされているか確認
        /// </summary>
        /// <param name="installItems">インストール一覧</param>
        /// <returns>ダウンロードされていないファイル一覧</returns>
        private List<string> VerifyInstallFile(List<InstallItem> installItems)
        {
            List<string> faileVerifyList = new List<string>();
            string[] cacheFileList = Directory.GetFiles(SysConfig.CacheDirPath);

            foreach (InstallItem item in installItems)
            {
                if (item.IsSelect && !cacheFileList.Any(x => Path.GetFileName(x) == item.FileName))
                {
                    faileVerifyList.Add(item.FileName);
                }
            }

            return faileVerifyList;
        }
    }
}
