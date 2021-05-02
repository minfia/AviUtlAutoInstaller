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
        /// インストール
        /// </summary>
        /// <param name="installFileList"></param>
        /// <returns></returns>
        public bool Install(string[] installFileList)
        {
            if (InstallFileType.Encoder == FileType)
            {
                // エンコーダ関係のインストール処理
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

        /// <summary>
        /// 単純にインストール出来ないプラグインの判定
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private bool IsSpecialPlugin(string name)
        {
            return false;
        }

        /// <summary>
        /// 単純にインストール出来ないプラグインのインストール
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool InstallSpecialPlugin(string name)
        {
            return true;
        }
    }
}
