using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class IniFileRW
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileInt(
            string lpAppName,
            string lpKeyName,
            int nDefault,
            string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern uint WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);

        private string IniFilePath;

        public IniFileRW(string iniFilePath)
        {
            IniFilePath = iniFilePath;
        }

        /// <summary>
        /// 値を取得
        /// </summary>
        /// <param name="sectionName">セクション名</param>
        /// <param name="keyName">キー</param>
        /// <returns>取得した値</returns>
        public uint GetIntValue(string sectionName, string keyName)
        {
            uint value = GetPrivateProfileInt(sectionName, keyName, 0, IniFilePath);

            return value;
        }

        /// <summary>
        /// 文字列を取得
        /// </summary>
        /// <param name="sectionName">セクション名</param>
        /// <param name="keyName">キー</param>
        /// <returns>取得した文字列</returns>
        public string GetStringValue(string sectionName, string keyName)
        {
            StringBuilder value = new StringBuilder(0x400);
            uint strSize = GetPrivateProfileString(sectionName, keyName, "", value, (uint)value.Capacity, IniFilePath);
            return value.ToString();
        }

        /// <summary>
        /// 文字列を設定
        /// </summary>
        /// <param name="sectionName">セクション名</param>
        /// <param name="keyName">キー</param>
        /// <param name="value">値</param>
        /// <returns>成否</returns>
        public bool SetValue(string sectionName, string keyName, string value)
        {
            uint res = WritePrivateProfileString(sectionName, keyName, value, IniFilePath);
            if (res != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="sectionName">セクション名</param>
        /// <param name="keyName">キー</param>
        /// <param name="value">値</param>
        /// <returns>成否</returns>
        public bool SetValue(string sectionName, string keyName, int value)
        {
            return SetValue(sectionName, keyName, value.ToString());
        }

        /// <summary>
        /// キーを削除
        /// </summary>
        /// <param name="sectionName">セクション名</param>
        /// <param name="keyName">削除するキー</param>
        /// <returns>成否</returns>
        public bool DelKey(string sectionName, string keyName)
        {
            uint res = WritePrivateProfileString(sectionName, keyName, null, IniFilePath);
            if (res != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// セクションを削除
        /// </summary>
        /// <param name="sectionName">削除するセクション名</param>
        /// <returns>成否</returns>
        public bool DelSeciton(string sectionName)
        {
            uint res = WritePrivateProfileString(sectionName, null, null, IniFilePath);
            if (res != 0)
            {
                return true;
            }
            return false;
        }
    }
}
