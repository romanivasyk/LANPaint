using System;
using System.Windows.Input;
using LANPaint.MVVM;
using Xunit;

namespace LANPaint.UnitTests.MVVM
{
    public class RelayCommandTTests
    {
        [Fact]
        public void Ctor_ValidData()
        {
            static void Action(int a)
            {
            }
            
            static bool CanExecute() => true;
            var command = new RelayCommand<int>(Action, CanExecute);
        }

        [Fact]
        public void Ctor_NullAction()
        {
            Action<int> action = null;
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<int>(action));
        }

        [Fact]
        public void Execute_ExecuteAction()
        {
            var state = 10;
            const int addition = 5;
            var expected = state + addition;

            void Action(int input) => state += input;
            var command = new RelayCommand<int>(Action);

            command.Execute(addition);

            Assert.Equal(expected, state);
        }

        [Fact]
        public void CanExecute_DefaultCanExecute()
        {
            static void Action(int a)
            {
            }

            var command = new RelayCommand<int>(Action);

            var canExecuteResult = command.CanExecute(default);

            Assert.True(canExecuteResult);
        }

        [InlineData(true, true)]
        [InlineData(false, false)]
        [Theory]
        public void CanExecute_CanExecuteTheory(bool canExecuteReturn, bool expectedReturn)
        {
            static void Action(int a)
            {
            }

            bool CanExecute() => canExecuteReturn;
            var command = new RelayCommand<int>(Action, CanExecute);

            var canExecuteResult = command.CanExecute(default);

            Assert.Equal(expectedReturn, canExecuteResult);
        }

        [Fact]
        public void RaiseCanExecute()
        {
            var isCanExecuteChangedExecuted = false;

            static void Action(int a)
            {
            }

            var command = new RelayCommand<int>(Action);
            command.CanExecuteChanged += (sender, args) => isCanExecuteChangedExecuted = true;

            command.RaiseCanExecuteChanged();

            Assert.True(isCanExecuteChangedExecuted);
        }

        [Fact]
        public void ICommandExecute_ExecuteAction()
        {
            var state = 10;
            const int addition = 5;
            var expected = state + addition;

            void Action(int input) => state += input;
            var command = (ICommand) new RelayCommand<int>(Action);

            command.Execute(addition);

            Assert.Equal(expected, state);
        }

        [InlineData(true, true)]
        [InlineData(false, false)]
        [Theory]
        public void ICommandExecute_CanExecuteTheory(bool canExecuteReturn, bool expectedReturn)
        {
            static void Action(int a)
            {
            }

            bool CanExecute() => canExecuteReturn;
            var command = (ICommand) new RelayCommand<int>(Action, CanExecute);

            var canExecuteResult = command.CanExecute(default(int));

            Assert.Equal(expectedReturn, canExecuteResult);
        }
    }
}