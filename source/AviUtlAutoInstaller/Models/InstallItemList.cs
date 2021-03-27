using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            _installItemList[(int)repoType][index].FileName = modifyItem.FileName;
            _installItemList[(int)repoType][index].FileType = modifyItem.FileType;
            _installItemList[(int)repoType][index].Version = modifyItem.Version;
            _installItemList[(int)repoType][index].ScriptDirName = modifyItem.ScriptDirName;
            _installItemList[(int)repoType][index].AppendFile = modifyItem.AppendFile;
            _installItemList[(int)repoType][index].NicoVideoID = modifyItem.NicoVideoID;
        }

        /// <summary>
        /// インストールアイテムを削除
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="deleteItem">削除するInstallItem</param>
        public static void DeleteInstallItem(RepoType repoType, InstallItem deleteItem)
        {
            _installItemList[(int)repoType].Remove(deleteItem);
            OrganizeNo(repoType);

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
            return _installItemList[(int)repoType].Any(x => x.FileName == fileName);
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
    }
}
