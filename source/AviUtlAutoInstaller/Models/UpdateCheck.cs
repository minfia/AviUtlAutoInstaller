using AviUtlAutoInstaller.Models.Files;
using AviUtlAutoInstaller.Models.Network;
using AviUtlAutoInstaller.Models.Network.Parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace AviUtlAutoInstaller.Models
{
    class UpdateCheck
    {
        public enum CheckResult
        {
            /// <summary>
            /// アップデートあり
            /// </summary>
            Update,
            /// <summary>
            /// アップデートなし
            /// </summary>
            NoUpdate,
            /// <summary>
            /// アップデートチェック失敗
            /// </summary>
            Failed
        }

        public enum CheckTarget
        {
            /// <summary>
            /// プリインストールリポジトリ
            /// </summary>
            PreRepo,
            /// <summary>
            /// アプリケーション
            /// </summary>
            App
        }

        public enum RepoUpdateResult
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success,
            /// <summary>
            /// 失敗
            /// </summary>
            Failed,
            /// <summary>
            /// アプリが非サポート
            /// </summary>
            UnSupported
        }


        //private string _preRepoGitHubUrl = "https://api.github.com/repos/minfia/AAI_Repo/releases/latest";
        //private string _applicationGitHubUrl = "https://api.github.com/repos/minfia/AviUtlAutoInstaller/releases/latest";

        /// <summary>
        /// プリインストールリポジトリのGitHub APIのURL
        /// </summary>
        private readonly string _preRepoGitHubUrl = "https://api.github.com/repos/minfia/AAI_Repo/releases";
        /// <summary>
        /// アプリケーションのGitHub APIのURL
        /// </summary>
        private readonly string _applicationGitHubUrl = "https://api.github.com/repos/minfia/AviUtlAutoInstaller/releases";

        /// <summary>
        /// 削除除外ディレクトリリスト
        /// </summary>
        private readonly List<string> _delExcludeDirList = new()
        {
            "cache", "repo", "AviUtl"
        };

        /// <summary>
        /// 削除除外ファイルリスト
        /// </summary>
        private List<string> _delExcludeFileList = new()
        {
        };

        /// <summary>
        /// アップデートチェックを行う
        /// </summary>
        /// <param name="target">アップデートチェック対象</param>
        /// <param name="nowVersion">現在のバージョン</param>
        /// <param name="getVersion">取得したバージョン</param>
        /// <param name="downloadURL">ダウンロードURL</param>
        /// <returns>チェック結果</returns>
        public CheckResult Check(CheckTarget target, string nowVersion, out string getVersion, out string downloadURL)
        {

            getVersion = "";
            downloadURL = "";
            string url = "";

            if (!Enum.IsDefined(typeof(CheckTarget), target))
            {
                return CheckResult.Failed;
            }

            switch (target)
            {
                case CheckTarget.PreRepo:
                    url = _preRepoGitHubUrl;
                    break;
                case CheckTarget.App:
                    url = _applicationGitHubUrl;
                    break;
            }

            GitHub github = new();

            Uri uri = new(url);
            try
            {
                if (!github.Parse(uri))
                {
                    return CheckResult.Failed;
                }
            }
            catch
            {
                return CheckResult.Failed;
            }
            getVersion = github.Versions[0];
            downloadURL = github.VersionLink[getVersion];

            if (CompareVersion(nowVersion, getVersion))
            {
                return CheckResult.Update;
            }

            return CheckResult.NoUpdate;
        }

        /// <summary>
        /// バージョン比較
        /// </summary>
        /// <param name="srcVersion">比較対象</param>
        /// <param name="targetVersion">比較するバージョン</param>
        /// <returns>true: 最新あり, false: 最新なし</returns>
        private bool CompareVersion(string srcVersion, string targetVersion)
        {
            string[] sv = srcVersion.Trim("v".ToCharArray()).Split(('.'));
            string[] tv = targetVersion.Trim("v".ToCharArray()).Split(('.'));

            int verCount = sv.Length < tv.Length ? sv.Length : tv.Length;

            for (int i = 0; i < verCount; i++)
            {
                uint.TryParse(sv[i], out uint s);
                uint.TryParse(tv[i], out uint t);

                if (s > t)
                {
                    return false;
                }
                else if (s < t)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// プリインストールリポジトリのアップデート
        /// </summary>
        /// <param name="url">プリインストールリポジトリのURL</param>
        /// <returns>実行結果</returns>
        public RepoUpdateResult UpdatePreRepo(string url)
        {
            Downloader downloader = new($"{SysConfig.CacheDirPath}");

            string fileName = "aai.repo";
            var res = downloader.DownloadStart(url, fileName);
            string cacheFile = $"{SysConfig.CacheDirPath}\\{fileName}";

            if ((res == DownloadResult.Complete) && File.Exists(cacheFile))
            {
                PreRepoFileR preRepoFileR = new(cacheFile);
                preRepoFileR.Open();
                preRepoFileR.GetDBVersion(out uint major, out uint minor, out uint maintenance, out uint app_match);
                ProductInfo productInfo = new();

                if (!productInfo.IsSupportRepoVersion(app_match))
                {
                    preRepoFileR.Close();
                    File.Delete(cacheFile);
                    return RepoUpdateResult.UnSupported;
                }

                preRepoFileR.ReadInstallItemList();
                preRepoFileR.Close();
                File.Delete(SysConfig.AaiRepoFilePath);
                File.Move(cacheFile, SysConfig.AaiRepoFilePath);
                productInfo.SetRepoVersion(major, minor, maintenance, app_match);

                return RepoUpdateResult.Success;
            }

            return RepoUpdateResult.Failed;
        }

        /// <summary>
        /// アプリケーションのアップデート
        /// </summary>
        /// <param name="url">アプリケーションのURL</param>
        public void UpdateApplication(string url)
        {
            FileOperation fileOperation = new();

            string delExcludeDirArgs = "";
            foreach (string arg in _delExcludeDirList)
            {
                delExcludeDirArgs += $" {arg}";
            }

            string delExcludeFileArgs = "";
            foreach (string arg in _delExcludeFileList)
            {
                delExcludeFileArgs += $" {arg}";
            }

            string cmd = $"--app {url} --exclude-dirs {delExcludeDirArgs} --exclude-files {delExcludeFileArgs}";
            fileOperation.ExecApp(SysConfig.UpdaterFilePath, cmd, FileOperation.ExecAppType.GUI, out System.Diagnostics.Process proc);
        }

        /// <summary>
        /// アプリケーションのアップデート
        /// </summary>
        /// <param name="appURL">アプリケーションのURL</param>
        /// <param name="preRepoURL">プリインストールリポジトリのURL</param>
        public void UpdateApplication(string appURL, string preRepoURL)
        {
            FileOperation fileOperation = new();

            string delExcludeDirArgs = "";
            foreach (string arg in _delExcludeDirList)
            {
                delExcludeDirArgs += $" {arg}";
            }

            string delExcludeFileArgs = "";
            foreach (string arg in _delExcludeFileList)
            {
                delExcludeFileArgs += $" {arg}";
            }

            string cmd = $"--app {appURL} --repo {preRepoURL} --exclude-dirs {delExcludeDirArgs} --exclude-files {delExcludeFileArgs}";
            fileOperation.ExecApp(SysConfig.UpdaterFilePath, cmd, FileOperation.ExecAppType.GUI, out System.Diagnostics.Process proc);
        }
    }
}
