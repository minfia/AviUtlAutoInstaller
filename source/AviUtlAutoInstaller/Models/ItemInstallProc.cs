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
                        if (item.IsSpecialItem)
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

            if (InstallFileType.Encoder == item.FileType)
            {
                UninstallRigayaEncoder(item);

                return true;
            }
            else
            {
                switch (item.FileType)
                {
                    case InstallFileType.Tool:
                        baseDir = $"{SysConfig.InstallRootPath}";
                        break;
                    case InstallFileType.Plugin:
                        if (item.IsSpecialItem)
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
            fileOperation.DirectoryMove(exToolIconSrcPath, iconFileDestPath);
        }

        /// <summary>
        /// 単純にインストール出来ないプラグインのアンインストール
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool UninstallSpecialPlugin(InstallItem item)
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
                    UninstallPSDToolKit();
                    break;
                case SpecialPluginType.ExToolBar:
                    UninstallExToolBar();
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

        /// <summary>
        /// 拡張ツールバーのアンインストール
        /// </summary>
        private static void UninstallExToolBar()
        {
            string exToolBarDirPath = $"{SysConfig.AviUtlPluginDir}\\extoolbar";
            Directory.Delete(exToolBarDirPath, true);

            string[] filePath = new string[] {
                $"{SysConfig.AviUtlPluginDir}\\extoolbar.auf",
                $"{SysConfig.AviUtlPluginDir}\\extoolbar.ini",
                $"{SysConfig.AviUtlPluginDir}\\extoolkey.tsv"
            };

            foreach (string path in filePath)
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

        private static void UninstallRigayaEncoder(InstallItem item)
        {
            string delEncName;

            if (item.Name.Equals("x264guiEx"))
            {
                // 削除対象
                // \exe_files\boxdumper.exe
                // \exe_files\ffmpeg_audenc.exe
                // \exe_files\muxer.exe
                // \exe_files\remuxer.exe
                // \exe_files\timelineeditor.exe
                // \exe_files\x264_*.exe
                // \plugins\x264guiEx_stg
                // \plugins\x264guiEx.auo
                // \plugins\x264guiEx.conf
                // \plugins\x264guiEx.ini

                delEncName = "x264guiEx";
            }
            else if (item.Name.Equals("QSVEnc"))
            {
                // 削除対象
                // \exe_files\QSVEncC
                // \plugins\QSVEnc_stg
                // \plugins\QSVEnc.auo
                // \plugins\QSVEnc.conf
                // \plugins\QSVEnc.ini

                delEncName = "QSVEnc";
            }
            else if (item.Name.Equals("NVEnc"))
            {
                // 削除対象
                // \exe_files\NVEncC
                // \plugins\NVEnc_stg
                // \plugins\NVEnc.auo
                // \plugins\NVEnc.conf
                // \plugins\NVEnc.ini

                delEncName = "NVEnc";
            }
            else if (item.Name.Equals("VCEEnc"))
            {
                // 削除対象
                // \exe_files\VCEEncC
                // \plugins\VCEEnc_stg
                // \plugins\VCEEnc.auo
                // \plugins\VCEEnc.conf
                // \plugins\VCEEnc.ini

                delEncName = "VCEEnc";
            }
            else
            {
                return;
            }

            FileOperation fileOperation = new FileOperation();
            string exe_filesDir = $"{SysConfig.InstallRootPath}\\exe_files";

            {
                // exe_files配下のファイル/ディレクトリ削除
                if (item.Name.Equals("x264guiEx"))
                {
                    string[] files = { "boxdumper.exe", "ffmpeg_audenc.exe", "muxer.exe", "remuxer.exe", "timelineeditor.exe", "x264_*.exe" };
                    var delFiles = fileOperation.GenerateFilePathList(exe_filesDir, files).ToArray();

                    foreach (string delFile in delFiles)
                    {
                        File.Delete(delFile);
                    }
                }
                else
                {
                    Directory.Delete($"{exe_filesDir}\\{delEncName}C", true);
                }
            }

            {
                // plugins配下のファイル/ディレクトリ削除
                Directory.Delete($"{SysConfig.AviUtlPluginDir}\\{delEncName}_stg", true);
                string[] files = { $"{delEncName}.*" };
                var delFiles = fileOperation.GenerateFilePathList(SysConfig.AviUtlPluginDir, files).ToArray();

                foreach (string delFile in delFiles)
                {
                    File.Delete(delFile);
                }
            }

            if (fileOperation.IsDirectoryEmpty(exe_filesDir))
            {
                Directory.Delete(exe_filesDir);
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
