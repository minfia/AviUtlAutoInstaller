using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    class SysConfig
    {
        /// <summary>
        /// ダウンロードファイルキャッシュのディレクトリパス
        /// </summary>
        public static readonly string CacheDirPath = ".\\cache";

        /// <summary>
        /// プリインストールリポジトリファイルのディレクトリパス
        /// </summary>
        public static readonly string RepoDirPath = ".\\repo";

        /// <summary>
        /// ユーザーリポジトリファイルのディレクトリパス
        /// </summary>
        public static readonly string UserRepoDirPath = $"{RepoDirPath}\\user_repo";

        /// <summary>
        /// プリインストールリポジトリのファイルパス
        /// </summary>
        public static readonly string AaiRepoFilePath = $"{RepoDirPath}\\aai.repo";

        private static string _installRootPath = $"{Path.GetFullPath(".")}\\AviUtl";
        /// <summary>
        /// インストール先のルートディレクトリパス
        /// </summary>
        public static string InstallRootPath
        {
            get { return _installRootPath; }
            set
            {
                _installRootPath = value;
                InstallExpansionDir = $"{_installRootPath}\\EX_TEMP";
                InstallFileBackupDir = $"{_installRootPath}\\backup_files";
                AviUtlPluginDir = $"{_installRootPath}\\plugins";
                AviUtlScriptDir = $"{AviUtlPluginDir}\\script";
                AviUtlFigureDir = $"{AviUtlPluginDir}\\figure";
            }
        }

        /// <summary>
        /// プラグインのディレクトリパス
        /// </summary>
        public static string AviUtlPluginDir { get; private set; }

        /// <summary>
        /// スクリプトのディレクトリパス
        /// </summary>
        public static string AviUtlScriptDir { get; private set; }

        /// <summary>
        /// 図形のディレクトリパス
        /// </summary>
        public static string AviUtlFigureDir { get; private set; }

        /// <summary>
        /// インストールデータのバックアップのディレクトリパス
        /// </summary>
        public static string InstallFileBackupDir { get; private set; }

        /// <summary>
        /// ファイルの展開先のディレクトリパス
        /// </summary>
        public static string InstallExpansionDir { get; private set; }

        /// <summary>
        /// プラグイン拡張子
        /// </summary>
        public static string[] PluginFileExtension = { "*.auf", "*.aui", "*.auo", "*.auc", "*.aul" };

        /// <summary>
        /// スクリプト拡張子
        /// </summary>
        public static string[] ScriptFileExtension = { "*.anm", "*.obj", "*.scn", "*.cam" };

    }
}
