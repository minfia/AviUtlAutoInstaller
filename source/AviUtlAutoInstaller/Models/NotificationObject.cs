using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AviUtlAutoInstaller.Models
{
    class NotificationObject : INotifyPropertyChanged
    {
        #region INotiryPropertyChanged関係
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var h = PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(target, value))
            {
                return false;
            }
            target = value;
            RaisePropertyChanged(propertyName);

            return true;
        }
        #endregion
    }
}
