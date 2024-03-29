﻿using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using System.Windows.Documents;

namespace AviUtlAutoInstaller.ViewModels
{
    class AboutViewModel : NotificationObject
    {
        private string _iconPath;
        public string IconPath
        {
            get { return _iconPath; }
            private set { SetProperty(ref _iconPath, value); }
        }

        /// <summary>
        /// アプリ名
        /// </summary>
        public string ApplicationName
        {
            get { return ProductInfo.Product; }
        }

        /// <summary>
        /// アプリバージョン
        /// </summary>
        public string ApplicationVersion
        {
            get
            {
                return $"{ProductInfo.ValidAppVersion}-{ProductInfo.SupportRepoVersion}"
#if EARLY
                       + $" Early {ProductInfo.EralyVersion}"
#endif
                       ;
            }
        }

        private string _preRepoVersion;
        /// <summary>
        /// プリインストールのバージョン
        /// </summary>
        public string PreRepoVersion
        {
            get { return _preRepoVersion; }
            private set { SetProperty(ref _preRepoVersion, value); }
        }

        public AboutViewModel()
        {
#if EARLY
            IconPath = "..\\Assets\\app_early.ico";
#else
            IconPath = "..\\Assets\\app.ico";
#endif
            PreRepoFileR preRepoFileR = new($"{SysConfig.RepoDirPath}\\aai.repo");
            preRepoFileR.Open();
            preRepoFileR.GetDBVersion(out uint major, out uint minor, out uint maintenance, out uint app_match);
            preRepoFileR.Close();
            PreRepoVersion = $"{major}.{minor}.{maintenance}-{app_match}";
        }

        public void OpenLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hyperLink = (Hyperlink)sender;
            System.Diagnostics.Process.Start(hyperLink.NavigateUri.ToString());
        }
    }
}
