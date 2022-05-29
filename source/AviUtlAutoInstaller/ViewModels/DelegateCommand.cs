using System;
using System.Windows.Input;

namespace AviUtlAutoInstaller.ViewModels
{
    class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public DelegateCommand(Action<object> exetute) : this(exetute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return (_canExecute != null) ? _canExecute(parameter) : true;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExetuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute(parameter);
            }
        }
    }

    class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> exetute) : this(exetute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return (_canExecute != null) ? _canExecute((T)parameter) : true;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExetuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute((T)parameter);
            }
        }
    }
}
