using System;
using System.Windows.Input;

namespace AEMG
{
    internal class RemoveTurnCommand : ICommand
    {
        /// <summary>
        /// An reference of viewmodel to access it
        /// </summary>
        private AEMGViewModel _aEMGViewModel;

        /// <summary>
        /// Default Contructor: Pass in an instance of main viewmodel to access it
        /// </summary>
        /// <param name="aEMGViewModel">The main viewmodel</param>
        public RemoveTurnCommand(AEMGViewModel aEMGViewModel)
        {
            _aEMGViewModel = aEMGViewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Remove button only enable when tab number greater than 1
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (_aEMGViewModel.BossTurnList.Count > 1)
                return true;
            else return false;
        }

        /// <summary>
        /// Execute RemoveTab methods
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _aEMGViewModel.RemoveTab();
        }
    }
}