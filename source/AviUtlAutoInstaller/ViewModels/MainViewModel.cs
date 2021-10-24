using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using AviUtlAutoInstaller.Models.Network;
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
        /// <summary>
        /// インストール結果
        /// </summary>
        enum InstallResult { 
            /// <summary>
            /// 正常
            /// </summary>
            OK,
            /// <summary>
            /// 失敗
            /// </summary>
            NG,
            /// <summary>
            /// キャンセル
            /// </summary>
            Cancel,
            /// <summary>
            /// ダウンロード失敗あり
            /// </summary>
            DownloadFailed,
        };

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
            AppConfig.Save();
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
                if (!Directory.Exists(installPath))
                {
                    int lastIndex = installPath.LastIndexOf("\\");
                    installPath = installPath.Substring(0, lastIndex);
                }

                if (!FileOperation.IsWritableOfDirectory(installPath))
                {
                    MessageBox.Show("書き込み権限がありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    FolderDialogSelectInstallDirCallback = null;
                    return;
                }

                SysConfig.IsInstalledAviUtl = File.Exists($"{installPath}\\aviutl.exe");
                SysConfig.InstallRootPath = SysConfig.IsInstalledAviUtl ? $"{installPath}" : $"{installPath}\\AviUtl";
                if (SysConfig.IsInstalledAviUtl)
                {
                    ContentsTreeRW contentsTreeRW = new ContentsTreeRW();
                    contentsTreeRW.Read(SysConfig.InstallRootPath);

                    {
                        FileOperation fileOperation = new FileOperation();
                        string[] array = { "InstallationList_*_*.profile" };
                        var list = fileOperation.GenerateFilePathList(SysConfig.InstallRootPath, array);
                        InstallProfileRW installProfileRW = new InstallProfileRW();
                        installProfileRW.FileRead(list[list.Count - 1]);
                    }
                }
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

        private bool _isMakeShortcut = false;
        public bool IsMakeShortcut
        {
            get { return _isMakeShortcut; }
            set { SetProperty(ref _isMakeShortcut, value); }
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

        private bool _isMakeShortcutEnable = true;
        public bool IsMakeShortcutEnable
        {
            get { return _isMakeShortcutEnable; }
            private set { SetProperty(ref _isMakeShortcutEnable, value); }
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
                    IsInstallButtonEnable = IsFileOpenMenuEnable = IsInstallEditManuEnable = IsUpdateCheckManuEnable = IsCopyBackupEnable = IsMakeShortcutEnable = IsSelectInstallDirEnable = false;
                    var result = await InstallAsync();
                    IsInstallButtonEnable = IsFileOpenMenuEnable = IsInstallEditManuEnable = IsUpdateCheckManuEnable = IsCopyBackupEnable = IsMakeShortcutEnable = IsSelectInstallDirEnable = true;
                    _installing = false;
                    ProgressVisiblity = Visibility.Collapsed;

                    string message = "インストールが完了しました";
                    string title = "情報";
                    var image = MessageBoxImage.Information;
                    switch (result)
                    {
                        case InstallResult.DownloadFailed:
                            message = $"ダウンロードに失敗したファイルがあります\n詳細は{SysConfig.DownloadFailedFile}を確認してください";
                            title = "エラー";
                            image = MessageBoxImage.Error;
                            break;
                        case InstallResult.NG:
                            message = "インストールに失敗しました";
                            title = "エラー";
                            image = MessageBoxImage.Error;
                            break;
                        case InstallResult.Cancel:
                            message = "インストールがキャンセルされました";
                            break;
                        default:
                            break;
                    }
                    if (result == InstallResult.OK)
                    {
                        SysConfig.IsInstalledAviUtl = true;
                    }
                    MessageBox.Show(message, title, MessageBoxButton.OK, image);
                });
        }

        /// <summary>
        /// インストール処理
        /// </summary>
        /// <returns></returns>
        private async Task<InstallResult> InstallAsync()
        {
            InstallResult installResult = InstallResult.OK;
            SetupDirectory();

            List<InstallItem> installItems = new List<InstallItem>();
            List<string> discordItemList = new List<string>();
            {
                InstallItemList installItemList = new InstallItemList();

                for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
                {
                    foreach (InstallItem item in installItemList.GetInstalItemList(i))
                    {
                        if (item.IsSelect)
                        {
                            if (!item.DownloadExecute && !File.Exists($"{SysConfig.CacheDirPath}\\{item.DownloadFileName}"))
                            {
                                discordItemList.Add(item.Name);
                            }
                            installItems.Add(item);
                        }
                    }
                }
            }

            if (discordItemList.Count != 0)
            {
                string names = "";
                foreach (string str in discordItemList)
                {
                    names += $"・{str}\n";
                }

                if (MessageBox.Show($"以下の項目のファイルが存在しません。継続しますか？\n" +
                                    $"(継続の場合は以下のファイルはインストールされません)\n" +
                                    $"{names}", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return InstallResult.Cancel;
                }
            }

            List<string> failedList = await Downloads(installItems);
            if (0 < failedList.Count)
            {
                bool pr = false;
                using (StreamWriter sw = new StreamWriter($"{SysConfig.DownloadFailedFile}"))
                {
                    sw.WriteLine("ダウンロード失敗リスト");
                    sw.WriteLine("項目名, ダウンロードURL, ダウンロードファイル名");
                    foreach (string failedFile in failedList)
                    {
                        int index = installItems.FindIndex(x => x.DownloadFileName == failedFile);
                        if (index < 0)
                        {
                            continue;
                        }
                        var item = installItems[index];
                        sw.WriteLine($"{item.Name}, {item.URL}, {item.DownloadFileName}");
                        if (!pr && item.Priority == InstallPriority.High)
                        {
                            pr = true;
                        }
                    }
                }
                if (pr)
                {
                    return InstallResult.DownloadFailed;
                }
                installResult = InstallResult.DownloadFailed;
            }

            await Installs(installItems);
            Directory.Delete(SysConfig.InstallExpansionDir, true);

            InstallProfileRW installProfileRW = new InstallProfileRW();
            installProfileRW.FileWrite($"{SysConfig.InstallRootPath}");

            ContentsTreeRW contentsTreeRW = new ContentsTreeRW();
            if (contentsTreeRW.IsExistContents)
            {
                contentsTreeRW.Write($"{SysConfig.InstallRootPath}");
            }

            if (IsCopyBackupFiles)
            {
                BackupInstallFile(installItems);
            }

            if (IsMakeShortcut)
            {
                FileOperation fileOperation = new FileOperation();
                fileOperation.MakeShortcut($"{InstallDirPath}\\AviUtl.exe", "AviUtl");
            }

            return installResult;
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
                if (item.IsSelect &&
                    (!item.IsDownloadCompleted || !cacheFileList.Any(x => Path.GetFileName(x) == item.DownloadFileName)) &&
                    item.DownloadExecute)
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
                    {
                        var func = new Func<InstallItem, string[], bool>(InstallItem.Install);
                        var installFileList = installItemList.GenerateInstalList(i, item);
                        var task = Task.Run(() => func(item, installFileList.ToArray()));
                        var res = await task;
                        if (res && "sm".Length < item.NicoVideoID.Length)
                        {
                            ContentsTreeRW.AddContents(item.NicoVideoID);
                        }
                        item.IsInstallCompleted = res;
                        item.IsSelect = res;
                    }

                    if (!string.IsNullOrWhiteSpace(item.ExternalFile))
                    {
                        var exFunc = new Func<string[], bool>(InstallItem.ExternalInstall);
                        var exTask = Task.Run(() => exFunc(item.ExternalFileList.ToArray()));
                        var res = await exTask;
                    }

                    StateItemNow++;
                }
            }
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

        public async void ShowWindow(object sender, EventArgs e)
        {
            UpdateCheck updateCheck = new UpdateCheck();

            UpdateCheck.CheckResult[] res = new UpdateCheck.CheckResult[2];
            res[0] = updateCheck.Check(UpdateCheck.CheckTarget.PreRepo, ProductInfo.RepoVersion, out string getRepoVersion, out string repoURL);
            res[1] = updateCheck.Check(UpdateCheck.CheckTarget.App, ProductInfo.AppVersion, out string getAppVersion, out string appURL);

            if (res[1] == UpdateCheck.CheckResult.Update)
            {
                // アプリ
                if (MessageBoxResult.Yes == MessageBox.Show($"アプリのアップデート({getAppVersion})があります\nアップデートしますか？", "情報", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    if (!File.Exists(SysConfig.UpdaterFilePath))
                    {
                        MessageBox.Show("updater.exeがありません、アプリケーションを再ダウンロードしてください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    updateCheck.UpdateApplication(appURL, repoURL);
                    App.Current.Shutdown();
                }
            }
            else if (res[0] == UpdateCheck.CheckResult.Update)
            {
                // プリインストールリポジトリ
                if (MessageBoxResult.Yes == MessageBox.Show($"リポジトリのアップデート({getRepoVersion})があります\nアップデートしますか？", "情報", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    Task<UpdateCheck.RepoUpdateResult> task = Task.Run(() => updateCheck.UpdatePreRepo(repoURL));
                    await task;

                    switch (task.Result)
                    {
                        case UpdateCheck.RepoUpdateResult.UnSupported:
                            MessageBox.Show("新しいバージョンは、このアプリケーションのバージョンでは使用できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case UpdateCheck.RepoUpdateResult.Failed:
                            MessageBox.Show("アップデートに失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case UpdateCheck.RepoUpdateResult.Success:
                            MessageBox.Show("アップデートしました", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if ((res[0] == UpdateCheck.CheckResult.Failed) || res[1] == UpdateCheck.CheckResult.Failed)
            {
                // アップデートチェック失敗
            }
        }
    }
}
