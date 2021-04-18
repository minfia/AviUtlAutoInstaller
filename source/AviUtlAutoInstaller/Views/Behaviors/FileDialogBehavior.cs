using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace AviUtlAutoInstaller.Views.Behaviors
{
    /// <summary>
    /// コモンダイアログに関するビヘイビア
    /// </summary>
    internal class FileDialogBehavior
    {
        #region 添付プロパティ群
        public static readonly DependencyProperty CallbackProperty = DependencyProperty.RegisterAttached("Callback", typeof(Action<bool, string>), typeof(FileDialogBehavior), new PropertyMetadata(null, OnCallbackPropertyChanged));

        #region コールバック
        /// <summary>
        /// 添付プロパティを取得 (コールバック)
        /// </summary>
        /// <param name="target">取得するDependencyObject</param>
        /// <returns></returns>
        public static Action<bool, string> GetCallback(DependencyObject target)
        {
            return (Action<bool, string>)target.GetValue(CallbackProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(コールバック)
        /// </summary>
        /// <param name="target">設定するDependencyObject</param>
        /// <param name="value">設定するアクション</param>
        public static void SetCallback(DependencyObject target, Action<bool, string> value)
        {
            target.SetValue(CallbackProperty, value);
        }

        /// <summary>
        /// 添付プロパティ変更イベントハンドラ(コールバック)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCallbackPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var callback = GetCallback(sender);
            if (callback != null)
            {
                object dlg;

                switch (GetOpenSave(sender))
                {
                    case OpenSaveType.Open:
                        dlg = new OpenFileDialog();
                        ((OpenFileDialog)dlg).Multiselect = GetMultiselect(sender);
                        break;
                    case OpenSaveType.Save:
                        dlg = new SaveFileDialog();
                        break;
                    case OpenSaveType.Folder:
                        dlg = new FolderDialog();
                        break;
                    default:
                        return;
                }

                var owner = Window.GetWindow(sender);
                if ((OpenSaveType.Open == GetOpenSave(sender)) || (OpenSaveType.Save == GetOpenSave(sender)))
                {
                    ((FileDialog)dlg).Title = GetTitle(sender);
                    ((FileDialog)dlg).Filter = GetFilter(sender);
                    var result = ((FileDialog)dlg).ShowDialog(owner);
                    callback(result.Value, ((FileDialog)dlg).FileName);
                }
                else if (OpenSaveType.Folder == GetOpenSave(sender))
                {
                    ((FolderDialog)dlg).Title = GetTitle(sender);
                    var result = ((FolderDialog)dlg).ShowDialog(owner);
                    callback(result.Value, ((FolderDialog)dlg).SelectedPath);
                }
            }
        }
        #endregion

        #region タイトル
        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached("Title", typeof(string), typeof(FileDialogBehavior), new PropertyMetadata("ファイルを開く"));

        /// <summary>
        /// 添付プロパティを取得(タイトル)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetTitle(DependencyObject target)
        {
            return (string)target.GetValue(TitleProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(タイトル)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetTitle(DependencyObject target, string value)
        {
            target.SetValue(TitleProperty, value);
        }
        #endregion

        #region フィルタ
        public static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(string), typeof(FileDialogBehavior), new PropertyMetadata("すべてのファイル (*.*)|*.*"));

        /// <summary>
        /// 添付プロパティを取得(フィルタ)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetFilter(DependencyObject target)
        {
            return (string)target.GetValue(FilterProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(フィルタ)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetFilter(DependencyObject target, string value)
        {
            target.SetValue(FilterProperty, value);
        }
        #endregion

        #region マルチセレクト
        public static readonly DependencyProperty MultiselectProprty = DependencyProperty.RegisterAttached("Multiselect", typeof(bool), typeof(FileDialogBehavior), new PropertyMetadata(true));

        /// <summary>
        /// 添付プロパティを取得(マルチセレクト)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetMultiselect(DependencyObject target)
        {
            return (bool)target.GetValue(MultiselectProprty);
        }

        /// <summary>
        /// 添付プロパティを設定(マルチセレクト)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetMultiselect(DependencyObject target, bool value)
        {
            target.SetValue(MultiselectProprty, value);
        }
        #endregion

        #region ダイアログタイプ
        public enum OpenSaveType
        {
            Open,
            Save,
            Folder,
        }
        public static readonly DependencyProperty OpenSaveProperty = DependencyProperty.RegisterAttached("OpenSave", typeof(OpenSaveType), typeof(FileDialogBehavior), new PropertyMetadata(OpenSaveType.Open));

        /// <summary>
        /// 添付プロパティを取得(ダイアログタイプ)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static OpenSaveType GetOpenSave(DependencyObject target)
        {
            return (OpenSaveType)target.GetValue(OpenSaveProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(ダイアログタイプ)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetOpenSave(DependencyObject target, OpenSaveType value)
        {
            target.SetValue(OpenSaveProperty, value);
        }
        #endregion

        #endregion
    }

    #region フォルダ選択ダイアログ関連
    /*
     * 以下のサイトで紹介されているコードをベースにFileDialogBehavior用に改変
     * https://shikaku-sh.hatenablog.com/entry/wpf-folder-selection-dialog
     */

    /// <summary>
    /// FolderDialog クラスは、フォルダを選択する機能を提供するクラスです。
    /// </summary>
    internal class FolderDialog
    {
        [DllImport("shell32.dll")]
        private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgfInOut);

        [DllImport("shell32.dll")]
        private static extern int SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr pidl, out IShellItem ppsi);


        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialogInternal { }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHadler();
            void GetParent();
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes();
            void Compare();
        }

        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            uint Show([In] IntPtr parent);
            void SetFileTypes();
            void SetFileTypeIndex([In] uint iFileType);
            void GetGileTypeIndex(out uint piFileType);
            void Advise();
            void Unadvise();
            void SetOptions([In] _FILEOPENDIALOGOPTIONS fos);
            void GetOptions(out _FILEOPENDIALOGOPTIONS pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int alignment);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid();
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
            void GetResults([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenum);
            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppsai);
        }

        private const uint ERROR_CANCELLED = 0x800704C7;

        /// <summary>
        /// ユーザーによって選択されたフォルダーのパスを取得または設定します。
        /// </summary>
        public string SelectedPath { get; set; }

        /// <summary>
        /// ダイアログ上に表示されるタイトルのテキストを取得または設定します。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// <see cref="FolderDialog"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public FolderDialog() { }

        public bool? ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        public bool? ShowDialog(Window owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("指定したウィンドウは null です。オーナーを正しく設定できません。");
            }

            var handle = new WindowInteropHelper(owner).Handle;

            return ShowDialog(handle);
        }

        public bool? ShowDialog(IntPtr owner)
        {
            var dialog = new FileOpenDialogInternal() as IFileOpenDialog;

            try
            {
                IShellItem item;
                string selectedPath;

                dialog.SetOptions(_FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | _FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);

                if (!string.IsNullOrEmpty(SelectedPath))
                {
                    IntPtr idl = IntPtr.Zero; // path の intptr
                    uint attributes = 0;

                    if (SHILCreateFromPath(SelectedPath, out idl, ref attributes) == 0)
                    {
                        if (SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, idl, out item) == 0)
                        {
                            dialog.SetFolder(item);
                        }

                        if (idl != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(idl);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Title))
                {
                    dialog.SetTitle(Title);
                }

                var hr = dialog.Show(owner);

                // 選択のキャンセルまたは例外
                if (hr == ERROR_CANCELLED) return false;
                if (hr != 0) return false;

                dialog.GetResult(out item);

                if (item != null)
                {
                    item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out selectedPath);
                    SelectedPath = selectedPath;
                }
                else
                {
                    return false;
                }

                return true;
            }
            finally
            {
                Marshal.FinalReleaseComObject(dialog);
            }
        }

        /// <summary>
        /// <see cref="_FILEOPENDIALOGOPTIONS"/> 列挙型は、[開く] または [保存] ダイアログで使用できるオプションのセットを定義します。
        /// </summary>
        [Flags]
        public enum _FILEOPENDIALOGOPTIONS : uint
        {
            FOS_OVERWRITEPROMPT = 0x00000002,
            FOS_STRICTFILETYPES = 0x00000004,
            FOS_NOCHANGEDIR = 0x00000008,
            /// <summary>
            /// ファイルではなくフォルダを選択できる [開く] ダイアログボックスを表示します。
            /// </summary>
            FOS_PICKFOLDERS = 0x00000020,
            /// <summary>
            /// ファイルシステムのアイテムを返却します。
            /// </summary>
            FOS_FORCEFILESYSTEM = 0x00000040,
            FOS_ALLNONSTORAGEITEMS = 0x00000080,
            FOS_NOVALIDATE = 0x00000100,
            FOS_ALLOWMULTISELECT = 0x00000200,
            FOS_PATHMUSTEXIST = 0x00000800,
            FOS_FILEMUSTEXIST = 0x00001000,
            FOS_CREATEPROMPT = 0x00002000,
            FOS_SHAREAWARE = 0x00004000,
            FOS_NOREADONLYRETURN = 0x00008000,
            FOS_NOTESTFILECREATE = 0x00010000,
            FOS_HIDEMRUPLACES = 0x00020000,
            FOS_HIDEPINNEDPLACES = 0x00040000,
            FOS_NODEREFERENCELINKS = 0x00100000,
            FOS_DONTADDTORECENT = 0x02000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_FORCEPREVIEWPANEON = 0x40000000,
            FOS_SUPPORTSTREAMABLEITEMS = 0x80000000
        }

        /// <summary>
        /// SIGDN クラスは、IShellItem::GetDisplayName および SHGetNameFromIDList を使用して取得するアイテムの表示名の形式を定義します。
        /// </summary>
        private enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }
    }
    #endregion
}
