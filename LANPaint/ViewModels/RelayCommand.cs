using System;
using System.Windows.Input;

namespace LANPaint.ViewModels
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private Action Action { get; }
        private Func<bool> CanExecuteDelegate { get; }

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            CanExecuteDelegate = canExecute;
        }

        public bool CanExecute() => CanExecuteDelegate?.Invoke() ?? true;
        public void Execute() => Action();

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
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private Action<T> Action { get; }
        private Func<bool> CanExecuteDelegate { get; }

        public RelayCommand(Action<T> action, Func<bool> canExecuteDelegate = null)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            CanExecuteDelegate = canExecuteDelegate;
        }

        public bool CanExecute(T parameter) => CanExecuteDelegate?.Invoke() ?? true;
        public void Execute(T parameter) => Action(parameter);

        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);
        void ICommand.Execute(object parameter) => Execute((T)parameter);
    }
}
