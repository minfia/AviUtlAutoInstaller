using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    class FileOperation
    {
        /// <summary>
        /// 指定パスに書き込み権限があるかチェック
        /// </summary>
        /// <param name="dirPath">チェック対象</param>
        /// <returns>true(書き込み可能) or false(書き込み不可)</returns>
        public static bool IsWritableOfDirectory(string dirPath)
        {
            {
                string[] exceptionDirecrotyPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                };

                if (dirPath.Equals("C:\\") || dirPath.Equals("C:"))
                {
                    return true;
                }

                foreach (string exDirPath in exceptionDirecrotyPaths)
                {
                    if (dirPath.Contains(exDirPath))
                    {
                        return false;
                    }
                }
            }

            bool writable = false;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal myPrincipal = (WindowsPrincipal)Thread.CurrentPrincipal;
            DirectorySecurity dirsecurity = Directory.GetAccessControl(dirPath);
            foreach (FileSystemAccessRule rule in dirsecurity.GetAccessRules(true, true, typeof(NTAccount)))
            {
                if (myPrincipal.IsInRole(rule.IdentityReference.Value))
                {
                    if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                    {
                        writable = true;
                    }
                }
            }
            return writable;
        }

        /// <summary>
        /// 圧縮ファイルを解凍
        /// </summary>
        /// <param name="srcFilePath">解凍するファイルのパス</param>
        /// <param name="destPath">解凍先のパス</param>
        public void Extract(string srcFilePath, string destPath)
        {
            using (ArchiveFile file = new ArchiveFile(srcFilePath))
            {
                file.Extract(destPath, true);
            }
        }

        /// <summary>
        /// ファイルパス一覧を指定ディレクトリに移動
        /// </summary>
        /// <param name="filePathList">移動するファイルパス一覧</param>
        /// <param name="destDirPath">移動先のパス</param>
        /// <exception cref="PathTooLongException"></exception>
        public void FileMove(string[] filePathList, string destDirPath)
        {
            foreach (string filePath in filePathList)
            {
                string fileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                try
                {
                    string destFile = $"{destDirPath}\\{fileName}";
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(filePath, destFile);
                }
                catch (PathTooLongException)
                {
                    throw;
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// ディレクトリを移動
        /// </summary>
        /// <param name="srcDirPath">移動元のディレクトパス</param>
        /// <param name="destDirPath">移動先のディレクトリパス</param>
        public void DirectoryMove(string srcDirPath, string destDirPath)
        {
            string[] srcDirHierarchy = Directory.GetDirectories(srcDirPath, "*", SearchOption.AllDirectories);
            string srcDirLastName = srcDirPath.Split('\\').Last();

            // 出力先のディレクトリ作成
            foreach (string dir in srcDirHierarchy)
            {
                int startIndex = dir.IndexOf(srcDirLastName) + "\\".Length;
                var d = dir.Substring(startIndex + srcDirLastName.Length);
                Directory.CreateDirectory($"{destDirPath}\\{d}");
            }

            var fileList = GenerateFilePathList(srcDirPath, "*.*");

            // 出力先のファイル一覧を生成と移動
            foreach (string file in fileList)
            {
                int startIndex = file.IndexOf(srcDirLastName) + "\\".Length;
                var f = file.Substring(startIndex + srcDirLastName.Length);

                File.Move(file, $"{destDirPath}\\{f}");
            }
        }

        /// <summary>
        /// ファイル一覧の生成
        /// </summary>
        /// <param name="srcDirPath">取得元のディレクトリパス</param>
        /// <param name="files">取得するファイルまたは拡張子</param>
        /// <returns>ファイル一覧</returns>
        public List<string> GenerateFilePathList(string srcDirPath, string[] files)
        {
            List<string> filePathList = new List<string>();

            foreach (string file in files)
            {
                string[] pathList = Directory.GetFiles(srcDirPath, file, SearchOption.AllDirectories);
                foreach (string path in pathList)
                {
                    filePathList.Add(path);
                }
            }

            return filePathList;
        }

        /// <summary>
        /// ファイル一覧の生成
        /// </summary>
        /// <param name="srcDirPath">取得元のディレクトリパス</param>
        /// <param name="file">特定のファイル</param>
        /// <returns>ファイル一覧</returns>
        private List<string> GenerateFilePathList(string srcDirPath, string file)
        {
            string[] files = { file };

            return GenerateFilePathList(srcDirPath, files);
        }


        /// <summary>
        /// アプリの実行タイプ
        /// </summary>
        public enum ExecAppType
        {
            /// <summary>
            /// CUIアプリ
            /// </summary>
            CUI,
            /// <summary>
            /// GUIアプリ
            /// </summary>
            GUI
        };

        /// <summary>
        /// アプリを実行
        /// </summary>
        /// <param name="appPath">実行するアプリのパス</param>
        /// <param name="argument">アプリの実行引数</param>
        /// <param name="appType">アプリの実行タイプ</param>
        /// <param name="proc">アプリ実行のProcess</param>
        /// <returns>成否</returns>
        public bool ExecApp(string appPath, string argument, ExecAppType appType, out Process proc)
        {
            proc = new Process();
            proc.StartInfo.FileName = appPath;
            proc.StartInfo.Arguments = argument;
            proc.StartInfo.UseShellExecute = appType == ExecAppType.CUI ? true : false;
            bool ret = proc.Start();

            return ret;
        }

        /// <summary>
        /// アプリを実行
        /// </summary>
        /// <param name="appPath">実行するアプリのパス</param>
        /// <param name="appType">アプリの実行タイプ</param>
        /// <param name="proc">アプリ実行のProcess</param>
        /// <returns>成否</returns>
        public bool ExecApp(string appPath, ExecAppType appType, out Process proc)
        {
            return ExecApp(appPath, "", appType, out proc);
        }

        /// <summary>
        /// 実行したアプリを停止させる
        /// </summary>
        /// <param name="proc">停止させたいプロセス</param>
        /// <returns>成否</returns>
        public bool KillApp(Process proc)
        {
            if (proc.HasExited)
            {
                return true;
            }
            if (proc.StartInfo.UseShellExecute)
            {
                // CUI
                proc.Kill();
                return true;
            }
            else
            {
                // GUI
                try
                {
                    if (!proc.CloseMainWindow())
                    {
                        return false;
                    }
                    proc.WaitForExit(10000);
                    return proc.HasExited;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// ショートカット作成
        /// </summary>
        /// <param name="destPath">ショートカット作成元のファイルパス</param>
        /// <param name="shortcutName">ショートカット名</param>
        /// <returns></returns>
        public bool MakeShortcut(string destPath, string shortcutName)
        {
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), @$"{shortcutName}.lnk");
            string targetPath = destPath;

            string WScriptShellCLSID = "72C24DD5-D70A-438B-8A42-98424B88AFB8";
            Type t = Type.GetTypeFromCLSID(new Guid(WScriptShellCLSID));
            dynamic shell = Activator.CreateInstance(t);

            var shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.IconLocation = targetPath + ",0";

            shortcut.Save();

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);

            return true;
        }
    }
}
