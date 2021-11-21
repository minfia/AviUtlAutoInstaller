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
            bool success = false;
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
                    {
                        string scriptDir = $"{SysConfig.AviUtlScriptDir}\\{item.ScriptDirName}";
                        if (!Directory.Exists(scriptDir))
                        {
                            Directory.CreateDirectory(scriptDir);
                        }
                        if (item.IsSpecialItem)
                        {
                            success = InstallSpecialScript(item);
                        }
                        else
                        {
                            installDir = scriptDir;
                        }
                    }
                    break;
                case InstallFileType.Plugin:
                    if (item.IsSpecialItem)
                    {
                        success = InstallSpecialPlugin(item);
                    }
                    else
                    {
                        installDir = SysConfig.AviUtlPluginDir;
                    }
                    break;
                case InstallFileType.Encoder:
                    InstallRigayaEncoder(item.DownloadFileName);
                    success = true;
                    break;
                case InstallFileType.Image:
                    installDir = SysConfig.AviUtlFigureDir;
                    break;
            }

            if (installDir != string.Empty)
            {
                FileOperation fileOperation = new FileOperation();
                fileOperation.FileMove(installFileList, installDir);
                success = true;
            }

            item.IsInstalled = success;
            return success;
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
            { "vc2010redist_x86.exe", ExternalFileType.VSRuntime },
            { "vc2010redist_x64.exe", ExternalFileType.VSRuntime },
            { "vc2012redist_x86.exe", ExternalFileType.VSRuntime },
            { "vc2012redist_x64.exe", ExternalFileType.VSRuntime },
            { "vc2013redist_x86.exe", ExternalFileType.VSRuntime },
            { "vc2013redist_x64.exe", ExternalFileType.VSRuntime },
            { "vc201Xredist_x86.exe", ExternalFileType.VSRuntime },
            { "vc201Xredist_x64.exe", ExternalFileType.VSRuntime },
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
                                if ((args = GetVSRuntimeArgument(exFile)) == string.Empty)
                                {
                                    break;
                                }
                                if (VSRuntimeInstall(exFilePath, args))
                                {
                                    RegistVSRuntime(exFile);
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
        /// VisualStudioのランタイムインストール用の引数を取得
        /// </summary>
        /// <param name="exFile">インストーラ名</param>
        /// <returns>arg or string.Empty</returns>
        private static string GetVSRuntimeArgument(string exFile)
        {
            string args = string.Empty;

            if (exFile.Contains("vc2008") &&
                (exFile.Contains("_x86") && !AppConfig.Runtime.vs2008_x86) ||
                (exFile.Contains("_x64") && !AppConfig.Runtime.vs2008_x64))
            {
                args = "/q";
            }
            else if (exFile.Contains("vc2010") &&
                    (exFile.Contains("_x86") && !AppConfig.Runtime.vs2010_x86) ||
                    (exFile.Contains("_x64") && !AppConfig.Runtime.vs2010_x64))
            {
                args = "/q";
            }
            else if (exFile.Contains("vc2012") &&
                     (exFile.Contains("_x86") && !AppConfig.Runtime.vs2012_x86) ||
                     (exFile.Contains("_x64") && !AppConfig.Runtime.vs2012_x64))
            {
                args = "quiet";
            }
            else if (exFile.Contains("vc2013") &&
                     (exFile.Contains("_x86") && !AppConfig.Runtime.vs2013_x86) ||
                     (exFile.Contains("_x64") && !AppConfig.Runtime.vs2013_x64))
            {
                args = "quiet";
            }
            else if (exFile.Contains("vc201X") &&
                     (exFile.Contains("_x86") && !AppConfig.Runtime.vs201X_x86) ||
                     (exFile.Contains("_x64") && !AppConfig.Runtime.vs201X_x64))
            {
                args = "quiet";
            }

            return args;
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

        /// <summary>
        /// VisualStudioのランタイムの登録
        /// </summary>
        /// <param name="exFile">インストーラ名</param>
        private static void RegistVSRuntime(string exFile)
        {
            if (exFile.Contains("vc2008"))
            {
                if (exFile.Contains("_x86"))
                {
                    AppConfig.Runtime.vs2008_x86 = true;
                }
                else if (exFile.Contains("_x64"))
                {
                    AppConfig.Runtime.vs2008_x86 = true;
                }
            }
            else if (exFile.Contains("vc2010"))
            {
                if (exFile.Contains("_x86"))
                {
                    AppConfig.Runtime.vs2010_x86 = true;
                }
                else if (exFile.Contains("_x64"))
                {
                    AppConfig.Runtime.vs2010_x86 = true;
                }
            }
            else if (exFile.Contains("vc2012"))
            {
                if (exFile.Contains("_x86"))
                {
                    AppConfig.Runtime.vs2012_x86 = true;
                }
                else if (exFile.Contains("_x64"))
                {
                    AppConfig.Runtime.vs2012_x86 = true;
                }
            }
            else if (exFile.Contains("vc2013"))
            {
                if (exFile.Contains("_x86"))
                {
                    AppConfig.Runtime.vs2013_x86 = true;
                }
                else if (exFile.Contains("_x64"))
                {
                    AppConfig.Runtime.vs2013_x64 = true;
                }
            }
            else if (exFile.Contains("vc201X"))
            {
                if (exFile.Contains("_x86"))
                {
                    AppConfig.Runtime.vs201X_x86 = true;
                }
                else if (exFile.Contains("_x64"))
                {
                    AppConfig.Runtime.vs201X_x64 = true;
                }
            }
            AppConfig.Save();
        }

        private enum SpecialPluginType
        {
            PSDToolkit,
            ExToolBar,
        };

        private static readonly Dictionary<SpecialPluginType, string> _specialPluginDic = new Dictionary<SpecialPluginType, string>()
        {
            { SpecialPluginType.PSDToolkit, "PSDToolKit" },
            { SpecialPluginType.ExToolBar, "拡張ツールバー" },
        };


        /// <summary>
        /// 単純にインストール出来ないプラグインのインストール
        /// </summary>
        /// <param name="item">インストールアイテム</param>
        /// <returns></returns>
        private static bool InstallSpecialPlugin(InstallItem item)
        {
            SpecialPluginType type;
            try
            {
                type = _specialPluginDic.First(x => x.Value == item.Name).Key;
            }
            catch
            {
                return false;
            }

            switch (type)
            {
                case SpecialPluginType.PSDToolkit:
                    InstallPSDToolKit(item.DownloadFileName);
                    break;
                case SpecialPluginType.ExToolBar:
                    InstallExToolBar(item.DownloadFileName);
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
            fileOperation.DirectoryMove(srcDocsPath, $"{psdManualDestPath}\\PSDToolKitDocs", null);
            Directory.Delete(srcDocsPath, true);
            string[] fileNames = { "GCMZDrops.txt", "PSDToolKit.txt", "PSDToolKit説明書.html", "ZRamPreview.txt", "キャッシュテキスト.txt" };
            List<string> srcFileNamePath = new List<string>();
            foreach (string fileName in fileNames)
            {
                srcFileNamePath.Add($"{psdSrcPath}\\{fileName}");
            }
            fileOperation.FileMove(srcFileNamePath.ToArray(), psdManualDestPath);

            // 本体の移動
            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}", $"{SysConfig.AviUtlPluginDir}", null);
        }

        /// <summary>
        /// 拡張ツールバーのインストール
        /// </summary>
        /// <param name="downloadFileName"></param>
        private static void InstallExToolBar(string downloadFileName)
        {
            FileOperation fileOperation = new FileOperation();
            string exToolSrcPath = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}";

            string iconFileDestPath = $"{SysConfig.AviUtlPluginDir}";
            Directory.CreateDirectory($"{iconFileDestPath}\\extoolbar");

            File.Delete($"{exToolSrcPath}\\extoolbar.txt");

            string exToolIconSrcPath = $"{exToolSrcPath}";
            fileOperation.DirectoryMove(exToolIconSrcPath, iconFileDestPath, null);
        }

        private enum SpecialScriptType
        {
            EqualizeHist,
        };

        private static readonly Dictionary<SpecialScriptType, string> _specialScriptDic = new Dictionary<SpecialScriptType, string>()
        {
            { SpecialScriptType.EqualizeHist, "自動明暗補正" },
        };

        /// <summary>
        /// 単純にインストールできないプラグインのインストール
        /// </summary>
        /// <param name="item">インストールアイテム</param>
        /// <returns></returns>
        private static bool InstallSpecialScript(InstallItem item)
        {
            SpecialScriptType type;
            try
            {
                type = _specialScriptDic.First(x => x.Value == item.Name).Key;
            }
            catch
            {
                return false;
            }

            switch (type)
            {
                case SpecialScriptType.EqualizeHist:
                    InstallEqualizeHist(item.DownloadFileName);
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 自動明暗補正のインストール
        /// </summary>
        /// <param name="downloadFileName"></param>
        private static void InstallEqualizeHist(string downloadFileName)
        {
            FileOperation fileOperation = new FileOperation();

            string extractSrcPath = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}\\{Path.GetFileNameWithoutExtension(downloadFileName)}";

            fileOperation.FileMove(new string[] { $"{extractSrcPath}\\opencv_world452.dll" }, SysConfig.InstallRootPath);

            var list = fileOperation.GenerateFilePathList($"{extractSrcPath}\\明暗補正", new string[] { "equalizeHist.dll", "明暗補正.anm" });

            fileOperation.FileMove(list.ToArray(), $"{SysConfig.AviUtlScriptDir}\\ちはユキ氏");
        }

        /// <summary>
        /// rigaya氏エンコーダーインストール
        /// </summary>
        /// <param name="downloadFileName"></param>
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

            fileOperation.DirectoryMove($"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(downloadFileName)}", tempDir, null);

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
