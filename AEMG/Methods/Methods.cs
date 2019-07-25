using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace AEMG
{
    public static class Methods
    {
        internal static string awk = @"awk\awk.exe";
        internal static string AEMG = @"awk\AEMG.txt";
        internal static string Scripts = @"Scripts\";

        /// <summary>
        /// Start a CMD process from its aurgument
        /// </summary>
        /// <param name="agrs">the full argument</param>
        /// <returns></returns>
        internal static bool StartCMDProcess(string agrs)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + agrs);

            procStartInfo.RedirectStandardOutput = false;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            // wrap IDisposable into using (in order to release hProcess) 
            using (Process process = new Process())
            {
                process.StartInfo = procStartInfo;
                process.Start();

                // Wait until process does its work
                process.WaitForExit();

                // and only then read the result
                return ((process.ExitCode == 0)? true : false);
            }
        }

        /// <summary>
        /// Get a list of macro with it's name and template path from a txt file
        /// </summary>
        /// <param name="path">path to txt file hold macro</param>
        /// <returns></returns>
        internal static List<MacroItem> GetMacroList(string path )
        {
            //Create an empty list of macroItem
            List<MacroItem> macroList = new List<MacroItem>();

            //Open a stream to macroList
            StreamReader macroListTxt = new StreamReader(path);

            //Read the stream per 4 lines
            while (macroListTxt.EndOfStream == false)
            {
                var name = macroListTxt.ReadLine(); //read the name
                var template = macroListTxt.ReadLine(); //read the template name
                var description = macroListTxt.ReadLine(); //read the description name
                var type = macroListTxt.ReadLine(); //read the type of macro
                var delay = macroListTxt.ReadLine(); //read the delay value

                //add an macroItem to the list
                macroList.Add(new MacroItem {
                    Name = name,
                    TemplateName = template,
                    Description = description,
                    Type = (type == "EXP")? MacroItemType.EXP : (type == "AD")? MacroItemType.AD : MacroItemType.ADVH,
                    Delay = delay
                });
            }

            macroListTxt.Close();

            return macroList;
        }

        /// <summary>
        /// Methods to generate the Macro from user input
        /// </summary>
        /// <param name="macroItem">The Macro item</param>
        /// <returns></returns>
        internal static void GenerateMacro()
        {
            try
            {
                //Argument string
                string agstring;

                //Auto close streamreader
                using (StreamReader agStream = new StreamReader(@"awk\Ag.txt"))
                {
                    //Go through Ag.txt to execute all command
                    while (agStream.EndOfStream == false)
                    {
                        //read the argument from the file
                        agstring = agStream.ReadLine();
                        //execute the command from its argument
                        Methods.StartCMDProcess(agstring);
                    }
                }              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Convert Nox macro to memu base on res
        /// </summary>
        /// <param name="res">720 or 1080</param>
        internal static void Convert2Memu(int res)
        {
            switch (res)
            {
                case 720:
                    Methods.StartCMDProcess($"{awk} -f awk\\awk-Nox2Memu.txt Macro.txt > Macro-Memu-720.mir");
                    break;
                case 1080:
                    Methods.StartCMDProcess($"{awk} -f awk\\awk-Nox2Memu1080.txt Macro.txt > Macro-Memu-1080.mir");
                    break;
            }
        }

        /// <summary>
        /// Insert battle data and delay into macro template (main AEMG scripts)
        /// </summary>
        /// <param name="maxBattle">MAX battle from MAXMP and SKILLMP</param>
        /// <param name="templatePath">Name of the template</param>
        /// <param name="delay">The delay when waiting for horror</param>
        internal static void InsertBattleData(string templateName, string delay = "0", int maxBattle = 1, bool food = false, bool isBuy = false)
        {

            //If delay is not a number or that number smaller than 0 then set delay to default 0
            if (!int.TryParse(delay, out int tmp) || tmp < 0)
                delay = "0";

            Methods.StartCMDProcess($"{awk} -v MAX={maxBattle} -v DELAY={delay} -v food={food} -v isBuy={isBuy} -f {AEMG} Scripts\\{templateName} > tmp0.txt");
        }

        /// <summary>
        /// Insert trash mob battle data
        /// </summary>
        /// <param name="char1">character 1 action</param>
        /// <param name="char2">character 2 action</param>
        /// <param name="char3">character 3 action</param>
        /// <param name="char4">character 4 action</param>
        internal static void InsertCharacterDataAD(int char1, int char2, int char3, int char4, bool turn2 = false, string rl = "4")
        {
            //Create a new blank B-AD.txt file
            File.Create($"{Scripts}B-AD.txt").Close();

            //Open a stream to the newly created B-AD.txt
            StreamWriter AD = new StreamWriter($"{Scripts}B-AD.txt");

            //Insert RLRL to it if LeftRight textBox no input
            if (!(int.TryParse(rl, out int rlint)) || rlint <= 4)
                InsertTxt("RLRL.txt", AD);
            else
            {
                bool left = true;
                for (int i = 1; i <= rlint - 4; i++)
                {
                    //alternaing between left and right
                    if (left == true)
                    { InsertTxt("Left.txt", AD); left = false; }
                    else { InsertTxt("Right.txt", AD); left = true; }
                }
                if (rlint % 2 == 1)
                    InsertTxt("LRLR.txt", AD);
                else InsertTxt("RLRL.txt", AD);
            }
    
            //Insert char action
            if (char1 != 0)
                InsertCharacterAction(1, char1, AD);
            if (char2 != 0)
                InsertCharacterAction(2, char2, AD);
            if (char3 != 0)
                InsertCharacterAction(3, char3, AD);
            if (char4 != 0)
                InsertCharacterAction(4, char4, AD);

            //Insert turn 2
            if (turn2 == true)
                InsertTxt("atk.txt", AD);

            //Insert atk and battle won
            InsertTxt("battleWon.txt", AD);

            AD.Close();
        }

        /// <summary>
        /// Insert Character's action into boss battle data
        /// </summary>
        /// <param name="bossTurnList">List of actions</param>
        /// <param name="af">AF use or not</param>
        internal static void InsertCharacterDataADBoss(ObservableCollection<ObservableCollection<CharacterAction>> bossTurnList, bool af)
        {
            //Character's last actions
            List<int> lastCharAct = new List<int> { 0, 0, 0, 0 };

            int currentChar = 0;

            //Create a new blank B-AD.txt file
            File.Create($"{Scripts}B-AD-Boss.txt").Close();

            //Open a stream to the newly created B-AD-Boss.txt
            StreamWriter ADBoss = new StreamWriter($"{Scripts}B-AD-Boss.txt");

            //Insert character's action, do nothing if current action equal to last action
            foreach (var turn in bossTurnList)
            {
                foreach (var character in turn)
                {
                    //if current Character action different from last action then insert, otherwise do nothing 
                    if (character.CharAct != lastCharAct[currentChar])
                        InsertCharacterAction(character.CharPos, character.CharAct, ADBoss);
                    //Change last charaction to current action
                    lastCharAct[currentChar] = character.CharAct;
                    //change to next char
                    currentChar++;
                }
                //insert atk after each turn
                InsertTxt("atk.txt", ADBoss);
                //reset Char count
                currentChar = 0;
            }

            //Insert AF if it's checked
            if (af == true)
                InsertTxt("AF.txt", ADBoss);
           
            ADBoss.Close();
        }

        /// <summary>
        /// Edit the records file with output name the user input and copy the macro to the folder
        /// </summary>
        /// <param name="recordsPath">Path to the records file</param>
        /// <param name="outputName">OutputName text box</param>
        internal static bool AddToRecordFile(string recordsPath, string outputName)
        {
            //check if file existed, it not then create directory for it
            (new FileInfo(recordsPath)).Directory.Create();

            //create records file it not existed
            File.AppendAllText(recordsPath, string.Empty);

            //path to records template and record folder
            string recordsTemplate = $"{Scripts}record-template.txt";
            string recordsdir = Directory.GetParent(recordsPath).ToString();

            //original records file text as list
            List<string> recordsList;

            //read the records template
            string recordsTemplateTxt = File.ReadAllText(recordsTemplate);

            //edit record template, name = output name, time = epoch time
            recordsTemplateTxt = recordsTemplateTxt.Replace(@"NAMEHERE", $"{outputName}").Replace(@"TIMEHERE", $"{DateTimeOffset.Now.ToUnixTimeSeconds()}");

            //read record file to the list
            using (StreamReader sr = new StreamReader(recordsPath))
            {
                recordsList = File.ReadAllLines(recordsPath).ToList();
            }

            //if records file were empty then add { }
            if (recordsList.Count == 0)         
                recordsList.AddRange(new List<string> { "{\n", "}" });

            //check in the record already contain outputname to ask for overwrite, if not then simple continue
            switch (Methods.IsOverwrite(outputName, recordsList))
            {
                case "no":
                    return false;
                case "yes":
                    File.Copy("Macro.txt", $"{recordsdir}\\{outputName}", true);
                    return true;
                default:
                    break;
            }

            //remove the first line which contain "{"
            recordsList.RemoveAt(0);

            //insert the newly edited template
            File.WriteAllText(recordsPath, recordsTemplateTxt);
            File.AppendAllText(recordsPath, Environment.NewLine);

            //if recordLists only has 1 child then Append "}", otherwise "},"
            if (recordsList.Count == 1)
                File.AppendAllText(recordsPath, "\t}");
            else File.AppendAllText(recordsPath, "\t},");

            //Append the rest of record file
            File.AppendAllText(recordsPath, Environment.NewLine);
            File.AppendAllLines(recordsPath, recordsList);

            //Copy and rename the macro to record folder
            File.Copy("Macro.txt", $"{recordsdir}\\{outputName}", true);

            return true;
        }

        #region Helper Methods

        /// <summary>
        /// Insert character's action base on their position and action number
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="action"></param>
        internal static void InsertCharacterAction(int pos, int action, StreamWriter sw)
        {
            //if (action == 0)
            //    return;
            //insert character pos
            sw.WriteLine(File.ReadAllText($"{Scripts}u{pos}.txt"));

            //insert character action
            sw.WriteLine(File.ReadAllText($"{Scripts}a{action}.txt"));
        }

        /// <summary>
        /// Insert a txt file into another txt
        /// </summary>
        /// <param name="txt">name of Txt to insert</param>
        /// <param name="sw">Stream txt inserted to</param>
        internal static void InsertTxt(string txt, StreamWriter sw)
        {          
            sw.WriteLine(File.ReadAllText($"{Scripts}{txt}"));
        }

        /// <summary>
        /// Check for overwrite option. Return "no", "yes", "nomatch", empty
        /// </summary>
        /// <param name="name">name to check</param>
        /// <param name="list">list of string to check</param>
        /// <returns></returns>
        internal static string IsOverwrite(string name, List<string> list)
        {
            if (list.Count == 0)
                return string.Empty;

            foreach (var line in list)
            {
                //If it contain the name then ask user if they want to overwrite the file
                //If No then return false for no overwrite
                //If Yes then return yes to overwrite
                if (line.Contains(name) == true)
                {
                    if ((MessageBox.Show("The Macro is already exist. Do you want to overwrite it?",
                    "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Question)) == MessageBoxResult.No)
                        return "no";
                    else return "yes";
                }
            }

            return "nomatch";
        }

        /// <summary>
        /// Insert Exp data to template
        /// </summary>
        /// <param name="char01">Character 1 IsChecked</param>
        /// <param name="char02">Character 2 IsChecked</param>
        /// <param name="char03">Character 3 IsChecked</param>
        /// <param name="char04">Character 4 IsChecked</param>
        internal static void InsertExpData(bool char01, bool char02, bool char03, bool char04)
        {
            //Create a new blank B-3u2s.txt
            File.Create($"{Scripts}B-3u2s.txt").Close();
            //Open a stream to it
            StreamWriter sw = new StreamWriter($"{Scripts}B-3u2s.txt");
            
            //Insert left right
            InsertTxt("LRLR.txt", sw);

            //If Char is selected then insert its action, if all fours is not selected then 3rd char default
            if (char01 == true)
                InsertCharacterAction(1, 2, sw);
            if (char02 == true)
                InsertCharacterAction(2, 2, sw);
            if (char03 == true || (char01 == false && char02 == false && char03 == false && char04 == false))
                InsertCharacterAction(3, 2, sw);
            if (char04 == true)
                InsertCharacterAction(4, 2, sw);

            //Insert battleWon text
            InsertTxt("battleWon.txt", sw);

            //Close the stream
            sw.Close();
        }

        #endregion
    }
}
