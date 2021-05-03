using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    partial class InstallItem
    {
        /// <summary>
        /// インストール
        /// </summary>
        /// <param name="installFileList"></param>
        /// <returns></returns>
        public bool Install(string[] installFileList)
        {
            if (InstallFileType.Encoder == FileType)
            {
                InstallRigayaEncoder();

                return true;
            }
            else
            {
                string installDir = string.Empty;
                switch (FileType)
                {
                    case InstallFileType.Tool:
                        break;
                    case InstallFileType.Main:
                        installDir = SysConfig.InstallRootPath;
                        break;
                    case InstallFileType.Script:
                        installDir = $"{SysConfig.AviUtlScriptDir}\\{ScriptDirName}";
                        if (!Directory.Exists(installDir))
                        {
                            Directory.CreateDirectory(installDir);
                        }
                        break;
                    case InstallFileType.Plugin:
                        if (IsSpecialPlugin(Name))
                        {
                            return InstallSpecialPlugin(Name);
                        }
                        else
                        {
                            installDir = SysConfig.AviUtlPluginDir;
                        }
                        break;
                    case InstallFileType.Image:
                        installDir = SysConfig.AviUtlFigureDir;
                        return true;
                }
                FileOperation fileOperation = new FileOperation();
                fileOperation.FileMove(installFileList, installDir);
            }

            return true;
        }

        private enum SpecialPluginType
        {
            PSDToolkit,
        };

        private static readonly Dictionary<SpecialPluginType, string> _specialPluginDic = new Dictionary<SpecialPluginType, string>()
        {
            { SpecialPluginType.PSDToolkit, "PSDToolKit" },
        };

        /// <summary>
        /// 単純にインストール出来ないプラグインの判定
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private bool IsSpecialPlugin(string name)
        {
            bool b = _specialPluginDic.Any(x => x.Value == name);
            return b;
        }

        /// <summary>
        /// 単純にインストール出来ないプラグインのインストール
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private bool InstallSpecialPlugin(string name)
        {
            var type = _specialPluginDic.First(x => x.Value == name).Key;

            switch (type)
            {
                case SpecialPluginType.PSDToolkit:
                    InstallPSDToolKit();
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// PSDToolKitのインストール
        /// </summary>
        private void InstallPSDToolKit()
        {
            FileOperation fileOperation = new FileOperation();
            string psdSrcPath = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(DownloadFileName)}";

            // 取説の移動
            string psdManualDestPath = $"{SysConfig.InstallRootPath}\\PSDToolKitの説明ファイル群";
            Directory.CreateDirectory(psdManualDestPath);

            string srcDocsPath = $"{psdSrcPath}\\PSDToolKitDocs";
            fileOperation.DirectoryMove(srcDocsPath, $"{psdManualDestPath}\\PSDToolKitDocs");
            Directory.Delete(srcDocsPath, true);
            string[] fileNames = { "GCMZDrops.txt", "PSDToolKit.txt", "PSDToolKit説明書.html", "ZRamPreview.txt", "キャッシュテキスト.txt" };
            List<string> srcFileNamePath = new List<string>();
            foreach (string fileName in fileNames)
            {
                srcFileNamePath.Add($"{psdSrcPath}\\{fileName}");
            }
            fileOperation.FileMove(srcFileNamePath.ToArray(), psdManualDestPath);

            // 本体の移動
            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(DownloadFileName)}", $"{SysConfig.AviUtlPluginDir}");
        }


        private void InstallRigayaEncoder()
        {
            FileOperation fileOperation = new FileOperation();
            string tempDir = Path.GetTempPath();
            var s = DownloadFileName.Split('_');
            string searchDirName = $"{s[0]}_{s[1]}";

            string extractDirPath = $"{tempDir}{searchDirName}";

            // すでにエンコーダのディレクトリが存在していたら削除
            {
                if (Directory.Exists(extractDirPath))
                {
                    Directory.Delete(extractDirPath, true);
                }
            }

            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(DownloadFileName)}", tempDir);

            string[] exe = { "auo_setup.exe" };
            var exeList = fileOperation.GenerateFilePathList(extractDirPath, exe);

            foreach (var exeFile in exeList)
            {
                if (!fileOperation.ExecApp(exeFile, $"-autorun -nogui -dir \"{SysConfig.InstallRootPath}\"", FileOperation.ExecAppType.CUI, out Process process))
                {
                    break;
                }

                process.WaitForExit();
            }

            // エンコーダのディレクトリを削除
            {
                if (Directory.Exists(extractDirPath))
                {
                    Directory.Delete(extractDirPath, true);
                }
            }
        }
    }
}
