using System;
using System.Windows.Input;

namespace LANPaint.MVVM
{
    public class RelayCommand : ICommand
    {
        private Action Action { get; }
        private Func<bool> CanExecuteDelegate { get; }

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            CanExecuteDelegate = canExecute;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
        public bool CanExecute() => CanExecuteDelegate?.Invoke() ?? true;
        public void Execute() => Action();


        public event EventHandler CanExecuteChanged;
        bool ICommand.CanExecute(object parameter) => parameter == null
            ? CanExecute()
            : throw new ArgumentException("This implementation of RelayCommand doesn't support parameters.",
                nameof(parameter));
        void ICommand.Execute(object parameter)
        {
            if (parameter == null)
                Execute();
            else
                throw new ArgumentException("This implementation of RelayCommand doesn't support parameters.",
                    nameof(parameter));
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T> Action { get; }
        private Func<bool> CanExecuteDelegate { get; }

        public RelayCommand(Action<T> action, Func<bool> canExecuteDelegate = null)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            CanExecuteDelegate = canExecuteDelegate;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
        public bool CanExecute(T parameter) => CanExecuteDelegate?.Invoke() ?? true;
        public void Execute(T parameter) => Action(parameter);


        public event EventHandler CanExecuteChanged;
        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);
        void ICommand.Execute(object parameter) => Execute((T)parameter);
    }
}
