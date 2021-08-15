using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class PreRepoFileR
    {
        private static readonly string _installTableName = "InstallList";
        private static readonly string _priorityTableName = "Priority";
        private static readonly string _fileTypeTableName = "FileType";
        private static readonly string _versionTableName = "Version";
        private static readonly string _sectionTableName = "Section";
        private static readonly string _makerTableName = "MakerName";
        private static readonly string _dependentTableName = "Dependent";
        private static SQLiteConnection connection;

        public PreRepoFileR(string databaseFilePath)
        {
            SQLiteConnectionStringBuilder connectionSB = new SQLiteConnectionStringBuilder() { DataSource = databaseFilePath };
            connection = new SQLiteConnection(connectionSB.ToString());
        }

        public void Open()
        {
            connection.Open();
        }

        public void Close()
        {
            connection.Close();
            connection = null;
        }

        /// <summary>
        /// DBバージョンを取得
        /// </summary>
        /// <param name="major">メジャーバージョン</param>
        /// <param name="minor">マイナーバージョン</param>
        /// <param name="maintenance">メンテナンスバージョン</param>
        /// <param name="app_match">アプリ整合性バージョン</param>
        /// <returns>成否</returns>
        public bool GetDBVersion(out uint major, out uint minor, out uint maintenance, out uint app_match)
        {
            major = minor = maintenance = app_match = 0;

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = $"select * from {_versionTableName} where id = 1";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        major = uint.Parse(reader["major"].ToString());
                        minor = uint.Parse(reader["minor"].ToString());
                        maintenance = uint.Parse(reader["maintenance"].ToString());
                        app_match = uint.Parse(reader["app_match"].ToString());
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// セクションリストを読み出す
        /// </summary>
        /// <returns></returns>
        public bool ReadSectionList()
        {
            InstallItem.SectionTypeDic.Clear();
            int cnt = 0;
            InstallItem.SectionTypeDic.Add(cnt++, "全て");
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = $"select * from {_sectionTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InstallItem.SectionTypeDic.Add(cnt++, reader["section"].ToString());
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 製作者名リストを読み出す
        /// </summary>
        /// <returns></returns>
        public bool ReadMakerList()
        {
            InstallItem.MakerTypeDic.Clear();
            int cnt = 0;
            InstallItem.MakerTypeDic.Add(cnt++, "全て");
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = $"select * from {_makerTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            InstallItem.MakerTypeDic.Add(cnt++, reader["maker_name"].ToString());
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// インストールリストを読み出す
        /// </summary>
        /// <returns>成否</returns>
        public bool ReadInstallItemList()
        {
            InstallItemList.ItemClear(InstallItemList.RepoType.Pre);
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = $"select * from {_installTableName} natural join {_priorityTableName} natural join {_fileTypeTableName} natural join {_sectionTableName} natural join {_makerTableName} natural join {_dependentTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InstallItem item = new InstallItem
                            {
                                Priority = (InstallPriority)uint.Parse(reader["priority"].ToString()),
                                Name = reader["name"].ToString(),
                                CommandName = reader["command_name"].ToString(),
                                IsSpecialItem = bool.Parse(reader["is_special_item"].ToString()),
                                MakerName = reader["maker_name"].ToString(),
                                URL = reader["url"].ToString(),
                                DownloadFileName = reader["download_file_name"].ToString(),
                                FileType = (InstallFileType)uint.Parse(reader["file_type"].ToString()),
                                Version = reader["version"].ToString(),
                                ScriptDirName = reader["script_dir_name"].ToString(),
                                InstallFile = reader["install_file"].ToString(),
                                ExternalFile = reader["external_file"].ToString(),
                                ExternalFileURL = reader["external_file_url"].ToString(),
                                NicoVideoID = reader["nico_video_id"].ToString(),
                                SectionType = reader["section"].ToString(),
                                DependentName = reader["parent_name"].ToString()
                            };
                            item.IsSelect = ((InstallFileType.Main == item.FileType) || (item.CommandName == "exedit")) ? true : false;
                            item.IsItemSelectEnable = (InstallFileType.Main == item.FileType || (item.CommandName == "exedit")) ? false : true;

                            InstallItemList.AddInstallItem(InstallItemList.RepoType.Pre, item);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
