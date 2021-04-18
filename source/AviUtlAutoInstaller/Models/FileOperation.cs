using System;
using System.Collections.Generic;
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
    }
}
