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
        private const string KEY_RUNTIME_VS2008 = "vs2008runtime";
        private const string KEY_RUNTIME_VS2013 = "vs2013runtime";
        private const string KEY_RUNTIME_VS201X = "vs201Xruntime";
        #endregion

        /// <summary>
        /// ランタイム設定値
        /// </summary>
        public class Runtime
        {
            /// <summary>
            /// VS2008ランタイム
            /// </summary>
            public static bool vs2008 = false;
            /// <summary>
            /// VS2013ランタイム
            /// </summary>
            public static bool vs2013 = false;
            /// <summary>
            /// VS201Xランタイム
            /// </summary>
            public static bool vs201X = false;
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
                if (key == KEY_RUNTIME_VS2008)
                {
                    Runtime.vs2008 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS2013)
                {
                    Runtime.vs2013 = Convert.ToBoolean(appConfig[key].Value);
                }
                if (key == KEY_RUNTIME_VS201X)
                {
                    Runtime.vs201X = Convert.ToBoolean(appConfig[key].Value);
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

                Update(settings, KEY_RUNTIME_VS2008, Runtime.vs2008.ToString());
                Update(settings, KEY_RUNTIME_VS2013, Runtime.vs2013.ToString());
                Update(settings, KEY_RUNTIME_VS201X, Runtime.vs201X.ToString());

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
