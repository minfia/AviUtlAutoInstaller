using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.Models.Files;
using AviUtlAutoInstaller.ViewModels;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AviUtlAutoInstaller
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            Mutex mutex;
            if (IsMultipleStart(out mutex))
            {
                try
                {
                    App app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
                finally
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
            else
            {
                MessageBox.Show("多重起動はできません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                mutex.Close();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string message = "";
            if (IsAdministrator())
            {
                message = "管理者権限では実行できません";
            }
            else if (!Directory.Exists(SysConfig.RepoDirPath))
            {
                message = "repoフォルダが存在しません\nアプリをダウンロードし直してください";
            }
            else if (!File.Exists(SysConfig.AaiRepoFilePath))
            {
                message = $"{SysConfig.RepoDirPath}にaai.repoが存在しません\nアプリをダウンロードし直すか、aai.repoをダウンロードしてください";
            }
            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            if (!Directory.Exists(SysConfig.UserRepoDirPath))
            {
                Directory.CreateDirectory(SysConfig.UserRepoDirPath);
            }
            if (!Directory.Exists(SysConfig.CacheDirPath))
            {
                Directory.CreateDirectory(SysConfig.CacheDirPath);
            }

            {
                PreRepoFileR preRepoFileR = new PreRepoFileR($"{SysConfig.RepoDirPath}\\aai.repo");
                preRepoFileR.Open();

                if (!preRepoFileR.GetDBVersion(out uint major, out uint minor, out uint maintenance, out uint app_match))
                {
                    MessageBox.Show("aai.repoのバージョンを読み込めませんでした\naai.repoをダウンロードし直してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    preRepoFileR.Close();
                    Current.Shutdown();
                    return;
                }

                ProductInfo productInfo = new ProductInfo();
                if (!productInfo.IsSupportRepoVersion(app_match))
                {
                    MessageBox.Show($"アプリのバージョン({ProductInfo.SupportRepoVersion})とaai.repoのバージョン({app_match})とが一致しません\n" +
                                    $"バージョンを合わせるためにアプリをダウンロードし直すか、aai.repoをダウンロードしてください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    preRepoFileR.Close();
                    Current.Shutdown();
                    return;
                }
                preRepoFileR.ReadSectionList();
                preRepoFileR.ReadMakerList();
                preRepoFileR.ReadInstallItemList();
                preRepoFileR.Close();
                productInfo.SetRepoVersion(major, minor, maintenance, app_match);
            }

            AppConfig.Load();

            var mv = new MainView();
            mv.Show();
        }

        /// <summary>
        /// 多重起動チェック
        /// </summary>
        /// <param name="mutex"></param>
        /// <returns></returns>
        private static bool IsMultipleStart(out Mutex mutex)
        {
            string mutexName = "Global\\MAviUtlAutoInstaller";
            mutex = new Mutex(true, mutexName, out bool createdNew);

            return createdNew;
        }

        /// <summary>
        /// 管理者権限チェック
        /// </summary>
        /// <returns></returns>
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
