using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SystemProgramming4.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private Predicate<object?> canExecute_;
        private Action<object?> execute_;

        public RelayCommand(Action<object?> execute_, Predicate<object?> canExecute_ = null)
        {
            ArgumentNullException.ThrowIfNull(execute_);
            this.canExecute_ = canExecute_;
            this.execute_ = execute_;
        }

        public bool CanExecute(object? parameter) => canExecute_ is null || canExecute_.Invoke(parameter);

        public void Execute(object? parameter) => execute_.Invoke(parameter);
    }
}
