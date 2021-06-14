using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AviUtlAutoInstaller.ViewModels
{
    class AboutViewModel : NotificationObject
    {
        /// <summary>
        /// アプリ名
        /// </summary>
        public string ApplicationName
        {
            get { return ProductInfo.AppName; }
        }

        /// <summary>
        /// アプリバージョン
        /// </summary>
        public string ApplicationVersion
        {
            get { return $"{ProductInfo.ValidAppVersion}-{ProductInfo.SupportRepoVersion}"; }
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
            PreRepoFileR preRepoFileR = new PreRepoFileR($"{SysConfig.RepoDirPath}\\aai.repo");
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
