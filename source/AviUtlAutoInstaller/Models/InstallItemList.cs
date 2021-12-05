using AviUtlAutoInstaller.Models.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models
{
    class InstallItemList
    {
        public enum RepoType
        {
            /// <summary>
            /// プリインストール
            /// </summary>
            Pre,
            /// <summary>
            /// ユーザー
            /// </summary>
            User,

            /// <summary>
            /// RepoTypeの最大数
            /// </summary>
            MAX
        }

        /// <summary>
        /// 各リポジトリ
        /// </summary>
        private static ObservableCollection<InstallItem>[] _installItemList = new ObservableCollection<InstallItem>[(int)RepoType.MAX]
        {
            new ObservableCollection<InstallItem>(),
            new ObservableCollection<InstallItem>()
        };

        /// <summary>
        /// インストールアイテム一覧を取得
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <returns></returns>
        public ObservableCollection<InstallItem> GetInstalItemList(RepoType repoType)
        {
            return _installItemList[(int)repoType];
        }

        /// <summary>
        /// インストールアイテムを追加
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="item"></param>
        public static void AddInstallItem(RepoType repoType, InstallItem item)
        {
            _installItemList[(int)repoType].Add(item);
            int no = _installItemList[(int)repoType].IndexOf(item);
            if (-1 < no)
            {
                _installItemList[(int)repoType][no].No = no + 1;
            }
        }

        /// <summary>
        /// インストールアイテムを編集
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="targetItem">対象のインストールアイテム</param>
        /// <param name="modifyItem">変更後のインストールアイテム</param>
        public static void ModifyInstallItem(RepoType repoType, InstallItem targetItem, InstallItem modifyItem)
        {
            int index = _installItemList[(int)repoType].IndexOf(targetItem);
            _installItemList[(int)repoType][index].Name = modifyItem.Name;
            _installItemList[(int)repoType][index].URL = modifyItem.URL;
            _installItemList[(int)repoType][index].DownloadFileName = modifyItem.DownloadFileName;
            _installItemList[(int)repoType][index].FileType = modifyItem.FileType;
            _installItemList[(int)repoType][index].Version = modifyItem.Version;
            _installItemList[(int)repoType][index].ScriptDirName = modifyItem.ScriptDirName;
            _installItemList[(int)repoType][index].InstallFile = modifyItem.InstallFile;
            _installItemList[(int)repoType][index].NicoVideoID = modifyItem.NicoVideoID;
        }

        /// <summary>
        /// インストールアイテムを削除
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="deleteItem">削除するInstallItem</param>
        public static void DeleteInstallItem(RepoType repoType, List<InstallItem> deleteItemList)
        {
            foreach (InstallItem item in deleteItemList)
            {
                _installItemList[(int)repoType].Remove(item);
            }
            OrganizeNo(repoType);
        }

        /// <summary>
        /// インストールアイテムをクリア
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        public static void ItemClear(RepoType repoType)
        {
            _installItemList[(int)repoType].Clear();
        }

        /// <summary>
        /// 項目名の重複チェック
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="name">確認する項目名</param>
        /// <returns></returns>
        public static bool CheckDuplicateName(RepoType repoType, string name)
        {
            return _installItemList[(int)repoType].Any(x => x.Name == name);
        }

        /// <summary>
        /// URLの重複チェック
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="url">確認するURL</param>
        /// <returns></returns>
        public static bool CheckDuplicateURL(RepoType repoType, string url)
        {
            return _installItemList[(int)repoType].Any(x => x.URL == url);
        }

        /// <summary>
        /// ファイル名の重複チェック
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="fileName">確認するファイル名</param>
        /// <returns></returns>
        public static bool CheckDuplicateFileName(RepoType repoType, string fileName)
        {
            return _installItemList[(int)repoType].Any(x => x.DownloadFileName == fileName);
        }

        /// <summary>
        /// インストール完了の有無をセットする
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="name">セットする項目名</param>
        /// <param name="b">true(有効) or false(無効)</param>
        public static void SetIsInstalled(RepoType repoType, string name, bool b)
        {
            foreach (InstallItem item in _installItemList[(int)repoType])
            {
                if (item.Name == name)
                {
                    item.IsInstalled = b;
                    if ((name == "AviUtl") || (name == "拡張編集"))
                    {
                        item.IsSelect = b;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// インストールの有無をクリアする
        /// </summary>
        public static void ClearAllIsSelect()
        {
            for (RepoType i = 0; i < RepoType.MAX; i++)
            {
                foreach (var item in _installItemList[(int)i])
                {
                    item.IsSelect = false;
                }
            }

            // 必須項目のため有効
            SetIsInstalled(RepoType.Pre, "AviUtl", true);
            SetIsInstalled(RepoType.Pre, "拡張編集", true);
        }

        /// <summary>
        /// インストールアイテムの項目番号を採番
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        private static void OrganizeNo(RepoType repoType)
        {
            foreach (InstallItem item in _installItemList[(int)repoType])
            {
                int no = _installItemList[(int)repoType].IndexOf(item);
                if (-1 < no)
                {
                    _installItemList[(int)repoType][no].No = no + 1;
                }
            }
        }

        /// <summary>
        /// 依存先のチェックを行い、インストールの有効/無効を設定する(プリインストールリポジトリ専用)
        /// </summary>
        /// <param name="DependentName"></param>
        public static void DependentAction(InstallItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.DependentName)) return;

            if (item.IsSelect && !item.DependentName.Contains("None"))
            {
                // 依存アイテム -> 依存元
                InstallItem dependentItem = _installItemList[(int)RepoType.Pre].FirstOrDefault(x => x.CommandName == item.DependentName);
                if (dependentItem == null) return;

                dependentItem.IsSelect = true;

                if (string.IsNullOrEmpty(dependentItem.DependentName)) DependentAction(dependentItem);

            }
            else
            {
                if (item.IsSelect) return;

                // 依存元 -> 依存アイテム
                var items = _installItemList[(int)RepoType.Pre].ToList().FindAll(x => x.DependentName == item.CommandName);

                foreach (var i in items)
                {
                    i.IsSelect = false;
                }
            }
        }

        /// <summary>
        /// インストールアイテムから、実際にインストールするファイル一覧を生成
        /// </summary>
        /// <param name="repoType">リポジトリの種類</param>
        /// <param name="item">InstallItem</param>
        /// <returns></returns>
        public List<string> GenerateInstalList(RepoType repoType, InstallItem item)
        {
            List<string> readyInstallFiles = new List<string>(); // インストールするファイルのパス一覧

            string searchSrcDir;

            FileOperation fileOperation = new FileOperation();
            if (IsExtractExtension(item.DownloadFileName))
            {
                // ダウンロードしたファイルが圧縮ファイル
                string extractFile = $"{SysConfig.CacheDirPath}\\{item.DownloadFileName}";   // 解凍するファイルのパス: .\cache\FileName.圧縮形式
                string extractDestDir = $"{SysConfig.InstallExpansionDir}\\{Path.GetFileNameWithoutExtension(item.DownloadFileName)}";  //解凍先: root\EX_TEMP\FileNameDir\
                Extractor extractor = new Extractor();
                extractor.Extract(extractFile, extractDestDir);
                searchSrcDir = extractDestDir;
            }
            else
            {
                // ダウンロードしたファイルがスクリプトorプラグインファイル
                File.Copy($"{SysConfig.CacheDirPath}\\{item.DownloadFileName}", $"{SysConfig.InstallExpansionDir}\\{item.DownloadFileName}", true);
                searchSrcDir = $"{SysConfig.InstallExpansionDir}";
            }

            if (RepoType.Pre == repoType)
            {
                readyInstallFiles = fileOperation.GenerateFilePathList(searchSrcDir, item.InstallFileList.ToArray());
            }
            else
            {
                if (InstallFileType.Plugin == item.FileType)
                {
                    readyInstallFiles = fileOperation.GenerateFilePathList(searchSrcDir, SysConfig.PluginFileExtension);
                }
                else if (InstallFileType.Script == item.FileType)
                {
                    readyInstallFiles = fileOperation.GenerateFilePathList(searchSrcDir, SysConfig.ScriptFileExtension);
                }

                if (0 < item.InstallFileList.Count)
                {
                    var itemPaths = fileOperation.GenerateFilePathList(searchSrcDir, item.InstallFileList.ToArray());
                    readyInstallFiles.AddRange(itemPaths);
                }
            }

            var installFiles = readyInstallFiles.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

            return installFiles;
        }

        /// <summary>
        /// ダウンロードしたファイルが圧縮ファイルか判定
        /// </summary>
        /// <param name="fileExtension">拡張子</param>
        /// <returns></returns>
        private bool IsExtractExtension(string fileName)
        {
            string fileExtension = "*" + Path.GetExtension(fileName);
            if (Array.IndexOf(SysConfig.PluginFileExtension, fileExtension) != -1)
            {
                return false;
            }
            if (Array.IndexOf(SysConfig.ScriptFileExtension, fileExtension) != -1)
            {
                return false;
            }

            return true;
        }
    }
}
