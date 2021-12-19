using AviUtlAutoInstaller.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AAIUpdater.Models
{
    class Updater
    {
#if DEBUG
        /// <summary>
        /// App更新時削除除外ディレクトリリスト
        /// </summary>
        private static List<string> delExcludeDirNames = new List<string>();

        /// <summary>
        /// App更新時削除除外ファイルリスト
        /// </summary>
        private static List<string> delExcludeFileNames = new List<string>()
        {
            "updater.exe",
            "updater.exe.config",
            "updater.pdb"
        };
#else
        /// <summary>
        /// App更新時削除除外ディレクトリリスト
        /// </summary>
        private static List<string> delExcludeDirNames = new List<string>();

        /// <summary>
        /// App更新時削除除外ファイルリスト
        /// </summary>
        private static List<string> delExcludeFileNames = new List<string>()
        {
            "updater.exe",
            "updater.exe.config",
        };
#endif

        /// <summary>
        /// 引数から取得した削除除外ディレクトリを設定
        /// </summary>
        /// <param name="excludeDirNames"></param>
        public void SetDeleteExcludeDir(string[] excludeDirNames)
        {
            foreach (string dirName in excludeDirNames)
            {
                delExcludeDirNames.Add(dirName);
            }
        }

        /// <summary>
        /// 引数から取得した削除除外ファイルを設定
        /// </summary>
        /// <param name="excludeFileNames"></param>
        public void SetDeleteExcludeFile(string[] excludeFileNames)
        {
            foreach (string fileName in excludeFileNames)
            {
                delExcludeFileNames.Add(fileName);
            }
        }

        /// <summary>
        /// アプリアップデート
        /// </summary>
        /// <param name="filePath"></param>
        public void AppUpdate(string filePath)
        {
            {
                // 現存するファイル郡を削除
                var dirList = Directory.GetDirectories(".\\").ToList();
                foreach (var delName in delExcludeDirNames)
                {
                    dirList.Remove($".\\{delName}");
                }

                var fileList = Directory.GetFiles(".\\").ToList();
                foreach (var delName in delExcludeFileNames)
                {
                    fileList.Remove($".\\{delName}");
                }

                foreach (var dir in dirList)
                {
                    Directory.Delete(dir, true);
                }
                foreach (var file in fileList)
                {
                    File.Delete(file);
                }
            }

            {
                // DLしたファイルを展開、コピーする
                string extractDir = ".\\cache\\AAI";
                // すでに、展開先のディレクトリがあったら削除
                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                }
    
                ZipFile.ExtractToDirectory(filePath, extractDir);

                var dirList = Directory.GetDirectories(extractDir).ToList();
                foreach (var dirName in delExcludeDirNames)
                {
                    dirList.Remove($"{extractDir}\\{dirName}");
                }

                var fileList = Directory.GetFiles(extractDir).ToList();
                foreach (var fileName in delExcludeFileNames)
                {
                    fileList.Remove($"{extractDir}\\{fileName}");
                }

                FileOperation fileOperation = new FileOperation();

                var excludeFileList = new List<string>();
                foreach (var fileName in delExcludeFileNames)
                {
                    excludeFileList.Add($"{extractDir}\\{fileName}");
                }

                fileOperation.DirectoryMove(extractDir, ".\\", excludeFileList.ToArray());

                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                }
            }
        }

        /// <summary>
        /// プリインストールリポジトリアップデート
        /// </summary>
        /// <param name="filePath"></param>
        public void RepoUpdate(string filePath)
        {
            FileOperation fileOperation = new FileOperation();
            fileOperation.FileMove(new string[] { filePath }, ".\\repo");
        }
    }
}
