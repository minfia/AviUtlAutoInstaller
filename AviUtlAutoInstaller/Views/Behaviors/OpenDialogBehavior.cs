using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;

namespace AviUtlAutoInstaller.Views.Behaviors
{
    /// <summary>
    /// オープンダイアログに関するビヘイビア
    /// </summary>
    class OpenDialogBehavior
    {
        #region 添付プロパティ(データコンテキスト)
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.RegisterAttached("DataContext", typeof(object), typeof(OpenDialogBehavior), new PropertyMetadata(null));

        /// <summary>
        /// 添付プロパティを取得(データコンテキスト)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object GetDataContext(DependencyObject target)
        {
            return target.GetValue(DataContextProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(データコンテキスト)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetDataContext(DependencyObject target, object value)
        {
            target.SetValue(DataContextProperty, value);
        }
        #endregion

        #region 添付プロパティ(ウィンドウタイプ)
        public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.RegisterAttached("WindowType", typeof(Type), typeof(OpenDialogBehavior), new PropertyMetadata(null));

        /// <summary>
        /// 添付プロパティを取得(ウィンドウタイプ)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Type GetWindowType(DependencyObject target)
        {
            return (Type)target.GetValue(WindowTypeProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(ウィンドウタイプ)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetWindowType(DependencyObject target, Type value)
        {
            target.SetValue(WindowTypeProperty, value);
        }
        #endregion

        #region 添付プロパティ(コールバック)
        public static readonly DependencyProperty CallbackProperty = DependencyProperty.RegisterAttached("Callback", typeof(Action<bool>), typeof(OpenDialogBehavior), new PropertyMetadata(null, OnCallbackPropertyChanged));

        /// <summary>
        /// 添付プロパティを取得(コールバック)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Action<bool> GetCallback(DependencyObject target)
        {
            return (Action<bool>)target.GetValue(CallbackProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(コールバック)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetCallback(DependencyObject target, Action<bool> value)
        {
            target.SetValue(CallbackProperty, value);
        }

        private static void OnCallbackPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var callback = GetCallback(sender);
            if (callback != null)
            {
                var type = GetWindowType(sender);
                var obj = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                var child = obj as Window;
                if (child != null)
                {
                    child.Owner = Window.GetWindow(sender);
                    child.DataContext = GetDataContext(sender);
                    bool modal = GetModal(sender);
                    if (modal)
                    {
                        var result = child.ShowDialog();
                        callback(result.Value);
                    }
                    else
                    {
                        child.Show();
                    }
                }
            }
        }
        #endregion

        #region 添付プロパティ(モーダル)
        public static readonly DependencyProperty ModalProperty = DependencyProperty.RegisterAttached("Modal", typeof(bool), typeof(OpenDialogBehavior), new PropertyMetadata(null));

        /// <summary>
        /// 添付プロパティを取得(モーダル)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetModal(DependencyObject target)
        {
            return (bool)target.GetValue(ModalProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(モーダル)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetModal(DependencyObject target, bool value)
        {
            target.SetValue(ModalProperty, value);
        }
        #endregion
    }
}
