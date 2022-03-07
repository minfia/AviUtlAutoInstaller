using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    class ProductInfo
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();

        private static readonly string _appName = assembly.GetName().Name;
        /// <summary>
        /// アプリケーション名
        /// </summary>
        public static string AppName
        {
            get { return _appName; }
        }

        private static readonly string _appVersion = assembly.GetName().Version.ToString();
        /// <summary>
        /// アプリバージョン
        /// </summary>
        public static string AppVersion
        {
            get { return _appVersion; }
        }

        private static readonly string _validAppVersion = _appVersion.Substring(0, _appVersion.LastIndexOf('.'));
        public static string ValidAppVersion
        {
            get { return _validAppVersion; }
        }

        private static readonly string _description = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;
        /// <summary>
        /// アプリの説明
        /// </summary>
        public static string Desctiption
        {
            get { return _description; }
        }

        private static readonly string _company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute))).Company;
        /// <summary>
        /// 会社名
        /// </summary>
        public static string Company
        {
            get { return _company; }
        }

        private static readonly string _product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
        /// <summary>
        /// プロダクト名
        /// </summary>
        public static string Product
        {
            get { return _product; }
        }

        private static readonly string _copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute))).Copyright;
        /// <summary>
        /// コピーライト
        /// </summary>
        public static string Copyright
        {
            get { return _copyright; }
        }

        private static string _repoVersion = "0.0";
        public static string RepoVersion
        {
            get { return _repoVersion; }
        }

        private static uint _appMatchVersion = 3;
        /// <summary>
        /// リポジトリが対応しているバージョン
        /// </summary>
        public static uint AppMatchVersion
        { 
            get { return _appMatchVersion; }
        }

        private const uint _supportRepoVersion = 3;
        /// <summary>
        /// アプリが対応するリポジトリバージョン
        /// </summary>
        public static uint SupportRepoVersion
        {
            get { return _supportRepoVersion; }
        }

        /// <summary>
        /// リポジトリのバージョンをセットする
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="maintenance"></param>
        public void SetRepoVersion(uint major, uint minor, uint maintenance, uint app_match)
        {
            _repoVersion = $"{major}.{minor}.{maintenance}";
            _appMatchVersion = app_match;
        }

        /// <summary>
        /// アプリとリポジトリの整合性チェック
        /// </summary>
        /// <param name="app_match"></param>
        /// <returns></returns>
        public bool IsSupportRepoVersion(uint app_match)
        {
            return _supportRepoVersion == app_match;
        }
    }
}
