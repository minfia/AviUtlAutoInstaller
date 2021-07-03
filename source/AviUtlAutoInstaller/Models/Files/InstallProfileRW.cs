using Nett;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class InstallProfileRW : TomlFileRW
    {
        private const string MainKeyName = "select";

        private class InstallationItem
        {
            public string Name;
            public string FileName;
            public string Version;
            public InstallItemList.RepoType RefRepoType;
        }

        private enum FileVersion
        {
            None,
            V100,
        }

        /// <summary>
        /// InstallProfileの読み書き対象
        /// </summary>
        private enum RWKeyType
        {
            Name,
            FileName,
            Version,
            RefType,
        }

        private static Dictionary<FileVersion, string> _fileVersionDic = new Dictionary<FileVersion, string>()
        {
            { FileVersion.V100, "v1.0.0" },
        };

        /// <summary>
        /// RWKeyTypeとファイルのKeyの文字列との紐付け
        /// </summary>
        private static Dictionary<RWKeyType, string> _rwKeyTypeDic = new Dictionary<RWKeyType, string>()
        {
            { RWKeyType.Name, "name" },
            { RWKeyType.FileName, "filename" },
            { RWKeyType.Version, "version" },
            { RWKeyType.RefType, "reftype" },
        };

        /// <summary>
        /// インストールプロファイルを読み出す
        /// </summary>
        /// <param name="filePath"></param>
        public void FileRead(string filePath)
        {
            try
            {
                TomlTable toml = Read(filePath);
                ConvertToData(toml);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// インストールプロファイルをファイルに保存
        /// </summary>
        /// <param name="filePath"></param>
        public void FileWrite(string filePath)
        {
            TomlTable table = ConvertToTomlTable();
            Write(table, filePath);
        }

        protected override void ConvertToData(TomlTable data)
        {
            string tomlFileVersion = data.Get<string>("version");

            FileVersion version = FileVersion.None;
            try
            {
                version = _fileVersionDic.First(x => x.Value == tomlFileVersion).Key;
            }
            catch
            {
                throw new KeyNotFoundException($"読み込んだファイルのバージョンが不正です({tomlFileVersion})");
            }

            TomlTableArray array = data.Get<TomlTableArray>(MainKeyName);

            switch (version)
            {
                case FileVersion.V100:
                    SetEnableListV100(array);
                    break;
                default:
                    return;
            }
        }

        protected override TomlTable ConvertToTomlTable()
        {
            var toml = Toml.Create();

            toml.Add("version", _fileVersionDic.Last().Value);

            List<TomlTable> table = new List<TomlTable>();
            InstallItemList installItemList = new InstallItemList();

            for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
            {
                foreach (InstallItem item in installItemList.GetInstalItemList(i))
                {
                    if (!item.IsSelect || !item.IsInstallCompleted)
                    {
                        continue;
                    }

                    var tomlItem = Toml.Create();

                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileName], item.DownloadFileName);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.RefType], (int)i);

                    table.Add(tomlItem);
                }
            }

            TomlTableArray array = new TomlTableArray(new Root(), table);
            toml.Add(MainKeyName, array);

            return toml;
        }

        /// <summary>
        /// v1.0.0用読み出し
        /// </summary>
        /// <param name="array"></param>
        private void SetEnableListV100(TomlTableArray array)
        {
            List<InstallationItem> list = new List<InstallationItem>();
            for (int i = 0; i < array.Count; i++)
            {
                InstallationItem item = new InstallationItem
                {
                    Name = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Name]),
                    FileName = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.FileName]),
                    Version = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Version]),
                    RefRepoType = (InstallItemList.RepoType)array[i].Get<int>(_rwKeyTypeDic[RWKeyType.RefType]),
                };

                list.Add(item);
            }

            foreach (InstallationItem item in list)
            {
                if (!InstallItemList.CheckDuplicateName(item.RefRepoType, item.Name))
                {
                    continue;
                }
                InstallItemList.SetIsSelect(item.RefRepoType, item.Name, true);
                if (InstallItemList.CheckDuplicateName(InstallItemList.RepoType.Pre, item.Name) && (InstallItemList.RepoType.User == item.RefRepoType))
                {
                    // ユーザーリポジトリを優先
                    InstallItemList.SetIsSelect(InstallItemList.RepoType.Pre, item.Name, false);
                }

            }
        }

        private class Root : ITomlRoot
        {
            public TomlSettings Settings => TomlSettings.Create();
        }
    }
}
