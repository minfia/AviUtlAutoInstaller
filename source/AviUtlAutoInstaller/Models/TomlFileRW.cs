using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    abstract class TomlFileRW
    {
        /// <summary>
        /// tomlファイルの読み出し
        /// </summary>
        /// <param name="filePath">読み込み先</param>
        /// <returns></returns>
        protected TomlTable Read(string filePath)
        {
            TomlTable data = Toml.Create();
            try
            {
                data = Toml.ReadFile(filePath);
            }
            catch
            {
                throw;
            }

            return data;
        }

        /// <summary>
        /// tomlファイルへ書き込み
        /// </summary>
        /// <param name="data">書き込むTomlTableデータ</param>
        /// <param name="filePath">書き込み先</param>
        protected void Write(TomlTable data, string filePath)
        {
            try
            {
                Toml.WriteFile(data, filePath);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// TomlTableからデータに変換
        /// </summary>
        /// <param name="data"></param>
        protected abstract void ConvertToData(TomlTable data);

        /// <summary>
        /// データからTomlTableに変換
        /// </summary>
        /// <returns></returns>
        protected abstract TomlTable ConvertToTomlTable();
    }
}
