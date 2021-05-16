using AviUtlAutoInstaller.Models;
using AviUtlAutoInstaller.ViewModels;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace AviUtlAutoInstaller
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
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

                if (!preRepoFileR.GetDBVersion(out uint major, out uint minor) || !preRepoFileR.ReadInstallItemList())
                {
                    MessageBox.Show("aai.repoを読み込めませんでした\nアプリをダウンロードし直すか、aai.repoをダウンロードしてください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }
                preRepoFileR.Close();

                ProductInfo productInfo = new ProductInfo();
                productInfo.SetRepoVersion(major, minor);
            }

            var mv = new MainView();
            var mvm = new MainViewModel();

            mv.DataContext = mvm;
            mv.Show();
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
