using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Network.Parser;
using System;
using System.Collections.Generic;
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
            _updateCheckCommand = new DelegateCommand(
                async _ =>
                {
                    await UpdateCheck();
                });
            ApplicationVersion = ProductInfo.ValidAppVersion;
            PreRepoVersion = ProductInfo.RepoVersion;
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

            if (CompVersion(PreRepoVersion, PreRepoGetVersion))
            {
                PreRepoUpdateExist = "アップデートがあります";
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
                ApplicationGetVersion = "アップデートチェックに失敗しました";
                return false;
            }
            ApplicationGetVersion = gitHub.Versions[0];

            if (CompVersion(ApplicationVersion, ApplicationGetVersion))
            {
                ApplicationUpdateExist = "アップデートがあります";
            }

            return true;
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
