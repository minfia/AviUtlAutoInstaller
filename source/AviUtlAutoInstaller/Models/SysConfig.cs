using System;
using System.Collections.Generic;
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
    }
}
