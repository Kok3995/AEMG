using System;
using System.Windows;
using System.Windows.Input;

namespace AEMG
{
    class GenerateMacroCommand : ICommand
    {
        #region Private field       

        //main view model
        private AEMGViewModel _viewModel;

        #endregion

        #region Ctor

        /// <summary>
        /// Default Constructor: To Acess the main viewmodel
        /// </summary>
        /// <param name="viewModel"></param>
        public GenerateMacroCommand(AEMGViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        #endregion

        /// <summary>
        /// Events to Update CanExecute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determine if the macro can be generated or not
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            switch (_viewModel.SelectedMacro.Type)
            {
                case MacroItemType.EXP:
                    //Go button enable only when MaxMP and SkillMP are number and SkillMP is not a zero
                    if (int.TryParse(_viewModel.SkillMP, out int skillMPint)
                        && int.TryParse(_viewModel.MaxMP, out int maxMPint)
                        && _viewModel.SkillMP != "0")
                    return true;
                    else return false;
                case MacroItemType.AD:
                case MacroItemType.ADVH:
                    return true;
                default:
                    return false;
            }          
        }

        /// <summary>
        /// Generate the macro
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            //switch between EXP and AD
            switch (_viewModel.SelectedMacro.Type)
            {
                //If the macro is EXP type then do this
                case MacroItemType.EXP:
                    //Insert Exp data to template
                    Methods.InsertExpData(_viewModel.ExpIsChecked01, _viewModel.ExpIsChecked02, _viewModel.ExpIsChecked03, _viewModel.ExpIsChecked04);

                    //insert battle data into the template base on macro template and MaxBattle
                    Methods.InsertBattleData(_viewModel.SelectedMacro.TemplateName, string.Empty, _viewModel.MaxBattle);               
                    break;

                //If the macro is an AD then do this
                case MacroItemType.AD:
                case MacroItemType.ADVH:
                    //insert character's action in trash mob battle
                    Methods.InsertCharacterDataAD(_viewModel.Char01Ac, _viewModel.Char02Ac, _viewModel.Char03Ac, _viewModel.Char04Ac);
                    
                    //insert character's action in boss battle
                    Methods.InsertCharacterDataADBoss(_viewModel.BossTurnList, _viewModel.AFIsChecked);

                    //insert trashmob battle and boss battle data and delay to the template
                    Methods.InsertBattleData(_viewModel.SelectedMacro.TemplateName, _viewModel.DelayTextBox);
                    break;
            }

            //generate the macro
            Methods.GenerateMacro();

            //If insert record file fail then stop the command
            if (!Methods.AddToRecordFile(_viewModel.RecordFileLocation, _viewModel.OutputNameNox))
                return;

            //Convert to memu if checked and show complete message
            if (_viewModel.Memu720IsChecked == true) Methods.Convert2Memu(720);
            if (_viewModel.Memu1080IsChecked == true) Methods.Convert2Memu(1080);
            MessageBox.Show($"Generate {_viewModel.SelectedMacro.Name} Macro Done!", "AEMG", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
