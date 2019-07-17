using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AEMG.Properties;

namespace AEMG
{
    class AEMGViewModel : BaseViewModel
    {
        #region Ctor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AEMGViewModel()
        {
            //create a new list of turns/tabs
            _bossTurnList = new ObservableCollection<ObservableCollection<CharacterAction>>();
            //add a starting tab/turns
            AddTab();

            //Generate command
            GenerateMacroCommand = new GenerateMacroCommand(this);
            AddTurnCommand = new AddTurnCommand(this);
            RemoveTurnCommand = new RemoveTurnCommand(this);
            RecordFileOpenDialog = new RecordFileOpenDialog(this);

            ExpIsChecked03 = true;
        }

        #endregion

        #region Private field

        /// <summary>
        /// List of all Macro the user can choose from
        /// </summary>
        private ObservableCollection<MacroItem> _comboBoxItemList;

        /// <summary>
        /// The currently selected macro
        /// </summary>
        private MacroItem _selectedMacro;

        /// <summary>
        /// Default location of the record file
        /// </summary>
        private string _recordFileLocation = Settings.Default.RecordFileLocation;

        private string _outputNameNox = "Nox-macro";

        /// <summary>
        /// Max battles from MaxMP and SKillMP
        /// </summary>
        private int _maxBattle;

        /// <summary>
        /// List of the turn
        /// </summary>
        private ObservableCollection<ObservableCollection<CharacterAction>> _bossTurnList;

        /// <summary>
        /// Keep track of the current tab/turns
        /// </summary>
        private int _tab = 0;

        /// <summary>
        /// Text to show in description textblock
        /// </summary>
        private string _descriptionTextBlock;

        #endregion

        #region Public Properties

        /// <summary>
        /// List of all Macro in combobox
        /// </summary>
        public ObservableCollection<MacroItem> ComboBoxItemList
        {
            get
            {
                //get a list of all macro
                var children = Methods.GetMacroList(@"Scripts\macroList.txt");

                //add and convert them to macroList that bind to the combobox
                _comboBoxItemList = new ObservableCollection<MacroItem>(children.Select(f => new MacroItem
                {
                    Name = f.Name,
                    TemplateName = f.TemplateName,
                    Description = f.Description,
                    Type = f.Type,
                    Delay = f.Delay
                }));

                //set the first macro as the default selection
                if (_comboBoxItemList.Count > 0)
                    _selectedMacro = _comboBoxItemList[0];

                return _comboBoxItemList;
            }
        }

        /// <summary>
        /// The selected macro in combobox
        /// </summary>
        public MacroItem SelectedMacro
        {
            get { return _selectedMacro; }
            set { _selectedMacro = value; }
        }        

        /// <summary>
        /// Description textblock
        /// </summary>
        public string DescriptionTextBlock
        {
            get
            {
                _descriptionTextBlock = File.ReadAllText($"{Methods.Scripts}{SelectedMacro.Description}");
                return _descriptionTextBlock;
            }
            set { }
        }

        /// <summary>
        /// The path to record file of Nox
        /// </summary>
        public string RecordFileLocation
        {
            get
            {
                //If recordfilelocation in setting is null or empty then use default location
                if (_recordFileLocation == null || _recordFileLocation == string.Empty)
                {
                    _recordFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Nox\record\records");
                    return _recordFileLocation;
                }
                else return _recordFileLocation;
            }
            set { _recordFileLocation = Settings.Default.RecordFileLocation = value; Settings.Default.Save(); }
        }

        /// <summary>
        /// Name of macro appear in record folder and Nox Recorder
        /// </summary>
        public string OutputNameNox
        {
            get { return _outputNameNox; }
            set { _outputNameNox = value; }
        }

        public bool Memu720IsChecked { get; set; }

        public bool Memu1080IsChecked { get; set; }

        /// <summary>
        /// Max MP user input to the textbox
        /// </summary>
        public string MaxMP { get; set; }

        /// <summary>
        /// AOE skill MP the user input into the textbox
        /// </summary>
        public string SkillMP { get; set; }

        /// <summary>
        /// Max numbers of battle base on MaxMP and SkillMP
        /// </summary>
        public int MaxBattle
        {
            get
            {
               if (int.TryParse(MaxMP, out int MaxMPint) && int.TryParse(SkillMP, out int SkillMPint))
               {
                    _maxBattle = MaxMPint / SkillMPint;
               }
                return _maxBattle;
            }
        }

        /// <summary>
        /// Enable if selected macro is AD
        /// </summary>
        public bool IsEnabledAD
        {
            get { return (_selectedMacro.Type != MacroItemType.EXP)? true : false; }
        }

        /// <summary>
        /// Enable if selected macro is EXP
        /// </summary>
        public bool IsEnabledEXP
        {
            get { return (_selectedMacro.Type == MacroItemType.EXP) ? true : false; }
        }

        public bool ExpIsChecked01 { get; set; }
        public bool ExpIsChecked02 { get; set; }
        public bool ExpIsChecked03 { get; set; }
        public bool ExpIsChecked04 { get; set; }

        /// <summary>
        /// Character chosen action in trash mob battle
        /// </summary>
        public int Char01Ac { get; set; }
        public int Char02Ac { get; set; }
        public int Char03Ac { get; set; }
        public int Char04Ac { get; set; }

        /// <summary>
        /// Check if AF should be used or not
        /// </summary>
        public bool AFIsChecked { get; set; }

        /// <summary>
        /// Delay text box for user to input
        /// </summary>
        public string DelayTextBox
        {
            get
            {
                return SelectedMacro.Delay;
            }
            set
            {
                SelectedMacro.Delay = value;
            }
        }

        /// <summary>
        /// Turn number appears in Tab
        /// </summary>
        public string Tab { get { return $"Turn {_tab}"; } }

        /// <summary>
        /// List of turns/tabs bind to tab control
        /// </summary>
        public ObservableCollection<ObservableCollection<CharacterAction>> BossTurnList
        {
            get
            { return _bossTurnList; }          
        }

        #endregion

        #region Commands

        public ICommand GenerateMacroCommand { get; set; }
        public ICommand AddTurnCommand { get; set; }
        public ICommand RemoveTurnCommand { get; set; }
        public ICommand RecordFileOpenDialog { get; set; }

        #endregion

        #region Helper Methods       

        /// <summary>
        /// Methods to add a tab
        /// </summary>
        internal void AddTab()
        {
            //increase tab number by 1
            _tab++;

            //add an turn/tabs
            _bossTurnList.Add(new ObservableCollection<CharacterAction>());
            if (_tab == 1)
            {
                //Tab 1 with default action
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = 0, CharPos = 1 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = 0, CharPos = 2 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = 0, CharPos = 3 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = 0, CharPos = 4 });
            }
            else
            {
                //Tab 2 or more will take the action from the previous tab
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = _bossTurnList[_tab - 2][0].CharAct, CharPos = 1 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = _bossTurnList[_tab - 2][1].CharAct, CharPos = 2 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = _bossTurnList[_tab - 2][2].CharAct, CharPos = 3 });
                _bossTurnList[_tab - 1].Add(new CharacterAction { CharAct = _bossTurnList[_tab - 2][3].CharAct, CharPos = 4 });
            }
        }

        /// <summary>
        /// Methods to remove a tab
        /// </summary>
        internal void RemoveTab()
        {
            //Remove the last tab
            _bossTurnList.Remove(_bossTurnList.Last());

            //Decrease tab number
            _tab--;
        }

        #endregion
    }
}

        

    
