using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace AviUtlAutoInstaller.Views.Behaviors
{
    /// <summary>
    /// ウィンドウ終了に関するビヘイビア
    /// </summary>
    class WindowClosingBehavior
    {
        #region コールバック
        public static readonly DependencyProperty CallbackProperty = DependencyProperty.RegisterAttached("Callback", typeof(Func<bool>), typeof(WindowClosingBehavior), new PropertyMetadata(null, OnIsEnabledPropertyChanged));

        /// <summary>
        /// 添付プロパティを取得(コールバック)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Func<bool> GetCallback(DependencyObject target)
        {
            return (Func<bool>)target.GetValue(CallbackProperty);
        }

        /// <summary>
        /// 添付プロパティを設定(コールバック)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetCallback(DependencyObject target, Func<bool> value)
        {
            target.SetValue(CallbackProperty, value);
        }

        /// <summary>
        /// 添付プロパティ変更イベントハンドラ(コールバック)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnIsEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var w = sender as Window;
            if (w != null)
            {
                var callback = GetCallback(w);
                if ((callback != null) && (e.OldValue == null))
                {
                    w.Closing += OnClosing;
                }
                else if (callback == null)
                {
                    w.Closing -= OnClosing;
                }
            }
        }

        /// <summary>
        /// Closingイベントハンドラ(コールバック)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnClosing(object sender, CancelEventArgs e)
        {
            var callback = GetCallback(sender as DependencyObject);
            if (callback != null)
            {
                e.Cancel = !callback();
            }
        }
        #endregion
    }
}
