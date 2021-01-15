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
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), $"Command action cannot be null!");
            }

            Action = action;
            CanExecutePredicate = canExecutePredicate;
        }

        public bool CanExecute(object parameter) => CanExecutePredicate == null ? true : CanExecutePredicate(parameter);

        public void Execute(object parameter) => Action(parameter);
    }
}
