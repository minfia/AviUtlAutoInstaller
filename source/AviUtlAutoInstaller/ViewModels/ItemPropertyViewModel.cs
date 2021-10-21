using AviUtlAutoInstaller.Models;
using System.IO;
using System.Windows.Documents;

namespace AviUtlAutoInstaller.ViewModels
{
    class ItemPropertyViewModel : NotificationObject
    {
        private string _title = "";
        /// <summary>
        /// ウィンドウタイトルに表示するアイテム名
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _description = "";
        /// <summary>
        /// アイテムの概要
        /// </summary>
        public string Description
        {
            get { return _description; }
            private set { SetProperty(ref _description, value); }
        }

        private string _makerName = "";
        /// <summary>
        /// 製作者名
        /// </summary>
        public string MakerName
        {
            get { return _makerName; }
            private set { SetProperty(ref _makerName, value); }
        }

        private string _version = "";
        /// <summary>
        /// アイテムのバージョン
        /// </summary>
        public string Version
        {
            get { return _version; }
            private set { SetProperty(ref _version, value); }
        }

        private string _section = "";
        /// <summary>
        /// アイテムのジャンル
        /// </summary>
        public string Section
        {
            get { return _section; }
            private set { SetProperty(ref _section, value); }
        }

        private string _itemType = "";
        /// <summary>
        /// アイテムの種類
        /// </summary>
        public string ItemType
        {
            get { return _itemType; }
            private set { SetProperty(ref _itemType, value); }
        }

        private string _dependent = "";
        /// <summary>
        /// アイテムの依存元
        /// </summary>
        public string Dependent
        {
            get { return _dependent; }
            private set { SetProperty(ref _dependent, value); }
        }

        private string _downloadURL = "";
        /// <summary>
        /// ダウンロード先のURL
        /// </summary>
        public string DownloadURL
        {
            get { return _downloadURL; }
            private set { SetProperty(ref _downloadURL, value); }
        }

        private string _dLStatus = "";
        /// <summary>
        /// ダウンロード状態
        /// </summary>
        public string DLStatus
        {
            get { return _dLStatus; }
            private set { SetProperty(ref _dLStatus, value); }
        }

        private string _guideURL = "";
        /// <summary>
        /// 使い方のURL
        /// </summary>
        public string GuideURL
        {
            get { return _guideURL; }
            private set { SetProperty(ref _guideURL, value); }
        }

        /// <summary>
        /// アイテム情報の設定
        /// </summary>
        /// <param name="item"></param>
        public void SetInstallItem(InstallItem item)
        {
            Description = item.Description;
            MakerName = item.MakerName;
            Version = item.Version;
            Section = item.SectionType;
            ItemType = string.IsNullOrEmpty(item.ScriptDirName) ? (item.SectionType.Equals("本体") ? "本体" : "プラグイン") : "スクリプト";
            Dependent = item.DependentName.Equals("None") ? "なし" : item.DependentName;
            DownloadURL = (string.IsNullOrEmpty(item.URL) ? item.DownloadPage : item.URL);
            DLStatus = File.Exists($"{SysConfig.CacheDirPath}\\{item.DownloadFileName}") ? "ダウンロード済み" : "未ダウンロード";
            GuideURL = string.IsNullOrEmpty(item.NicoVideoID) ? item.GuideURL : $"https://www.nicovideo.jp/watch/{item.NicoVideoID}";
        }

        /// <summary>
        /// URLを開くイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hyperLink = (Hyperlink)sender;
            System.Diagnostics.Process.Start(hyperLink.NavigateUri.ToString());
        }
    }
}
