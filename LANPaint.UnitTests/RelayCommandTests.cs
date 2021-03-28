using System;
using System.Windows.Input;
using LANPaint.MVVM;
using Xunit;

namespace LANPaint.UnitTests
{
    public class RelayCommandTests
    {
        [Fact]
        public void Ctor_ValidData()
        {
            static void Action()
            {
            }

            static bool CanExecute() => true;
            var command = new RelayCommand(Action, CanExecute);
        }

        [Fact]
        public void Ctor_NullAction()
        {
            Action action = null;
            Assert.Throws<ArgumentNullException>(() => new RelayCommand(action));
        }

        [Fact]
        public void Execute_ExecuteAction()
        {
            var isExecuted = false;

            void Action() => isExecuted = true;
            var command = new RelayCommand(Action);

            command.Execute();

            Assert.True(isExecuted);
        }

        [Fact]
        public void CanExecute_DefaultCanExecute()
        {
            static void Action()
            {
            }

            var command = new RelayCommand(Action);

            var canExecuteResult = command.CanExecute();

            Assert.True(canExecuteResult);
        }

        [InlineData(true, true)]
        [InlineData(false, false)]
        [Theory]
        public void CanExecute_CanExecuteTheory(bool canExecuteReturn, bool expectedReturn)
        {
            static void Action()
            {
            }

            bool CanExecute() => canExecuteReturn;
            var command = new RelayCommand(Action, CanExecute);

            var canExecuteResult = command.CanExecute();

            Assert.Equal(expectedReturn, canExecuteResult);
        }

        [Fact]
        public void RaiseCanExecute()
        {
            var isCanExecuteChangedExecuted = false;

            static void Action()
            {
            }

            var command = new RelayCommand(Action);
            command.CanExecuteChanged += (sender, args) => isCanExecuteChangedExecuted = true;

            command.RaiseCanExecuteChanged();

            Assert.True(isCanExecuteChangedExecuted);
        }

        [Fact]
        public void ICommandExecute_ExecuteAction()
        {
            var isExecuted = false;

            void Action() => isExecuted = true;
            var command = (ICommand) new RelayCommand(Action);

            command.Execute(null);

            Assert.True(isExecuted);
        }

        [Fact]
        public void ICommandExecute_ExecuteWithParameter()
        {
            void Action()
            {
            }

            var command = (ICommand) new RelayCommand(Action);

            Assert.Throws<ArgumentException>(() => command.Execute(new object()));
        }

        [InlineData(true, true)]
        [InlineData(false, false)]
        [Theory]
        public void ICommandExecute_CanExecuteTheory(bool canExecuteReturn, bool expectedReturn)
        {
            static void Action()
            {
            }

            bool CanExecute() => canExecuteReturn;
            var command = (ICommand) new RelayCommand(Action, CanExecute);

            var canExecuteResult = command.CanExecute(null);

            Assert.Equal(expectedReturn, canExecuteResult);
        }

        [Fact]
        public void ICommandExecute_CanExecuteWithParameter()
        {
            static void Action()
            {
            }

            static bool CanExecute() => true;
            var command = (ICommand) new RelayCommand(Action, CanExecute);

            Assert.Throws<ArgumentException>(() => command.CanExecute(new object()));
        }
    }
}