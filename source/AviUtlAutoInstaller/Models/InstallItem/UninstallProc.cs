using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    partial class InstallItem
    {
        /// <summary>
        /// アンインストール
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Uninstall(InstallItem item)
        {
            bool success = false;
            string baseDir = string.Empty;

            switch (item.FileType)
            {
                case InstallFileType.Tool:
                    break;
                case InstallFileType.Script:
                    if (item.IsSpecialItem)
                    {
                        success = UninstallSpecialScript(item);
                    }
                    else
                    {
                        baseDir = $"{SysConfig.AviUtlScriptDir}";
                    }
                    break;
                case InstallFileType.Plugin:
                    if (item.IsSpecialItem)
                    {
                        success = UninstallSpecialPlugin(item);
                    }
                    else
                    {
                        baseDir = $"{SysConfig.AviUtlPluginDir}";
                    }
                    break;
                case InstallFileType.Encoder:
                    UninstallRigayaEncoder(item);
                    success = true;
                    break;
                case InstallFileType.Image:
                    baseDir = SysConfig.AviUtlFigureDir;
                    break;
            }

            if (baseDir != string.Empty)
            {
                // ファイルの削除
                FileOperation fileOperation = new FileOperation();
                List<string> uninstallFiles = new List<string>();
                foreach (string file in item.InstallFileList)
                {
                    var f = file.Split('\\');
                    uninstallFiles.Add(f[f.Length - 1]);
                }
                List<string> uninstallFileList = fileOperation.GenerateFilePathList(baseDir, uninstallFiles.ToArray());
                foreach (var file in uninstallFileList)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        success = false;
                    }
                    success = true;
                }

                if ((InstallFileType.Script == item.FileType) && success)
                {
                    // 空ディレクトリの削除
                    string scriptNameDir = $"{baseDir}\\{item.ScriptDirName}";
                    if (fileOperation.IsDirectoryEmpty(scriptNameDir))
                    {
                        Directory.Delete(scriptNameDir, true);
                    }
                }
            }

            item.IsInstalled = !success;
            return success;
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
                case SpecialPluginType.CameraAssist:
                    UninstallCameraAssist(item);
                    break;
                case SpecialPluginType.Redo:
                    UninstallRedo();
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

        /// <summary>
        /// カメラ操作補助のアンインストール
        /// </summary>
        private static void UninstallCameraAssist(InstallItem item)
        {
            string[] uninstallItem = new string[] {
                "CameraAssist.auf",
                "上方向ベクトル.cam",
                "カメラ制御(視点).exa",
                "カメラ制御(視点移動).exa"
            };

            FileOperation fileOperation = new FileOperation();
            var fileList = fileOperation.GenerateFilePathList($"{SysConfig.AviUtlPluginDir}", uninstallItem);

            foreach(var filePath in fileList)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            string cameraCtrlDirPath = $"{SysConfig.AviUtlPluginDir}\\カメラ制御";
            if (fileOperation.IsDirectoryEmpty(cameraCtrlDirPath))
            {
                Directory.Delete(cameraCtrlDirPath, false);
            }

            string makerDirPath = $"{SysConfig.AviUtlScriptDir}\\{item.MakerName}";
            if (fileOperation.IsDirectoryEmpty(makerDirPath))
            {
                Directory.Delete(makerDirPath, false);
            }
        }

        /// <summary>
        /// やり直し機能追加のアンインストール
        /// </summary>
        private static void UninstallRedo()
        {
            string[] uninstallItem = new string[] {
                "Redo.auf"
            };

            FileOperation fileOperation = new FileOperation();
            var fileList = fileOperation.GenerateFilePathList($"{SysConfig.AviUtlPluginDir}", uninstallItem);

            foreach (var filePath in fileList)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            string undoTmpDirPath = $"{SysConfig.InstallRootPath}\\UndoTmp";
            if (Directory.Exists(undoTmpDirPath))
            {
                Directory.Delete(undoTmpDirPath, true);
            }
        }

        /// <summary>
        /// 単純にアンインストール出来ないスクリプトのアンインストール
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool UninstallSpecialScript(InstallItem item)
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
                    UninstallEqualizeHist();
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 自動明暗補正のアンインストール
        /// </summary>
        /// <param name="downloadFileName"></param>
        private static void UninstallEqualizeHist()
        {
            string[] filePath = new string[] {
                $"{SysConfig.InstallRootPath}\\opencv_world452.dll",
                $"{SysConfig.AviUtlScriptDir}\\ちはユキ氏\\equalizeHist.dll",
                $"{SysConfig.AviUtlScriptDir}\\ちはユキ氏\\明暗補正.anm"
            };

            foreach (string path in filePath)
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// rigaya氏エンコーダーアンインストール
        /// </summary>
        /// <param name="item"></param>
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


    }
}
