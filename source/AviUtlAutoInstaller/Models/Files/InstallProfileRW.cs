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
        private const string _mainKeyName = "select";
        private const string _fileName = "InstallationList";
        private const string _fileExtension = "profile";

        /// <summary>
        /// ファイルを読み出すときの種類
        /// </summary>
        public enum ReadType
        {
            /// <summary>
            /// IsSelect指定
            /// </summary>
            Select,
            /// <summary>
            /// IsInstalled指定
            /// </summary>
            Installed,
        }

        /// <summary>
        /// profileのファイルバージョン
        /// </summary>
        private enum FileVersion
        {
            None,
            V100,
            V110,
        }

        /// <summary>
        /// profileの読み書き対象の種類
        /// </summary>
        private enum RWKeyType
        {
            Name,
            FileName,
            Version,
            Revision,
            RefType,
            InstallFiles,
        }

        private class InstallationItem
        {
            public string Name;
            public string FileName;
            public string Version;
            public uint Revision;
            public InstallItemList.RepoType RefRepoType;
            public string InstallFiles;
        }

        /// <summary>
        /// ファイルバージョンKeyと文字列の紐付け
        /// </summary>
        private static readonly Dictionary<FileVersion, string> _fileVersionDic = new Dictionary<FileVersion, string>()
        {
            { FileVersion.V100, "v1.0.0" },
            { FileVersion.V110, "v1.1.0" },
        };

        /// <summary>
        /// RWKeyTypeと文字列の紐付け
        /// </summary>
        private static readonly Dictionary<RWKeyType, string> _rwKeyTypeDic = new Dictionary<RWKeyType, string>()
        {
            { RWKeyType.Name, "name" },
            { RWKeyType.FileName, "filename" },
            { RWKeyType.Version, "version" },
            { RWKeyType.Revision, "revision" },
            { RWKeyType.RefType, "reftype" },
            { RWKeyType.InstallFiles, "installfiles" },
        };

        FileVersion fileVersion;
        public static string ReloadFileName { get; private set; }

        /// <summary>
        /// profileを読み出す
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="readType"></param>
        public void FileRead(string filePath, ReadType readType)
        {
            // ファイルからInstall済みを取得し、
            // Selectの時は、InstallItemのIsSelectをtrueにし、
            // Installedの時は、InstallItemのIsInstalledをtrueにする
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                TomlTable toml = Read(filePath);
                ConvertToData(toml, readType);
            }
            catch
            {
                throw;
            }
        }

        private void ConvertToData(TomlTable tomlTable, ReadType readType)
        {
            fileVersion = FileVersion.None;
            object tomlTableList;

            try
            {
                ConvertToData(tomlTable, out tomlTableList);
            }
            catch
            {
                throw;
            }

            if (tomlTableList == null) return;

            InstallItemList.ClearIsInstalled();
            InstallItemList.ClearAllIsSelect();

            List<InstallationItem> installationItemList = new List<InstallationItem>();

            switch (fileVersion)
            {
                case FileVersion.V100:
                    installationItemList = SetEnableListV100((List<TomlTable>)tomlTableList, readType);
                    break;
                case FileVersion.V110:
                    installationItemList = SetEnableListV110((List<TomlTable>)tomlTableList, readType);
                    break;
            }

            SetEnableItem(installationItemList, readType);
        }

        protected override void ConvertToData(TomlTable tomlData, out object data)
        {
            data = null;
            string tomlFileVersion = tomlData.Get<string>("version");

            try
            {
                fileVersion = _fileVersionDic.First(x => x.Value == tomlFileVersion).Key;
            }
            catch
            {
                throw new KeyNotFoundException($"読み込んだファイルのバージョンが不正です({tomlFileVersion})");
            }

            data = tomlData.Get<TomlTableArray>(_mainKeyName).Items;
        }

        /// <summary>
        /// v1.0.0用読み出し
        /// </summary>
        /// <param name="tomlTableList"></param>
        /// <param name="readType"></param>
        /// <returns></returns>
        private List<InstallationItem> SetEnableListV100(List<TomlTable> tomlTableList, ReadType readType)
        {
            List<InstallationItem> installationItemList = new List<InstallationItem>();

            TomlTableArray array = new TomlTableArray(new Root(), tomlTableList);

            for (int i = 0; i < array.Count; i++)
            {
                InstallationItem installationItem = new InstallationItem()
                {
                    Name = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Name]),
                    FileName = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.FileName]),
                    Version = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Version]),
                    Revision = 0,
                    RefRepoType = (InstallItemList.RepoType)array[i].Get<int>(_rwKeyTypeDic[RWKeyType.RefType]),
                    InstallFiles = "",
                };

                installationItemList.Add(installationItem);
            }

            return installationItemList;
        }


        /// <summary>
        /// v1.1.0用読み出し
        /// </summary>
        /// <param name="tomlTableList"></param>
        /// <param name="readType"></param>
        /// <returns></returns>
        private List<InstallationItem> SetEnableListV110(List<TomlTable> tomlTableList, ReadType readType)
        {
            List<InstallationItem> installationItemList = new List<InstallationItem>();

            TomlTableArray array = new TomlTableArray(new Root(), tomlTableList);

            for (int i = 0; i < array.Count; i++)
            {

                InstallationItem installationItem = new InstallationItem()
                {
                    Name = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Name]),
                    FileName = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.FileName]),
                    Version = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Version]),
                    Revision = array[i].Get<uint>(_rwKeyTypeDic[RWKeyType.Revision]),
                    RefRepoType = (InstallItemList.RepoType)array[i].Get<int>(_rwKeyTypeDic[RWKeyType.RefType]),
                    InstallFiles = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.InstallFiles]),
                };

                installationItemList.Add(installationItem);
            }

            return installationItemList;
        }

        /// <summary>
        /// 読み込んだInstallationItamListをInstallItemListに反映させる
        /// </summary>
        /// <param name="list">読み込んだInstallationItemList</param>
        /// <param name="readType">読み込んだ時の種類</param>
        private void SetEnableItem(List<InstallationItem> list, ReadType readType)
        {
            switch (readType)
            {
                case ReadType.Select:
                    foreach (InstallationItem item in list)
                    {
                        // 指定リポジトリに無いアイテムは除外
                        if (!InstallItemList.CheckDuplicateName(item.RefRepoType, item.Name)) continue;

                        InstallItemList.SetIsSelect(item.RefRepoType, item.Name, true);
                        InstallItemList.SetUninstallFileList(item.RefRepoType, item.Name, item.InstallFiles);
                        if ((InstallItemList.RepoType.User == item.RefRepoType) && InstallItemList.CheckDuplicateName(InstallItemList.RepoType.Pre, item.Name))
                        {
                            // ユーザーリポジトリ優先
                            InstallItemList.SetIsSelect(InstallItemList.RepoType.Pre, item.Name, false);
                        }
                    }
                    break;
                case ReadType.Installed:
                    foreach (InstallationItem item in list)
                    {
                        // 指定リポジトリにないアイテムは除外
                        if (!InstallItemList.CheckDuplicateName(item.RefRepoType, item.Name)) continue;

                        InstallItemList.SetIsInstalled(item.RefRepoType, item.Name, InstallStatus.Installed, item.Revision);
                        InstallItemList.SetProfileItemRevition(item.RefRepoType, item.Name, item.Revision);
                        InstallItemList.SetUninstallFileList(item.RefRepoType, item.Name, item.InstallFiles);
                        if ((InstallItemList.RepoType.User == item.RefRepoType) && InstallItemList.CheckDuplicateName(InstallItemList.RepoType.Pre, item.Name))
                        {
                            // ユーザーリポジトリ優先
                            InstallItemList.SetIsInstalled(InstallItemList.RepoType.Pre, item.Name, InstallStatus.NotInstall);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// profileをファイルに保存
        /// </summary>
        /// <param name="dirPath"></param>
        public void FileWrite(string dirPath)
        {
            // InstallItemListから、IsInstalledを抽出して、itemListにする
            List<InstallationItem> itemList = new List<InstallationItem>();
            {
                // List<InstallationItem>に変換
                InstallItemList installItemList = new InstallItemList();

                for (var i = InstallItemList.RepoType.Pre; i < InstallItemList.RepoType.MAX; i++)
                {
                    foreach (InstallItem installItem in installItemList.GetInstalItemList(i))
                    {
                        if (InstallStatus.NotInstall == installItem.IsInstalled) continue;
                        // アップデートせずにここに来た時、どうやって

                        var item = new InstallationItem()
                        {
                            Name = installItem.Name,
                            FileName = installItem.DownloadFileName,
                            Version = installItem.Version,
                            Revision = (installItem.IsInstalled == InstallStatus.Installed) ? installItem.ItemRevision : installItem.ProfileItemRevision,
                            RefRepoType = i,
                            InstallFiles = installItem.InstallFile,
                        };

                        itemList.Add(item);
                    }
                }
            }

            TomlTable table = ConvertToTomlTable(itemList);
            ReloadFileName = $"{_fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.{_fileExtension}";
            Write(table, $"{dirPath}\\{ReloadFileName}");
        }

        protected override TomlTable ConvertToTomlTable(in object data)
        {
            // dataからtomlファイル生成の元ネタを作成

            TomlTable toml = Toml.Create();

            toml.Add("version", _fileVersionDic.Last().Value);

            List<TomlTable> tomlList = new List<TomlTable>();
            foreach (var item in (List<InstallationItem>)data)
            {
                var tomlItem = Toml.Create();
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileName], item.FileName);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.Revision], item.Revision);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.RefType], (int)item.RefRepoType);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.InstallFiles], item.InstallFiles);

                tomlList.Add(tomlItem);
            }

            tomlList = tomlList.Distinct(new TomlTableComparer()).ToList();
            TomlTableArray tomlTableArray = new TomlTableArray(new Root(), tomlList);
            toml.Add(_mainKeyName, tomlTableArray);

            return toml;
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
