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

        public Action<object> Action { get; }
        public Predicate<object> CanExecutePredicate { get; }

        public RelayCommand(Action<object> action, Predicate<object> canExecutePredicate = null)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            CanExecutePredicate = canExecutePredicate;
        }

        public bool CanExecute(object parameter) => CanExecutePredicate?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => Action(parameter);
    }
}
