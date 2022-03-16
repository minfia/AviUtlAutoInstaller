using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Files
{
    class ContentsTreeRW
    {
        private const string _saveFileName = "contents_tree.txt";
        private static readonly List<string> _contentsList = new List<string>();

        /// <summary>
        /// コンテンツツリーの要素が存在するか
        /// </summary>
        public bool IsExistContents
        {
            get { return _contentsList.Count == 0 ? false : true; }
        }

        /// <summary>
        /// コンテンツの追加
        /// </summary>
        /// <param name="contents">コンテンツ</param>
        public static void AddContents(string contents)
        {
            _contentsList.Add(contents);
        }

        /// <summary>
        /// コンテンツの削除
        /// </summary>
        /// <param name="contents">コンテンツ</param>
        public static void DeleteContents(string contents)
        {
            if (0 <= _contentsList.IndexOf(contents))
            {
                _contentsList.Remove(contents);
            }
        }

        /// <summary>
        /// コンテンツツリーをファイルから読み出す
        /// </summary>
        /// <param name="dirPath"></param>
        public void Read(string dirPath)
        {

            if (!File.Exists($"{dirPath}\\{_saveFileName}"))
            {
                return;
            }
            _contentsList.Clear();

            using (StreamReader sr = new StreamReader($"{dirPath}\\{_saveFileName}", Encoding.UTF8))
            {
                while (sr.Peek() != -1)
                {
                    var contents = sr.ReadLine().Split(", ".ToCharArray());
                    foreach (var c in contents)
                    {
                        if (!string.IsNullOrWhiteSpace(c))
                        {
                            _contentsList.Add(c);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// コンテンツツリーをファイルに書き出す
        /// </summary>
        /// <param name="dirPath"></param>
        public void Write(string dirPath)
        {
            const int DivCount = 10;
            using (StreamWriter sw = new StreamWriter($"{dirPath}\\{_saveFileName}", false, Encoding.UTF8))
            {
                sw.NewLine = "\n";
                var contentsTreeList = _contentsList.Distinct().ToArray();
                long remain = contentsTreeList.Length;
                long loopCnt = 0;

                while (0 < remain)
                {
                    int div = (remain % DivCount == 0 || remain > DivCount) ? DivCount : (int)(remain % DivCount);

                    string line = "";

                    long row = loopCnt * DivCount;
                    for (int i = 0; i < div; i++, remain--)
                    {
                        line += contentsTreeList[row + i] + ' ';
                    }
                    sw.WriteLine(line);
                    loopCnt++;
                }
            }
        }
    }
}
