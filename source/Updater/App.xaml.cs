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

            if (Environment.GetCommandLineArgs().Length <= 1)
            {
                Shutdown();
            }
            else
            {
                var mv = new MainView();
                mv.Show();
            }
        }
    }
}
