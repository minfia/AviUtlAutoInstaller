using SevenZipExtractor;

namespace AviUtlAutoInstaller.Models.Files
{
    class Extractor
    {
        /// <summary>
        /// 圧縮ファイルを解凍
        /// </summary>
        /// <param name="srcFilePath">解凍するファイルのパス</param>
        /// <param name="destPath">解凍先のパス</param>
        public void Extract(string srcFilePath, string destPath)
        {
            using (ArchiveFile file = new(srcFilePath))
            {
                file.Extract(destPath, true);
            }
        }
    }
}
