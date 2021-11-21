using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    class AppConfig
    {
        #region 設定キー名
        private const string KEY_RUNTIME_VS2008_x86 = "vs2008runtime_x86";
        private const string KEY_RUNTIME_VS2008_x64 = "vs2008runtime_x64";
        private const string KEY_RUNTIME_VS2010_x86 = "vs2010runtime_x86";
        private const string KEY_RUNTIME_VS2010_x64 = "vs2010runtime_x64";
        private const string KEY_RUNTIME_VS2012_x86 = "vs2012runtime_x86";
        private const string KEY_RUNTIME_VS2012_x64 = "vs2012runtime_x64";
        private const string KEY_RUNTIME_VS2013_x86 = "vs2013runtime_x86";
        private const string KEY_RUNTIME_VS2013_x64 = "vs2013runtime_x64";
        private const string KEY_RUNTIME_VS201X_x86 = "vs201Xruntime_x86";
        private const string KEY_RUNTIME_VS201X_x64 = "vs201Xruntime_x64";
        #endregion

        /// <summary>
        /// ランタイム設定値
        /// </summary>
        public class Runtime
        {
            /// <summary>
            /// VS2008ランタイム(x86)
            /// </summary>
            public static bool vs2008_x86 = false;
            /// <summary>
            /// VS2008ランタイム(x64)
            /// </summary>
            public static bool vs2008_x64 = false;
            /// <summary>
            /// VS2010ランタイム(x86)
            /// </summary>
            public static bool vs2010_x86 = false;
            /// <summary>
            /// VS2010ランタイム(x64)
            /// </summary>
            public static bool vs2010_x64 = false;
            /// <summary>
            /// VS2012ランタイム(x86)
            /// </summary>
            public static bool vs2012_x86 = false;
            /// <summary>
            /// VS2012ランタイム(x64)
            /// </summary>
            public static bool vs2012_x64 = false;
            /// <summary>
            /// VS2013ランタイム(x86)
            /// </summary>
            public static bool vs2013_x86 = false;
            /// <summary>
            /// VS2013ランタイム(x64)
            /// </summary>
            public static bool vs2013_x64 = false;
            /// <summary>
            /// VS201Xランタイム(x86)
            /// </summary>
            public static bool vs201X_x86 = false;
            /// <summary>
            /// VS201Xランタイム(x64)
            /// </summary>
            public static bool vs201X_x64 = false;
        }

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        private static readonly string configFile = $"{SysConfig.UserAppDataPath}\\{ProductInfo.AppName}.config";

        /// <summary>
        /// 設定ファイルを読み出す
        /// </summary>
        public static void Load()
        {
            KeyValueConfigurationCollection appConfig = null;
            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configFile;
                appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None).AppSettings.Settings;
            }
            catch
            { 
            }
            if (appConfig == null || appConfig.Count == 0)
            {
                return;
            }

            foreach (string key in appConfig.AllKeys)
            {
                if (key == KEY_RUNTIME_VS2008_x86)
                {
                    Runtime.vs2008_x86 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2008_x64)
                {
                    Runtime.vs2008_x64 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2010_x86)
                {
                    Runtime.vs2010_x86 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2010_x64)
                {
                    Runtime.vs2010_x64 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2012_x86)
                {
                    Runtime.vs2012_x86 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2012_x64)
                {
                    Runtime.vs2012_x64 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2013_x86)
                {
                    Runtime.vs2013_x86 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2013_x64)
                {
                    Runtime.vs2013_x64 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS201X_x86)
                {
                    Runtime.vs201X_x86 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS201X_x64)
                {
                    Runtime.vs201X_x64 = Convert.ToBoolean(appConfig[key].Value);
                }
            }
        }

        /// <summary>
        /// 設定ファイルに保存
        /// </summary>
        public static void Save()
        {
            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configFile;
                Configuration saveConfigFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                KeyValueConfigurationCollection settings = saveConfigFile.AppSettings.Settings;

                Update(settings, KEY_RUNTIME_VS2008_x86, Runtime.vs2008_x86.ToString());
                Update(settings, KEY_RUNTIME_VS2008_x64, Runtime.vs2008_x64.ToString());
                Update(settings, KEY_RUNTIME_VS2010_x86, Runtime.vs2010_x86.ToString());
                Update(settings, KEY_RUNTIME_VS2010_x64, Runtime.vs2010_x64.ToString());
                Update(settings, KEY_RUNTIME_VS2012_x86, Runtime.vs2012_x86.ToString());
                Update(settings, KEY_RUNTIME_VS2012_x64, Runtime.vs2012_x64.ToString());
                Update(settings, KEY_RUNTIME_VS2013_x86, Runtime.vs2013_x86.ToString());
                Update(settings, KEY_RUNTIME_VS2013_x64, Runtime.vs2013_x64.ToString());
                Update(settings, KEY_RUNTIME_VS201X_x86, Runtime.vs201X_x86.ToString());
                Update(settings, KEY_RUNTIME_VS201X_x64, Runtime.vs201X_x64.ToString());

                saveConfigFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(saveConfigFile.AppSettings.SectionInformation.Name);

            }
            catch
            {
            }
        }

        /// <summary>
        /// <para>keyにvalueを反映</para>
        /// <para>keyが存在しない場合はkeyを作成</para>
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void Update(KeyValueConfigurationCollection setting, string key, string value)
        {
            if (setting[key] == null)
            {
                setting.Add(key, value);
            }
            else
            {
                setting[key].Value = value;
            }
        }
    }
}
