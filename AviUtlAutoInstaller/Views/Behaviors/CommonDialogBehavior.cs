using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MenuAndStatusBarAppTest.Views.Behaviors
{
    /// <summary>
    /// コモンダイアログに関するビヘイビア
    /// </summary>
    internal class CommonDialogBehavior
    {
        #region 添付プロパティ群
        public static readonly DependencyProperty CallbackProperty = DependencyProperty.RegisterAttached("Callback", typeof(Action<bool, string>), typeof(CommonDialogBehavior), new PropertyMetadata(null, OnCallbackPropertyChanged));

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
                var dlg = new OpenFileDialog()
                {
                    Title = GetTitle(sender),
                    Filter = GetFilter(sender),
                    Multiselect = GetMultiselect(sender),
                };
                var owner = Window.GetWindow(sender);
                var result = dlg.ShowDialog(owner);
                callback(result.Value, dlg.FileName);
            }
        }
        #endregion

        #region タイトル
        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached("Title", typeof(string), typeof(CommonDialogBehavior), new PropertyMetadata("ファイルを開く"));

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
        public static readonly DependencyProperty FilterProperty = DependencyProperty.RegisterAttached("Filter", typeof(string), typeof(CommonDialogBehavior), new PropertyMetadata("すべてのファイル (*.*)|*.*"));

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
        public static readonly DependencyProperty MultiselectProprty = DependencyProperty.RegisterAttached("Multiselect", typeof(bool), typeof(CommonDialogBehavior), new PropertyMetadata(true));

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

        #endregion
    }
}
