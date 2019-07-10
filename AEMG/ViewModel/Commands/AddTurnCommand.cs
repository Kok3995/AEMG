using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AEMG
{
    internal class AddTurnCommand : ICommand
    {
        /// <summary>
        /// An reference to the viewmodel to access it
        /// </summary>
        private AEMGViewModel _aEMGViewModel;

        /// <summary>
        /// Default Contructor: Pass in an instance of viewmodel to access it
        /// </summary>
        /// <param name="aEMGViewModel">the main viewmodel</param>
        public AddTurnCommand(AEMGViewModel aEMGViewModel)
        {
            _aEMGViewModel = aEMGViewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Execute the AddTab method
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _aEMGViewModel.AddTab();
        }
    }
}