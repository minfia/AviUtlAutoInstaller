using AAIUpdater.Models;
using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AAIUpdater.ViewModels
{
    class MainViewModel : NotificationObject
    {
        private string _processName = "";
        /// <summary>
        /// 実行内容名
        /// </summary>
        public string ProcessName
        {
            get { return _processName; }
            private set { SetProperty(ref _processName, value); }
        }

        private string _downloadFileName = "";
        /// <summary>
        /// ダウンロードファイル名
        /// </summary>
        public string DownloadFileName
        {
            get { return _downloadFileName; }
            private set { SetProperty(ref _downloadFileName, value); }
        }

        private int _downloadSizeNow = 0;
        /// <summary>
        /// ダウンロード完了サイズ
        /// </summary>
        public int DownloadSizeNow
        {
            get { return _downloadSizeNow; }
            private set { SetProperty(ref _downloadSizeNow, value); }
        }

        private int _downloadSizeMax = 0;
        /// <summary>
        /// ダウンロードファイルサイズ
        /// </summary>
        public int DownloadSizeMax
        {
            get { return _downloadSizeMax; }
            private set { SetProperty(ref _downloadSizeMax, value); }
        }

        private int _updateProgressMax = 0;
        /// <summary>
        /// プログレスバーの最大値
        /// </summary>
        public int UpdateProgressMax
        {
            get { return _updateProgressMax; }
            set { SetProperty(ref _updateProgressMax, value); }
        }

        private int _updateProgressValue = 0;
        /// <summary>
        /// プログレスバーの現在値
        /// </summary>
        public int UpdateProgressValue
        {
            get { return _updateProgressValue; }
            set { SetProperty(ref _updateProgressValue, value); }
        }

        private async void UpdateProcess()
        {
            var args = Environment.GetCommandLineArgs();

            string appURL = "";
            string repoURL = "";
            List<string> dirs = new List<string>();
            List<string> files = new List<string>();
            // --は別のオプションとする
            // ""で括った--も別オプションとする
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--app":
                        appURL = args[i + 1];
                        UpdateProgressMax += 2;
                        i++;
                        break;
                    case "--repo":
                        repoURL = args[i + 1];
                        UpdateProgressMax += 2;
                        i++;
                        break;
                    case "--exclude-dirs":
                        i++;
                        {
                            List<string> dirNames = new List<string>();
                            for (; i < args.Length; i++)
                            {
                                if (args[i].Contains("--") && (args[i].IndexOf(' ') < 0))
                                {
                                    i--;    // --は引数のため
                                    break;
                                }
                                dirNames.Add(args[i]);
                            }

                            foreach (string str in dirNames)
                            {
                                if (str.Contains(' '))
                                {
                                    var strs = str.Split(' ');
                                    dirs.AddRange(strs);
                                }
                                else 
                                {
                                    dirs.Add(str);
                                }
                            }
                        }
                        break;
                    case "--exclude-files":
                        i++;
                        {
                            List<string> fileNames = new List<string>();
                            for (; i < args.Length; i++)
                            {
                                if (args[i].Contains("--") && (args[i].IndexOf(' ') < 0))
                                {
                                    i--;    // --は引数のため
                                    break;
                                }
                                fileNames.Add(args[i]);
                            }

                            foreach (string str in fileNames)
                            {
                                if (str.Contains(' '))
                                {
                                    var strs = str.Split(' ');
                                    files.AddRange(strs);

                                }
                                else
                                {
                                    files.Add(str);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            Updater updater = new Updater();
            updater.SetDeleteExcludeDir(dirs.ToArray());
            updater.SetDeleteExcludeFile(files.ToArray());
            string cacheDir = ".\\cache";
            {
                // アプリ
                var sp = appURL.Split(new char[] { '/' });
                string fileName = sp[sp.Length - 1];
                DownloadSizeMax = 0;
                DownloadSizeNow = 0;

                Downloader downloader = new Downloader(cacheDir);
                DownloadFileName = fileName;
                ProcessName = "アプリアップデート";
                Task<DownloadResult> task = Task.Run(() => downloader.DownloadStart(appURL, fileName));

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
                if (res != DownloadResult.Complete)
                {
                    MessageBox.Show("ダウンロード出来ませんでした", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Current.Shutdown();
                }
                UpdateProgressValue++;

                string aaiZip = $"{cacheDir}\\{fileName}";
                updater.AppUpdate(aaiZip);

                File.Delete(aaiZip);

                UpdateProgressValue++;
            }

            {
                // プリインストールリポジトリ
                if (!string.IsNullOrEmpty(repoURL))
                {
                    var sp = repoURL.Split(new char[] { '/' });
                    string fileName = sp[sp.Length - 1];
                    DownloadSizeMax = 0;
                    DownloadSizeNow = 0;

                    Downloader downloader = new Downloader(cacheDir);
                    DownloadFileName = fileName;
                    ProcessName = "リポジトリアップデート";
                    Task<DownloadResult> task = Task.Run(() => downloader.DownloadStart(repoURL, fileName));

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
                    if (res != DownloadResult.Complete)
                    {
                        MessageBox.Show("ダウンロード出来ませんでした", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.Current.Shutdown();
                    }
                    UpdateProgressValue++;

                    updater.RepoUpdate($"{cacheDir}\\{fileName}");
                    UpdateProgressValue++;
                }
            }

            // アプリを起動して、自身は終了
            FileOperation fileOperation = new FileOperation();
            fileOperation.ExecApp(".\\aai.exe", FileOperation.ExecAppType.GUI, out Process proc);
            App.Current.Shutdown();
        }

        public void ShowWindow(object sender, EventArgs e)
        {
            UpdateProcess();
        }
    }
}
