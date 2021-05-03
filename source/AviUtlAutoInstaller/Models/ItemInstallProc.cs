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
                        AviUtlIniFileBuild();
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


        /// <summary>
        /// AviUtlの設定ファイルを生成
        /// </summary>
        private void AviUtlIniFileBuild()
        {
            string aviutl_ini = $"{SysConfig.InstallRootPath}\\aviutl.ini";
            IniFileRW iniFileRW = new IniFileRW(aviutl_ini);

            {
                Dictionary<string, string> systemConfig = new Dictionary<string, string>()
                {
                    { "width", "2560" }, { "height", "1560" }, { "frame", "320000" }, { "sharecache", "512" },
                    { "sse", "1" }, { "sse2", "1" }, { "vfplugin", "1" },
                    { "moveA", "5" }, { "moveB", "30" }, { "moveC", "899" }, { "moveD", "8991" },
                    { "saveunitsize", "4096" }, { "compprofile", "1" }, { "plugincache", "1" },
                    { "startframe", "1" }, { "shiftselect", "1" }, { "yuy2mode", "0" }, { "movieplaymain", "1" }, { "yuy2limit", "0" }, { "editresume", "0" }, { "fpsnoconvert", "0" },
                    { "tempconfig", "0" }, { "load30fps", "0" }, { "loadfpsadjust", "0" }, { "overwritecheck", "0" }, { "dragdropdialog", "0" }, { "openprojectaup", "1" }, { "closedialog", "1" },
                    { "projectonfig", "0" }, { "windowsnap", "0" }, { "dragdropactive", "1" }, { "trackbarclic", "1" }, { "defaultsavefile", "%p" }, { "finishsound", "" },
                    { "resizelist", "1920x1080,1280x720,640x480,352x240,320x240" },
                    { "fpslist", "*,30000/1001,24000/1001,60000/1001,60,50,30,25,24,20,15,12,10,8,6,5,4,3,2,1" },
                };

                foreach (var pair in systemConfig)
                {
                    iniFileRW.SetValue("system", pair.Key, pair.Value);
                }
            }

            {
                Dictionary<string, string> exeditConfig = new Dictionary<string, string>()
                {
                    { "disp", "1" }, { "new_w", "1280" }, { "new_h", "720" },
                };

                foreach (var pair in exeditConfig)
                {
                    iniFileRW.SetValue("拡張編集", pair.Key, pair.Value);
                }
            }
        }
    }
}
