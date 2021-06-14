using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using AviUtlAutoInstaller.Models.Network;
using AviUtlAutoInstaller.Models.Network.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AviUtlAutoInstaller.ViewModels
{
    class UpdateCheckViewModel : NotificationObject
    {
        private string _preRepoGitHubUrl = "https://github.com/minfia/AAI_Repo/releases";
        private string _applicationGitHubUrl = "https://github.com/minfia/AviUtlAutoInstaller/releases";

        #region プリインストール
        private string _preRepoVersion;
        public string PreRepoVersion
        {
            get { return _preRepoVersion; }
            private set { SetProperty(ref _preRepoVersion, value); }
        }

        private string _preRepoGetVersion;
        public string PreRepoGetVersion
        {
            get { return _preRepoGetVersion; }
            private set { SetProperty(ref _preRepoGetVersion, value); }
        }

        private string _preRepoUpdateExist;
        public string PreRepoUpdateExist
        {
            get { return _preRepoUpdateExist; }
            private set { SetProperty(ref _preRepoUpdateExist, value); }
        }

        private string _preRepoURL = "";
        private bool _preRepoUpdateEnable = false;
        public bool PreRepoUpdateEnable
        {
            get { return _preRepoUpdateEnable; }
            private set { SetProperty(ref _preRepoUpdateEnable, value); }
        }

        public string AppMatchVersion { get; private set; }
        private string _appMatchGetVersion = "";
        /// <summary>
        /// リポジトリが対応するバージョン
        /// </summary>
        public string AppMatchGetVersion
        {
            get { return _appMatchGetVersion; }
            private set { SetProperty(ref _appMatchGetVersion, value); }
        }

        private DelegateCommand _preRepoUpdateCommand;
        public DelegateCommand PreRepoUpdateCommand { get => _preRepoUpdateCommand; }
        #endregion

        #region アプリケーション
        private string _applicationVersion;
        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            private set { SetProperty(ref _applicationVersion, value); }
        }

        private string _applicationGetVersion;
        public string ApplicationGetVersion
        {
            get { return _applicationGetVersion; }
            private set { SetProperty(ref _applicationGetVersion, value); }
        }

        private string _applicationUpdateExist;
        public string ApplicationUpdateExist
        {
            get { return _applicationUpdateExist; }
            private set { SetProperty(ref _applicationUpdateExist, value); }
        }

        public string SupportRepoVersion { get; private set; }
        #endregion

        private bool _updateCheckButtonEnable = true;
        public bool UpdateCheckButtonEnable
        {
            get { return _updateCheckButtonEnable; }
            private set { SetProperty(ref _updateCheckButtonEnable, value); }
        }

        private Visibility _progressVisible = Visibility.Hidden;
        public Visibility ProgressVisible
        {
            get { return _progressVisible; }
            private set { SetProperty(ref _progressVisible, value); }
        }

        private DelegateCommand _updateCheckCommand;
        public DelegateCommand UpdateCheckCommand { get => _updateCheckCommand; }

        public UpdateCheckViewModel()
        {
            _preRepoUpdateCommand = new DelegateCommand(
                async _ =>
                {
                    PreRepoUpdateEnable = UpdateCheckButtonEnable = false;
                    await PreRepoUpdate();
                    UpdateCheckButtonEnable = true;
                });
            _updateCheckCommand = new DelegateCommand(
                async _ =>
                {
                    PreRepoGetVersion = PreRepoUpdateExist = "";
                    ApplicationGetVersion = ApplicationUpdateExist = "";
                    await UpdateCheck();
                });
            ApplicationVersion = ProductInfo.ValidAppVersion;
            PreRepoVersion = ProductInfo.RepoVersion;
            AppMatchVersion = ProductInfo.AppMatchVersion.ToString();
            SupportRepoVersion = ProductInfo.SupportRepoVersion.ToString();
        }

        private async Task<bool> UpdateCheck()
        {
            UpdateCheckButtonEnable = false;
            ProgressVisible = Visibility.Visible;

            bool[] res = new bool[2];

            Task task = Task.Run(() =>
            {
                res[0] = PreRepoUpdateCheck();
                res[1] = AppUpdateCheck();
            });
            await task;

            ProgressVisible = Visibility.Hidden;
            UpdateCheckButtonEnable = true;

            return res.Contains(false) == true ? false : true;
        }

        /// <summary>
        /// プリインストールリポジトリのアップデートチェック
        /// </summary>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        private bool PreRepoUpdateCheck()
        {
            GitHub gitHub = new GitHub();

            Uri uri = new Uri(_preRepoGitHubUrl);
            try
            {
                if (!gitHub.Parse(uri))
                {
                    return false;
                }
            }
            catch
            {
                PreRepoUpdateExist = "アップデートチェックに失敗しました";
                return false;
            }
            PreRepoGetVersion = gitHub.Versions[0];
            _preRepoURL = gitHub.VersionLink[PreRepoGetVersion];

            if (CompVersion(PreRepoVersion, PreRepoGetVersion))
            {
                PreRepoUpdateExist = "アップデートがあります";
                PreRepoUpdateEnable = true;
            }

            return true;
        }

        /// <summary>
        /// アプリのアップデートチェック
        /// </summary>
        /// <returns></returns>
        private bool AppUpdateCheck()
        {
            GitHub gitHub = new GitHub();

            Uri uri = new Uri(_applicationGitHubUrl);
            try
            {
                if (!gitHub.Parse(uri))
                {
                    return false;
                }
            }
            catch
            {
                ApplicationUpdateExist = "アップデートチェックに失敗しました";
                return false;
            }
            ApplicationGetVersion = gitHub.Versions[0];

            if (CompVersion(ApplicationVersion, ApplicationGetVersion))
            {
                ApplicationUpdateExist = "アップデートがあります";
            }

            return true;
        }

        /// <summary>
        /// プリインストールリポジトリの更新
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PreRepoUpdate()
        {
            Downloader downloader = new Downloader($"{SysConfig.CacheDirPath}");

            Task<DownloadResult> task = Task.Run(() => downloader.DownloadStart(_preRepoURL, "aai.repo"));
            await task;

            if (task.Result == DownloadResult.Complete)
            {
                if (File.Exists($"{SysConfig.CacheDirPath}\\aai.repo"))
                {
                    PreRepoFileR preRepoFileR = new PreRepoFileR($"{SysConfig.CacheDirPath}\\aai.repo");
                    preRepoFileR.Open();
                    preRepoFileR.GetDBVersion(out uint major, out uint minor, out uint maintenance, out uint app_match);
                    ProductInfo productInfo = new ProductInfo();

                    if (!productInfo.IsSupportRepoVersion(app_match))
                    {
                        MessageBox.Show("新しいバージョンは、このアプリケーションのバージョンでは使用できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        preRepoFileR.Close();
                        File.Delete($"{SysConfig.CacheDirPath}\\aai.repo");
                        return false;
                    }
                    preRepoFileR.ReadInstallItemList();
                    preRepoFileR.Close();
                    File.Delete(SysConfig.AaiRepoFilePath);
                    File.Move($"{SysConfig.CacheDirPath}\\aai.repo", SysConfig.AaiRepoFilePath);
                    productInfo.SetRepoVersion(major, minor, maintenance, app_match);
                    PreRepoVersion = ProductInfo.RepoVersion;
                    AppMatchVersion = ProductInfo.AppMatchVersion.ToString();
                    PreRepoUpdateExist = "";
                    return true;
                }
            }

            return false;
        }

        private bool CompVersion(string nowVersion, string getVersion)
        {
            string[] nv = nowVersion.Trim("v".ToCharArray()).Split('.');
            string[] gv = getVersion.Trim("v".ToCharArray()).Split('.');

            int verCount = nv.Length < gv.Length ? nv.Length : gv.Length;

            for (int i=0; i<verCount; i++)
            {
                uint n;
                uint g;

                uint.TryParse(nv[i], out n);
                uint.TryParse(gv[i], out g);

                if (n < g)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
