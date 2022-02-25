using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class UserRepoFileRW : TomlFileRW
    {
        /// <summary>
        /// UserRepoのファイルバージョン
        /// </summary>
        private enum FileVersion
        {
            None,
            V100,
        }

        /// <summary>
        /// UserRepoの読み書き対象
        /// </summary>
        private enum RWKeyType
        {
            Name,
            URL,
            DownloadFileName,
            FileType,
            Version,
            ScriptDirName,
            AppendFile,
            NicoVideoID,
        }

        private FileVersion fileVersion;

        /// <summary>
        /// FileVersionとファイルのKeyの文字列との紐付け
        /// </summary>
        private static readonly Dictionary<FileVersion, string> _fileVersionDic = new Dictionary<FileVersion, string>()
        {
            { FileVersion.V100, "v1.0.0" },
        };

        /// <summary>
        /// RWKeyTypeとファイルのKeyの文字列との紐付け
        /// </summary>
        private static readonly Dictionary<RWKeyType, string> _rwKeyTypeDic = new Dictionary<RWKeyType, string>()
        {
            { RWKeyType.Name, "name" },
            { RWKeyType.URL, "url" },
            { RWKeyType.DownloadFileName, "downloadfilename" },
            { RWKeyType.FileType, "filetype" },
            { RWKeyType.Version, "version" },
            { RWKeyType.ScriptDirName, "scriptdirname" },
            { RWKeyType.AppendFile, "installfile" },
            { RWKeyType.NicoVideoID, "nicovideoid" },
        };

        /// <summary>
        /// ユーザーリポジトリファイルを読み出す
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
        /// ユーザーリポジトリをファイルに保存
        /// </summary>
        /// <param name="filePath"></param>
        public void FileWrite(string filePath)
        {
            TomlTable table = ConvertToTomlTable(null);
            Write(table, filePath);
        }

        private void ConvertToData(TomlTable data)
        {
            object tomlTableArray;
            try
            {
                ConvertToData(data, out tomlTableArray);
            }
            catch
            {
                throw;
            }

            if (tomlTableArray == null)
            {
                return;
            }

            InstallItemList.ItemClear(InstallItemList.RepoType.User);

            switch (fileVersion)
            {
                case FileVersion.V100:
                    AddInstallItemV100((TomlTableArray)tomlTableArray);
                    break;
                default:
                    return;
            }
        }

        protected override void ConvertToData(TomlTable tomlData, out object data)
        {
            data = null;
            string tomlFileVersion = tomlData.Get<string>("version");

            fileVersion = FileVersion.None;
            try
            {
                fileVersion = _fileVersionDic.First(x => x.Value == tomlFileVersion).Key;
            }
            catch
            {
                throw new KeyNotFoundException($"読み込んだファイルのバージョンが不正です({tomlFileVersion})");
            }

            data = tomlData.Get<TomlTableArray>("data");
        }

        protected override TomlTable ConvertToTomlTable(in object data)
        {
            TomlTable toml = Toml.Create();

            toml.Add("version", _fileVersionDic.Last().Value);

            List<TomlTable> table = new List<TomlTable>();
            InstallItemList installItemList = new InstallItemList();
            var itemList = installItemList.GetInstalItemList(InstallItemList.RepoType.User);

            foreach (InstallItem item in itemList)
            {
                var tomlItem = Toml.Create();

                tomlItem.Add(_rwKeyTypeDic[RWKeyType.Name], item.Name);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.URL], item.URL);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.DownloadFileName], item.DownloadFileName);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.FileType], (int)item.FileType);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.Version], item.Version);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.ScriptDirName], item.ScriptDirName);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.AppendFile], item.InstallFile);
                tomlItem.Add(_rwKeyTypeDic[RWKeyType.NicoVideoID], item.NicoVideoID);

                table.Add(tomlItem);
            }
            TomlTableArray array = new TomlTableArray(new Root(), table);
            toml.Add("data", array);

            return toml;
        }

        /// <summary>
        /// v1.0.0用読み出し
        /// </summary>
        /// <param name="array"></param>
        private void AddInstallItemV100(TomlTableArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                InstallItem item = new InstallItem
                {
                    Name = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Name]),
                    URL = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.URL]),
                    DownloadFileName = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.DownloadFileName]),
                    FileType = (InstallFileType)array[i].Get<int>(_rwKeyTypeDic[RWKeyType.FileType]),
                    Version = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.Version]),
                    ScriptDirName = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.ScriptDirName]),
                    InstallFile = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.AppendFile]),
                    NicoVideoID = array[i].Get<string>(_rwKeyTypeDic[RWKeyType.NicoVideoID])
                };
                item.IsDownloadCompleted = System.IO.File.Exists($"{SysConfig.CacheDirPath}\\{item.DownloadFileName}");

                if (!InstallItemList.CheckDuplicateName(InstallItemList.RepoType.User, item.Name) ||
                    !InstallItemList.CheckDuplicateURL(InstallItemList.RepoType.User, item.URL) ||
                    !InstallItemList.CheckDuplicateFileName(InstallItemList.RepoType.User, item.DownloadFileName))
                {
                    InstallItemList.AddInstallItem(InstallItemList.RepoType.User, item);
                }
            }
        }

        private class Root : ITomlRoot
        {
            public TomlSettings Settings => TomlSettings.Create();
        }

    }
}
