using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AAIUpdater.ViewModels;
using AAIUpdater.Views;

namespace AAIUpdater
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mv = new MainView();
            var mvm = new MainViewModel();

            mv.DataContext = mvm;
            mv.Show();
        }
    }
}
