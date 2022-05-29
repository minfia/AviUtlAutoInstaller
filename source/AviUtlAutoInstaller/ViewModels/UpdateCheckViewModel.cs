using AviUtlAutoInstaller.Models;
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
        #region プリインストール
        private string _preRepoVersion;
        /// <summary>
        /// 現在のプリインストールリポジトリのバージョン
        /// </summary>
        public string PreRepoVersion
        {
            get { return _preRepoVersion; }
            private set { SetProperty(ref _preRepoVersion, value); }
        }

        private string _preRepoGetVersion;
        /// <summary>
        /// 取得したプリインストールリポジトリのバージョン
        /// </summary>
        public string PreRepoGetVersion
        {
            get { return _preRepoGetVersion; }
            private set { SetProperty(ref _preRepoGetVersion, value); }
        }

        private string _preRepoUpdateExist;
        public string PreRepoUpdateMsg
        {
            get { return _preRepoUpdateExist; }
            private set { SetProperty(ref _preRepoUpdateExist, value); }
        }

        /// <summary>
        /// プリインストールリポジトリのダウンロードURL
        /// </summary>
        private string _preRepoURL = "";
        private bool _preRepoUpdateEnable = false;
        /// <summary>
        /// プリインストールリポジトリ更新ボタンの有効/無効
        /// </summary>
        public bool PreRepoUpdateEnable
        {
            get { return _preRepoUpdateEnable; }
            private set { SetProperty(ref _preRepoUpdateEnable, value); }
        }

        /// <summary>
        /// 現在のプリインストールリポジトリが対応するバージョン
        /// </summary>
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

        private readonly DelegateCommand _preRepoUpdateCommand;
        public DelegateCommand PreRepoUpdateCommand { get => _preRepoUpdateCommand; }
        #endregion

        #region アプリケーション
        private string _applicationVersion;
        /// <summary>
        /// 現在のアプリバージョン
        /// </summary>
        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            private set { SetProperty(ref _applicationVersion, value); }
        }

        private string _applicationGetVersion;
        /// <summary>
        /// 取得したアプリバージョン
        /// </summary>
        public string ApplicationGetVersion
        {
            get { return _applicationGetVersion; }
            private set { SetProperty(ref _applicationGetVersion, value); }
        }

        /// <summary>
        /// アプリケーションのダウンロードURL
        /// </summary>
        private string _appURL = "";
        private bool _appUpdateEnable = false;
        /// <summary>
        /// アプリケーション更新ボタンの有効/無効
        /// </summary>
        public bool AppUpdateEnable
        {
            get { return _appUpdateEnable; }
            private set { SetProperty(ref _appUpdateEnable, value); }
        }
        private string _applicationUpdateMsg;
        public string ApplicationUpdateMsg
        {
            get { return _applicationUpdateMsg; }
            private set { SetProperty(ref _applicationUpdateMsg, value); }
        }

        /// <summary>
        /// アプリが対応するプリインストールリポジトリバージョン
        /// </summary>
        public string SupportRepoVersion { get; private set; }

        private readonly DelegateCommand _appUpdateCommand;
        public DelegateCommand AppUpdateCommand { get => _appUpdateCommand; }
        #endregion

        private bool _updateCheckButtonEnable = true;
        /// <summary>
        /// 更新確認ボタンの有効/無効
        /// </summary>
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

        private readonly DelegateCommand _updateCheckCommand;
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
            _appUpdateCommand = new DelegateCommand(
                _ =>
                {
                    AppUpdateEnable = UpdateCheckButtonEnable = false;
                    AppUpdate();
                });
            _updateCheckCommand = new DelegateCommand(
                async _ =>
                {
                    PreRepoGetVersion = PreRepoUpdateMsg = "";
                    ApplicationGetVersion = ApplicationUpdateMsg = "";
                    await OnUpdateCheck();
                });
            ApplicationVersion = ProductInfo.ValidAppVersion;
            PreRepoVersion = ProductInfo.RepoVersion;
            AppMatchVersion = ProductInfo.AppMatchVersion.ToString();
            SupportRepoVersion = ProductInfo.SupportRepoVersion.ToString();
        }

        /// <summary>
        /// アップデートチェック
        /// </summary>
        /// <returns></returns>
        private async Task<bool> OnUpdateCheck()
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
            UpdateCheck updateCheck = new();

            var res = updateCheck.Check(UpdateCheck.CheckTarget.PreRepo, PreRepoVersion, out string getVersion, out _preRepoURL);
            switch (res)
            {
                case UpdateCheck.CheckResult.Failed:
                    PreRepoUpdateMsg = "アップデートチェックに失敗しました";
                    PreRepoUpdateEnable = false;
                    return false;
                case UpdateCheck.CheckResult.NoUpdate:
                    PreRepoUpdateMsg = "";
                    PreRepoGetVersion = getVersion;
                    PreRepoUpdateEnable = false;
                    break;
                case UpdateCheck.CheckResult.Update:
                    PreRepoUpdateMsg = "アップデートがあります";
                    PreRepoGetVersion = getVersion;
                    PreRepoUpdateEnable = true;
                    break;
            }

            return true;
        }

        /// <summary>
        /// アプリのアップデートチェック
        /// </summary>
        /// <returns>true: 正常終了, false: 異常終了</returns>
        private bool AppUpdateCheck()
        {
            UpdateCheck updateCheck = new();

            var res = updateCheck.Check(UpdateCheck.CheckTarget.App, ApplicationVersion, out string getVersion, out _appURL);
            switch (res)
            {
                case UpdateCheck.CheckResult.Failed:
                    ApplicationUpdateMsg = "アップデートチェックに失敗しました";
                    AppUpdateEnable = false;
                    return false;
                case UpdateCheck.CheckResult.NoUpdate:
                    ApplicationUpdateMsg = "";
                    ApplicationGetVersion = getVersion;
                    AppUpdateEnable = false;
                    break;
                case UpdateCheck.CheckResult.Update:
                    ApplicationUpdateMsg = "アップデートがあります";
                    ApplicationGetVersion = getVersion;
                    AppUpdateEnable = true;
                    break;
            }

            return true;
        }

        /// <summary>
        /// プリインストールリポジトリの更新
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PreRepoUpdate()
        {
            UpdateCheck updateCheck = new();

            Task<UpdateCheck.RepoUpdateResult> task = Task.Run(() => updateCheck.UpdatePreRepo(_preRepoURL));
            await task;

            switch (task.Result)
            {
                case UpdateCheck.RepoUpdateResult.UnSupported:
                    MessageBox.Show("新しいバージョンは、このアプリケーションのバージョンでは使用できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                case UpdateCheck.RepoUpdateResult.Failed:
                    MessageBox.Show("アップデートに失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                case UpdateCheck.RepoUpdateResult.Success:
                    MessageBox.Show("アップデートしました", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    PreRepoVersion = ProductInfo.RepoVersion;
                    AppMatchVersion = ProductInfo.AppMatchVersion.ToString();
                    PreRepoUpdateMsg = "";
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// アプリケーションの更新
        /// </summary>
        private void AppUpdate()
        {
            if (!File.Exists(SysConfig.UpdaterFilePath))
            {
                MessageBox.Show("updater.exeがありません、アプリケーションを再ダウンロードしてください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                AppUpdateEnable = UpdateCheckButtonEnable = true;
                return;
            }
            UpdateCheck updateCheck = new();

            updateCheck.UpdateApplication(_appURL);
            App.Current.Shutdown();
        }
    }
}
