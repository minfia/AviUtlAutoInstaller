using Nett;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class InstallProfileRW : TomlFileRW
    {
        private const string mainKeyName = "select";
        private const string fileName = "InstallationList";
        private const string fileExtension = "profile";
        private static List<TomlTable> itemList = new List<TomlTable>();

        public static string ReloadFileName { get; private set; }

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
                if (!File.Exists(filePath))
                {
                    return;
                }
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
            ReloadFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}";
            Write(table, $"{filePath}\\{ReloadFileName}");
        }

        /// <summary>
        /// インストール情報を追加
        /// </summary>
        /// <param name="data"></param>
        public static void AddContents(InstallItem item)
        {
            var tomlItem = Toml.Create();

            tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileName], item.DownloadFileName);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.RefType], (int)InstallItemList.RepoType.Pre);
            try
            {
                itemList.Add(tomlItem);
            }
            catch
            {
            }
        }

        /// <summary>
        /// インストール情報を削除
        /// </summary>
        /// <param name="item"></param>
        public static void DeleteContents(InstallItem item)
        {
            var tomlItem = Toml.Create();

            tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileName], item.DownloadFileName);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
            tomlItem.Add(_rwKeyTypeDic[RWKeyType.RefType], (int)InstallItemList.RepoType.Pre);
            int index = itemList.FindIndex(x => x[_rwKeyTypeDic[RWKeyType.Name]].ToString() == item.Name);
            if (0 <= index)
            {
                itemList.RemoveAt(index);
            }
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

            itemList = data.Get<TomlTableArray>(mainKeyName).Items;
            InstallItemList.ClearAllIsSelect();

            switch (version)
            {
                case FileVersion.V100:
                    SetEnableListV100();
                    break;
                default:
                    return;
            }
        }

        protected override TomlTable ConvertToTomlTable()
        {
            var toml = Toml.Create();

            toml.Add("version", _fileVersionDic.Last().Value);

            InstallItemList installItemList = new InstallItemList();

            for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
            {
                foreach (InstallItem item in installItemList.GetInstalItemList(i))
                {
                    if (!item.IsInstalled || !item.IsInstallCompleted)
                    {
                        continue;
                    }

                    var tomlItem = Toml.Create();

                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileName], item.DownloadFileName);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
                    tomlItem.Add(_rwKeyTypeDic[RWKeyType.RefType], (int)i);

                    itemList.Add(tomlItem);
                }
            }

            itemList = itemList.Distinct(new TomlTableComparer()).ToList();
            TomlTableArray tomlTableArray = new TomlTableArray(new Root(), itemList);
            toml.Add(mainKeyName, tomlTableArray);

            return toml;
        }

        /// <summary>
        /// v1.0.0用読み出し
        /// </summary>
        /// <param name="array"></param>
        private void SetEnableListV100()
        {
            TomlTableArray array = new TomlTableArray(new Root(), itemList);
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
                InstallItemList.SetIsInstalled(item.RefRepoType, item.Name, true);
                if (InstallItemList.CheckDuplicateName(InstallItemList.RepoType.Pre, item.Name) && (InstallItemList.RepoType.User == item.RefRepoType))
                {
                    // ユーザーリポジトリを優先
                    InstallItemList.SetIsInstalled(InstallItemList.RepoType.Pre, item.Name, false);
                }
            }
        }

        private class Root : ITomlRoot
        {
            public TomlSettings Settings => TomlSettings.Create();
        }

        private class TomlTableComparer : IEqualityComparer<TomlTable>
        {
            public bool Equals(TomlTable x, TomlTable y)
            {
                if (x[_rwKeyTypeDic[RWKeyType.Name]].ToString() == y[_rwKeyTypeDic[RWKeyType.Name]].ToString() &&
                    x[_rwKeyTypeDic[RWKeyType.FileName]].ToString() == y[_rwKeyTypeDic[RWKeyType.FileName]].ToString() &&
                    x[_rwKeyTypeDic[RWKeyType.Version]].ToString() == y[_rwKeyTypeDic[RWKeyType.Version]].ToString())
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(TomlTable obj)
            {
                return 0;
            }
        }
    }
}
