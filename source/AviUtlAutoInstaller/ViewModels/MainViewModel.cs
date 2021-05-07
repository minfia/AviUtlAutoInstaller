using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    MessageBox.Show(e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show(e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private bool _installing = false;
        public Func<bool> ClosingCallback
        {
            get { return OnExit; }
        }
        private bool OnExit()
        {
            if (_installing)
            {
                if (MessageBox.Show("インストールをキャンセルして終了しますか？", "終了確認", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                {
                    return false;
                }
            }
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

        #region インストール時のコントロールの有効/無効
        private bool _isInstallButtonEnable = true;
        public bool IsInstallButtonEnable
        {
            get { return _isInstallButtonEnable; }
            private set { SetProperty(ref _isInstallButtonEnable, value); }
        }

        private bool _isFileOpenMenuEnable = true;
        public bool IsFileOpenMenuEnable
        {
            get { return _isFileOpenMenuEnable; }
            private set { SetProperty(ref _isFileOpenMenuEnable, value); }
        }

        private bool _isInstallEditManuEnable = true;
        public bool IsInstallEditManuEnable
        {
            get { return _isInstallEditManuEnable; }
            private set { SetProperty(ref _isInstallEditManuEnable, value); }
        }

        private bool _isUpdateCheckManuEnable = true;
        public bool IsUpdateCheckManuEnable
        {
            get { return _isUpdateCheckManuEnable; }
            private set { SetProperty(ref _isUpdateCheckManuEnable, value); }
        }

        private bool _isCopyBackupEnable = true;
        public bool IsCopyBackupEnable
        {
            get { return _isCopyBackupEnable; }
            private set { SetProperty(ref _isCopyBackupEnable, value); }
        }

        private bool _isSelectInstallDirEnable = true;
        public bool IsSelectInstallDirEnable
        {
            get { return _isSelectInstallDirEnable; }
            private set { SetProperty(ref _isSelectInstallDirEnable, value); }
        }
        #endregion

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
                    _installing = true;
                    IsInstallButtonEnable = IsFileOpenMenuEnable = IsInstallEditManuEnable = IsUpdateCheckManuEnable = IsCopyBackupEnable = IsSelectInstallDirEnable = false;
                    await InstallAsync();
                    IsInstallButtonEnable = IsFileOpenMenuEnable = IsInstallEditManuEnable = IsUpdateCheckManuEnable = IsCopyBackupEnable = IsSelectInstallDirEnable = true;
                    _installing = false;
                    ProgressVisiblity = Visibility.Collapsed;
                });
        }

        /// <summary>
        /// インストール処理
        /// </summary>
        /// <returns></returns>
        private async Task<bool> InstallAsync()
        {
            SetupDirectory();

            List<InstallItem> installItems = new List<InstallItem>();
            {
                InstallItemList installItemList = new InstallItemList();

                for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
                {
                    foreach (InstallItem item in installItemList.GetInstalItemList(i))
                    {
                        if (item.IsSelect) { installItems.Add(item); }
                    }
                }
            }

            List<string> failedList = await Downloads(installItems);
            if (0 < failedList.Count)
            {
                Console.WriteLine("missing download file.");
            }

            await Installs(installItems);
            Directory.Delete(SysConfig.InstallExpansionDir, true);

            InstallProfileRW installProfileRW = new InstallProfileRW();
            installProfileRW.FileWrite($"{SysConfig.InstallRootPath}\\InstallationList_{DateTime.Now:yyyyMMdd_HHmmss}.profile");

            if (IsCopyBackupFiles)
            {
                BackupInstallFile(installItems);
            }

            return true;
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
        /// ダウンロード処理関係
        /// </summary>
        /// <param name="installItems">インストール一覧</param>
        /// <returns></returns>
        private async Task<List<string>> Downloads(List<InstallItem> installItems)
        {
            await FileDownloadAsync(installItems);

            var verifyResultList = VerifyInstallFile(installItems);
            return verifyResultList;
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
            List<bool> downloadComp = new List<bool>();

            foreach (InstallItem item in installItems)
            {
                item.IsDownloadCompleted = false;
                List<string> itemUrls = new List<string>();
                List<string> itemFileNames= new List<string>();
                itemUrls.Add(item.URL);
                itemUrls.AddRange(item.ExternalFileURLList);
                itemFileNames.Add(item.DownloadFileName);
                itemFileNames.AddRange(item.ExternalFileList);

                Func<string, string, DownloadResult> func = new Func<string, string, DownloadResult>(downloader.DownloadStart);
                for (int i = 0; i < itemUrls.Count; i++)
                {
                    if (File.Exists($"{SysConfig.CacheDirPath}\\{itemFileNames[i]}"))
                    {
                        item.IsDownloadCompleted = true;
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
                    downloadComp.Add((DownloadResult.Complete == res) ? true : false);
                }

                foreach (bool b in downloadComp)
                {
                    if (!b)
                    {
                        item.IsDownloadCompleted = false;
                        break;
                    }
                    item.IsDownloadCompleted = true;
                }
                downloadComp.Clear();
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
                if (item.IsSelect && (!item.IsDownloadCompleted || !cacheFileList.Any(x => Path.GetFileName(x) == item.DownloadFileName)))
                {
                    faileVerifyList.Add(item.DownloadFileName);
                }
            }

            return faileVerifyList;
        }

        /// <summary>
        /// インストール処理関係
        /// </summary>
        /// <param name="installItems">インストール一覧</param>
        /// <returns></returns>
        private async Task Installs(List<InstallItem> installItems)
        {
            await FileInstallAsync(installItems);

            {
                string aviutl = $"{SysConfig.InstallRootPath}\\aviutl.exe";
                if (File.Exists(aviutl))
                {
                    FileOperation fileOperation = new FileOperation();
                    if (fileOperation.ExecApp(aviutl, FileOperation.ExecAppType.GUI, out Process process))
                    {
                        await Task.Delay(1000);

                        fileOperation.KillApp(process);
                    }
                }
            }
        }

        private async Task FileInstallAsync(List<InstallItem> installItems)
        {
            StateName = _processStateDic[ProcessState.Install];
            StateItemMax = installItems.Count;
            StateItemNow = 0;

            InstallItemList installItemList = new InstallItemList();

            for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
            {
                foreach (InstallItem item in installItemList.GetInstalItemList(i))
                {
                    if (!item.IsSelect || !item.IsDownloadCompleted)
                    {
                        continue;
                    }
                    Func<string[], bool> func = new Func<string[], bool>(item.Install);
                    var installFileList = GenerateInstalList(i, item);
                    var task = Task.Run(() => func(installFileList.ToArray()));

                    var res = await task;
                    StateItemNow++;
                }
            }
        }

        /// <summary>
        /// インストールアイテムから、実際にインストールするファイル一覧を生成
        /// </summary>
        /// <param name="repoType">リポジトリの種類</param>
        /// <param name="item">InstallItem</param>
        /// <returns></returns>
        private List<string> GenerateInstalList(InstallItemList.RepoType repoType, InstallItem item)
        {
            string[] _pluginFileExtension = { "*.auf", "*.aui", "*.auo", "*.auc", "*.aul" };
            string[] _scriptFileExtension = { "*.anm", "*.obj", "*.scn", "*.cam" };
            List<string> installFiles = new List<string>(); // インストールするファイルのパス一覧

            string searchSrcDir;

            string fileExtention = Path.GetExtension(item.DownloadFileName);
            FileOperation fileOperation = new FileOperation();
            if ((Array.IndexOf(_pluginFileExtension, fileExtention) == -1) &&
                (Array.IndexOf(_scriptFileExtension, fileExtention) == -1))
            {
                // ダウンロードしたファイルが圧縮ファイル
                string extractFile = $"{SysConfig.CacheDirPath}\\{item.DownloadFileName}";   // 解凍するファイルのパス: .\cache\FileName.圧縮形式
                string extractDestDir = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(item.DownloadFileName)}";  //解凍先: root\EX_TEMP\FileNameDir\
                fileOperation.Extract(extractFile, extractDestDir);
                searchSrcDir = extractDestDir;
            }
            else
            {
                // ダウンロードしたファイルがスクリプトorプラグインファイル
                searchSrcDir = $"{SysConfig.CacheDirPath}"; // cache\FileName
            }

            if (InstallItemList.RepoType.Pre == repoType)
            {
                installFiles = fileOperation.GenerateFilePathList(searchSrcDir, item.InstallFileList.ToArray());
            }
            else
            {
                if (InstallFileType.Plugin == item.FileType)
                {
                    installFiles = fileOperation.GenerateFilePathList(searchSrcDir, _pluginFileExtension);
                }
                else if (InstallFileType.Script == item.FileType)
                {
                    installFiles = fileOperation.GenerateFilePathList(searchSrcDir, _scriptFileExtension);
                }

                if (0 < item.InstallFileList.Count)
                {
                    installFiles.AddRange(item.InstallFileList.ToList());
                }
            }

            return installFiles;
        }

        /// <summary>
        /// インストールにしたダウンロードファイルをインストール先にバックアップ
        /// </summary>
        /// <param name="installItems">インストール一覧</param>
        private void BackupInstallFile(List<InstallItem> installItems)
        {
            Directory.CreateDirectory(SysConfig.InstallFileBackupDir);
            FileOperation fileOperation = new FileOperation();

            foreach (var file in installItems)
            {
                string copyFile = $"{SysConfig.InstallFileBackupDir}\\{file.DownloadFileName}";
                if (File.Exists(copyFile))
                {
                    File.Delete(copyFile);
                }
                File.Copy($"{SysConfig.CacheDirPath}\\{file.DownloadFileName}", copyFile);
            }
        }
    }
}
