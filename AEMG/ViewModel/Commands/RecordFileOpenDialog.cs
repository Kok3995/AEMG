using System;
using System.Windows.Input;
using Microsoft.Win32;

namespace AEMG
{
    internal class RecordFileOpenDialog : ICommand
    {
        private AEMGViewModel _aEMGViewModel;

        public RecordFileOpenDialog(AEMGViewModel aEMGViewModel)
        {
            _aEMGViewModel = aEMGViewModel;
        }

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (openDialog.ShowDialog() == true)
                _aEMGViewModel.RecordFileLocation = openDialog.FileName;
        }
    }
}