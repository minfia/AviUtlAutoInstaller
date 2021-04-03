using AviUtlAutoInstaller.ViewModels;
using AviUtlAutoInstaller.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            if (IsAdministrator())
            {
                MessageBox.Show("管理者権限では実行できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
