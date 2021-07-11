using AviUtlAutoInstaller.Models.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
        public static bool Install(InstallItem item, string[] installFileList)
        {
            if (InstallFileType.Encoder == item.FileType)
            {
                InstallRigayaEncoder(item.DownloadFileName);

                return true;
            }
            else
            {
                string installDir = string.Empty;
                switch (item.FileType)
                {
                    case InstallFileType.Tool:
                        break;
                    case InstallFileType.Main:
                        installDir = SysConfig.InstallRootPath;
                        AviUtlIniFileBuild();
                        break;
                    case InstallFileType.Script:
                        installDir = $"{SysConfig.AviUtlScriptDir}\\{item.ScriptDirName}";
                        if (!Directory.Exists(installDir))
                        {
                            Directory.CreateDirectory(installDir);
                        }
                        break;
                    case InstallFileType.Plugin:
                        if (IsSpecialPlugin(item.Name))
                        {
                            return InstallSpecialPlugin(item);
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

        /// <summary>
        /// アンインストール
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Uninstall(InstallItem item)
        {
            string baseDir = $"{SysConfig.InstallRootPath}";

            switch (item.FileType)
            {
                case InstallFileType.Tool:
                    baseDir = $"{SysConfig.InstallRootPath}";
                    break;
                case InstallFileType.Plugin:
                    if (IsSpecialPlugin(item.Name))
                    {
                        return UninstallSpecialPlugin(item);
                    }
                    else
                    {
                        baseDir = $"{SysConfig.AviUtlPluginDir}";
                    }
                    break;
                case InstallFileType.Script:
                    baseDir = $"{SysConfig.AviUtlScriptDir}";
                    break;
            }

            FileOperation fileOperation = new FileOperation();
            List<string> uninstallFileList = fileOperation.GenerateFilePathList(baseDir, item.InstallFileList.ToArray());
            foreach (var file in uninstallFileList)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private enum ExternalFileType
        {
            None,
            VSRuntime,
        }

        private static readonly Dictionary<string, ExternalFileType> _externalFileDic = new Dictionary<string, ExternalFileType>()
        {
            { "vc2008redist_x86.exe", ExternalFileType.VSRuntime },
            { "vc2008redist_x64.exe", ExternalFileType.VSRuntime },
            { "vc2013redist_x86.exe", ExternalFileType.VSRuntime },
            { "vc2013redist_x64.exe", ExternalFileType.VSRuntime },
            { "vc201Xredist.x86.exe", ExternalFileType.VSRuntime },
            { "vc201Xredist.x64.exe", ExternalFileType.VSRuntime },
        };

        /// <summary>
        /// 外部ソフトウェアのインストール
        /// </summary>
        /// <param name="externalFiles"></param>
        /// <returns></returns>
        public static bool ExternalInstall(string[] externalFiles)
        {
            foreach (string exFile in externalFiles)
            {
                if (_externalFileDic.ContainsKey(exFile))
                {
                    string exFilePath = $"{SysConfig.CacheDirPath}\\{exFile}";

                    var type = _externalFileDic[exFile];
                    string args;
                    switch (type)
                    {
                        case ExternalFileType.VSRuntime:
                            {
                                if (exFile.Contains("vc2008") && !Properties.Settings.Default.vs2008runtime)
                                {
                                    args = "/q";
                                }
                                else if (exFile.Contains("vc2013") && !Properties.Settings.Default.vs2013runtime)
                                {
                                    args = "quiet";
                                }
                                else if (exFile.Contains("vc201X") && !Properties.Settings.Default.vs201Xruntime)
                                {
                                    args = "quiet";
                                }
                                else
                                {
                                    break;
                                }
                                if (VSRuntimeInstall(exFilePath, args))
                                {
                                    if (exFile.Contains("vc2008"))
                                    {
                                        Properties.Settings.Default.vs2008runtime = true;
                                    }
                                    else if (exFile.Contains("vc2013"))
                                    {
                                        Properties.Settings.Default.vs2013runtime = true;
                                    }
                                    else if (exFile.Contains("vc201X"))
                                    {
                                        Properties.Settings.Default.vs201Xruntime = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// VisualStudioのランタイムインストール
        /// </summary>
        /// <param name="filePath">インストーラのパス</param>
        /// <param name="args">インストーラの引数</param>
        /// <returns>成否</returns>
        private static bool VSRuntimeInstall(string filePath, string args)
        {
            FileOperation fileOperation = new FileOperation();

            try
            {
                if (!fileOperation.ExecApp(filePath, args, FileOperation.ExecAppType.CUI, out Process process))
                {
                    return false;
                }
                process.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
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
        private static bool IsSpecialPlugin(string name)
        {
            bool b = _specialPluginDic.Any(x => x.Value == name);
            return b;
        }

        /// <summary>
        /// 単純にインストール出来ないプラグインのインストール
        /// </summary>
        /// <param name="item">インストールアイテム</param>
        /// <returns></returns>
        private static bool InstallSpecialPlugin(InstallItem item)
        {
            var type = _specialPluginDic.First(x => x.Value == item.Name).Key;

            switch (type)
            {
                case SpecialPluginType.PSDToolkit:
                    InstallPSDToolKit(item.DownloadFileName);
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// PSDToolKitのインストール
        /// </summary>
        private static void InstallPSDToolKit(string downloadFileName)
        {
            FileOperation fileOperation = new FileOperation();
            string psdSrcPath = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}";

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
            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}", $"{SysConfig.AviUtlPluginDir}");
        }

        /// <summary>
        /// 単純にインストール出来ないプラグインのアンインストール
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool UninstallSpecialPlugin(InstallItem item)
        {
            var type = _specialPluginDic.First(x => x.Value == item.Name).Key;

            switch (type)
            {
                case SpecialPluginType.PSDToolkit:
                    UninstallPSDToolKit();
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// PSDToolKitのアンインストール
        /// </summary>
        private static void UninstallPSDToolKit()
        {
            string psdManualDirPath = $"{SysConfig.InstallRootPath}\\PSDToolKitの説明ファイル群";
            Directory.Delete(psdManualDirPath, true);
            string[] psdFileDirPath = new string[] {
                $"{SysConfig.AviUtlPluginDir}\\GCMZDrops",
                $"{SysConfig.AviUtlPluginDir}\\PSDToolKit",
                $"{SysConfig.AviUtlPluginDir}\\かんしくん",
                $"{SysConfig.AviUtlScriptDir}\\PSDToolKit",
            };

            foreach (string path in psdFileDirPath)
            {
                Directory.Delete(path, true);
            }

            string[] psdFilePath = new string[] {
                $"{SysConfig.AviUtlPluginDir}\\AudioMixer.auf",
                $"{SysConfig.AviUtlPluginDir}\\GCMZDrops.auf",
                $"{SysConfig.AviUtlPluginDir}\\PSDToolKit.auf",
                $"{SysConfig.AviUtlPluginDir}\\ZRamPreview.auf",
                $"{SysConfig.AviUtlPluginDir}\\ZRamPreview.auo",
                $"{SysConfig.AviUtlPluginDir}\\ZRamPreview.exe",
                $"{SysConfig.AviUtlPluginDir}\\キャッシュテキスト.exa",
                $"{SysConfig.AviUtlScriptDir}\\CacheText.anm",
                $"{SysConfig.AviUtlScriptDir}\\CacheText.lua",
                $"{SysConfig.AviUtlScriptDir}\\Extram.dll",
                $"{SysConfig.AviUtlScriptDir}\\PSDToolKit.lua"
            };

            foreach (string path in psdFilePath)
            {
                File.Delete(path);
            }

        }

        private static void InstallRigayaEncoder(string downloadFileName)
        {
            FileOperation fileOperation = new FileOperation();
            string tempDir = Path.GetTempPath();
            var s = downloadFileName.Split('_');
            string searchDirName = $"{s[0]}_{s[1]}";

            string extractDirPath = $"{tempDir}{searchDirName}";

            // すでにエンコーダのディレクトリが存在していたら削除
            {
                if (Directory.Exists(extractDirPath))
                {
                    Directory.Delete(extractDirPath, true);
                }
            }

            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}", tempDir);

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
        private static void AviUtlIniFileBuild()
        {
            string aviutl_ini = $"{SysConfig.InstallRootPath}\\aviutl.ini";
            IniFileRW iniFileRW = new IniFileRW(aviutl_ini);

            {
                uint memSize = GetPhysicalMemSize();
                uint cacheSize = GetCacheSize(memSize);
                Dictionary<string, string> systemConfig = new Dictionary<string, string>()
                {
                    { "width", "2560" }, { "height", "1560" }, { "frame", "320000" }, { "sharecache", $"{cacheSize}" },
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

        /// <summary>
        /// 搭載されている物理メモリサイズを取得
        /// </summary>
        /// <returns>物理メモリサイズ[MB]</returns>
        private static uint GetPhysicalMemSize()
        {
            double getMemSize = 0;
            using (ManagementObjectCollection moc = new ManagementClass("Win32_OperatingSystem").GetInstances())
            {
                foreach (var mo in moc)
                {
                    double.TryParse(mo["TotalVisibleMemorySize"].ToString(), out getMemSize);
                    mo.Dispose();
                    break;
                }
            }

            const uint gb = 1048576;
            uint memSize = (uint)((getMemSize / gb) + 0.5);

            return memSize;
        }

        /// <summary>
        /// 設定するキャッシュサイズを取得
        /// </summary>
        /// <param name="memSize">物理メモリサイズ[GB]</param>
        /// <returns>キャッシュサイズ[MB]</returns>
        private static uint GetCacheSize(uint memSize)
        {
            uint cacheSize = 256;

            if (8 < memSize)
            {
                cacheSize = (memSize / 4) * 1024;
            }
            else if (4 < memSize)
            {
                cacheSize = 512;
            }

            return cacheSize;
        }

    }
}
