using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WikiClientLibrary.Client;
using WikiClientLibrary.Sites;
using WikiClientLibrary;
using WikiClientLibrary.Pages;
using Renci.SshNet;

namespace Dota2_LUA_Helper
{
    public partial class Form1 : Form
    {
        private string selectedpath;
        private string selectedpathContent;
        private static readonly HttpClient HttpClient = new HttpClient();

        private string Encounters;
        private string ModifierTable;
        private string LesserPerks;
        private string GreaterPerks;

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        public Form1()
        {
            // Initilize Form
            InitializeComponent();

            // Define work directories
            selectedpath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dota 2 beta\\game\\dota_addons\\dungeoneer_test";
            selectedpathContent = selectedpath.Replace("game", "content");

            // Process credentials for Google
            UserCredential credential;
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            String spreadsheetId = "1Qx-nu0oRBEKKpIa5l2fSfFdfHu4ez78IgIGUJlr3h1o";

            // range is Tab name
            String range;

            // Read Encounters Stage 1
            range = "Encounters 1";
            GetEncountersFromGoogle(spreadsheetId, range, service);

            // Read Encounters Stage 2
            range = "Encounters 2";
            GetEncountersFromGoogle(spreadsheetId, range, service);

            // Read Encounters Stage 3
            range = "Encounters 3";
            GetEncountersFromGoogle(spreadsheetId, range, service);

            // Read Modifier Table
            range = "Modifier Table";
            GetModifierTableFromGoogle(spreadsheetId, range, service);

            // Read Glyphs
            range = "Glyphs";
            LesserPerks = GetPerksFromGoogle(spreadsheetId, range, service);

            // Read Artifacts
            range = "Artifacts";
            GreaterPerks = GetPerksFromGoogle(spreadsheetId, range, service);

            // 
            CreatePerks();

            // 
            CreateEncounter();

            // just exit after all is done
            System.Windows.Forms.Application.Exit();
            Environment.Exit(0);
        }

        /// <summary>
        /// Reads Encounters from Google Sheets and saves them to "Encounters" as tab and line seperated string.
        /// </summary>

        private void GetEncountersFromGoogle(String spreadsheetId, String range, SheetsService service)
        {
            // Define range, what to get. Here: a whole Tab
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();

            IList<IList<Object>> values = response.Values;

            // Check if response is not NULL
            if (values != null && values.Count > 0)
            {
                // read every row
                foreach (var row in values)
                {
                    string Column = "";

                    // read columns - can be expanded in sheets
                    int i = 0;
                    foreach (var column in row)
                    {
                        Column += column;

                        // seperate each column with tab
                        if (i < 21)
                            Column += "\t";

                        i++;
                    }

                    // fill empty columns with tab
                    for (int j = Column.Split('\t').Length; j < 21; j++)
                         Column+= "\t";

                    // seperate each row with new line
                    Encounters += Column + "\n";
                }
            }
        }

        /// <summary>
        /// Reads Modifiers from Google Sheets and saves them to "ModifierTable" as tab and line seperated string.
        /// </summary>

        private void GetModifierTableFromGoogle(String spreadsheetId, String range, SheetsService service)
        {
            // Define range, what to get. Here: a whole Tab
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();

            IList<IList<Object>> values = response.Values;

            // Check if response is not NULL
            if (values != null && values.Count > 0)
            {
                // number of Modfiers in this table
                int x = 61;

                // read every row
                foreach (var row in values)
                {
                    if (x >= 0)
                    {
                        string Column = "";

                        // read columns - can be expanded in sheets
                        int i = 0;
                        foreach (var column in row)
                        {
                            Column += column;

                            // seperate each column with tab
                            if (i < 2)
                                Column += "\t";

                            i++;
                        }

                        // fill empty columns with tab
                        for (int j = Column.Split('\t').Length; j < 2; j++)
                            Column += "\t";

                        // seperate each row with new line
                        ModifierTable += Column + "\n";
                    }
                    x--;
                }
            }
        }

        /// <summary>
        /// Reads Perks from Google Sheets and returns them as tab and line seperated string.
        /// </summary>

        private string GetPerksFromGoogle(String spreadsheetId, String range, SheetsService service)
        {
            string Perks = "";

            // Define range, what to get. Here: a whole Tab
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();

            IList<IList<Object>> values = response.Values;

            // Check if response is not NULL
            if (values != null && values.Count > 0)
            {
                int x = 0;

                // read every row
                foreach (var row in values)
                {
                    // skip header row
                    if (x != 0)
                    {
                        string Column = "";

                        // read columns - can be expanded in sheets
                        int i = 0;
                        foreach (var column in row)
                        {
                            Column += column;
                            Column += "\t";

                            i++;
                        }

                        // fill empty columns with tab
                        for (int j = Column.Split('\t').Length; j < 23; j++)
                            Column += "\t";

                        // seperate each row with new line
                        Perks += Column + "\n";
                    }
                    x++;
                }
            }

            return Perks;
        }

        /// <summary>
        /// Creates perks of class type Perk and then uses the objects to create auto generated content and links within lua
        /// </summary>

        private void CreatePerks()
        {
            // Convert string filled with perks to a List of type Perk
            List<Perk> Perks = new List<Perk>();

            // Convert Lesser perks
            foreach (string Line in LesserPerks.Split('\n'))
            {
                if (Line != "")
                {
                    List<string> line = Line.Split('\t').ToList();
                    Perk perk = new Perk(line);
                    Perks.Add(perk);
                }
            }

            // Convert Greater perks
            foreach (string Line in GreaterPerks.Split('\n'))
            {
                if (Line != "")
                {
                    List<string> line = Line.Split('\t').ToList();
                    Perk perk = new Perk(line);
                    Perks.Add(perk);
                }
            }

            // Create Compile Dummy File
            // this file serves as a dummy file filled with textures which are compiled when read by Dota
            string compileContents = "";
            compileContents += "<root>\n";
            compileContents += "  <snippets>\n";
            compileContents += "	<snippet name=\"Compile\">\n";

            // define and create work directory
            string output_perks = selectedpath + "\\scripts\\vscripts\\perks\\";
            create_folder(output_perks);

            // define vars
            string ModifierLinks = "";
            string ModifierTable = "";
            string ModifierDescriptionTable = "";
            string ModifierDescriptionAddonEnglish = "";
            string currentContents = "";
            string updatedContents = "";
            bool skip = false;

            // Process each perk
            foreach (Perk Perk in Perks)
            {
                //Create Modifier .lua Files
                string filename = output_perks + Perk.Codename + ".lua";
                if (File.Exists(filename))
                {
                    //Merge GetSpecialValues
                    currentContents = System.IO.File.ReadAllText(filename);

                    updatedContents = "";

                    skip = false;
                    foreach (string line in currentContents.Split('\n'))
                    {
                        if (line.Contains("-- ### VALUES START ### --") && skip == false)
                        {
                            skip = true;
                            updatedContents += line;
                            updatedContents += Perk.GetSpecialValues();
                        }
                        if (line.Contains("-- ### VALUES END ### --") && skip == true)
                        {
                            skip = false;
                        }
                        if (!skip)
                        {
                            updatedContents += line + "\n";
                        }
                    }

                    System.IO.File.WriteAllText(filename, updatedContents);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, Perk.GetModifier(selectedpath));
                }

                //Create Modifier Link list in gamemode.lua
                ModifierLinks += "LinkLuaModifier( '" + Perk.Codename + "', 'perks/" + Perk.Codename + "', LUA_MODIFIER_MOTION_NONE )\n";


                //Create Modifier table in Perks.lua
                ModifierTable += "	table.insert(PerkModifiers, \"" + Perk.Codename + "\")\n";

                //Add texture to Compile dummy file
                compileContents += "		<Image src=\"file://{images}/custom_game/perks/" + Perk.Codename + ".png\" />\n";

                //Create Title and Description in addon_english.txt
                string Title = "		\"" + Perk.Codename + "_Title\"";
                for (int i = 0; i < 55 - Perk.Codename.Length; i++)
                    Title += " ";
                Title += "\"" + Perk.ReadableName + "\"\n";
                string Description = "		\"" + Perk.Codename + "_Description\"";
                for (int i = 0; i < 49 - Perk.Codename.Length; i++)
                    Description += " ";
                Description += "\"" + Perk.GetTranslatedDescription() + "\"\n";

                ModifierDescriptionAddonEnglish += Title + Description;

                //Create dynamic Description table in Perks.lua
                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"] = {}\n";
                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"title\"] = \"" + Perk.ReadableName + "\"\n";
                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"descr\"] = \"" + Perk.GetTranslatedDescription() + "\"\n";
                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"type1\"] = \"" + Perk.Type1 + "\"\n";
                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"type2\"] = \"" + Perk.Type2 + "\"\n";

                ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"stats\"] = {}\n";

                int count = 1;
                foreach (KeyValuePair<string, string> bonus in Perk.Bonuses)
                {
                    if (bonus.Key.Contains("effectiveness_"))
                    {
                        ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"" + bonus.Key + "\"] = " + bonus.Value + "\n";
                    }
                    else
                    {
                        ModifierDescriptionTable += "	PerkModifiers_Descr[\"" + Perk.Codename + "\"][\"stats\"][" + count + "] = " + bonus.Value + "\n";
                    }
                    count++;
                }

            }

            //Create Modifier Link list in gamemode.lua
            currentContents = System.IO.File.ReadAllText(selectedpath + "\\scripts\\vscripts\\gamemode.lua");
            updatedContents = "";

            skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if (line.Contains("-- ### Perk Modifiers Start ### --") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += ModifierLinks;
                }
                if (line.Contains("-- ### Perk Modifiers End ### --") && skip == true)
                {
                    skip = false;
                }
                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            System.IO.File.WriteAllText(selectedpath + "\\scripts\\vscripts\\gamemode.lua", updatedContents);


            //Create Modifier table in Perks.lua
            currentContents = System.IO.File.ReadAllText(selectedpath + "\\scripts\\vscripts\\mechanics\\Perks.lua");
            updatedContents = "";

            skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if (line.Contains("-- ### Dynamic Perk Modifier Table Start ### --") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += ModifierTable;
                }
                if (line.Contains("-- ### Dynamic Perk Modifier Table End ### --") && skip == true)
                {
                    skip = false;
                }
                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            System.IO.File.WriteAllText(selectedpath + "\\scripts\\vscripts\\mechanics\\Perks.lua", updatedContents);


            //Create Perks Title/Description in addon_english.txt
            currentContents = System.IO.File.ReadAllText(selectedpath + "\\resource\\addon_english.txt");
            updatedContents = "";

            skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if (line.Contains("// PERKS //") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += ModifierDescriptionAddonEnglish;
                }
                if (line.Contains("// PERKS END //") && skip == true)
                {
                    skip = false;
                }
                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            System.IO.File.WriteAllText(selectedpath + "\\resource\\addon_english.txt", updatedContents, Encoding.Unicode);


            //Create Perks Title/Description in Perks.lua
            currentContents = System.IO.File.ReadAllText(selectedpath + "\\scripts\\vscripts\\mechanics\\Perks.lua");
            updatedContents = "";

            skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if(line.Contains("-- ### Dynamic Perk Modifier Description Table Start ### --") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += ModifierDescriptionTable;
                }
                if (line.Contains("-- ### Dynamic Perk Modifier Description Table End ### --") && skip == true)
                {
                    skip = false;
                }
                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            System.IO.File.WriteAllText(selectedpath + "\\scripts\\vscripts\\mechanics\\Perks.lua", updatedContents);


            // Create Compile Dummy File
            DirectoryInfo directory = new DirectoryInfo(selectedpathContent + "\\panorama\\images\\custom_game\\perks\\perk_types\\");
            FileInfo[] files = directory.GetFiles("*.png");
            foreach (FileInfo s in files)
            {
                compileContents += "		<Image src=\"file://{images}/custom_game/perks/perk_types/" + s.Name + "\" />\n";
            }

            directory = new DirectoryInfo(selectedpathContent + "\\panorama\\images\\custom_game\\misc\\");
            files = directory.GetFiles("*.png");
            foreach (FileInfo s in files)
            {
                compileContents += "		<Image src=\"file://{images}/custom_game/misc/" + s.Name + "\" />\n";
            }

            compileContents += "		<Image src=\"file://{images}/custom_game/tutorial/tutorial.png\" />\n";
            compileContents += "	</snippet>\n";
            compileContents += "  </snippets>\n";
            compileContents += "</root>\n";
            System.IO.File.WriteAllText(selectedpathContent + "\\panorama\\layout\\custom_game\\perks\\perksPicsPreload.xml", compileContents, Encoding.UTF8);

        }

        /// <summary>
        /// Creates encounters and their abilities of corresponding class type and then 
        /// uses the objects to create auto generated content and links within lua
        /// 
        /// Note that Encounters are also Heroes
        /// </summary>

        private void CreateEncounter()
        {
            List<List<List<string>>> Encounter_Details = new List<List<List<string>>>();
            List<List<List<string>>> Abilities_Details = new List<List<List<string>>>();

            // 
            int count = 0;
            int encounter_count = -1;
            foreach (string Line in Encounters.Split('\n'))
            {
                List<string> line = Line.Split('\t').ToList();

                // Count non-empty columns
                count = 0;
                foreach (string column in line)
                {
                    if (column != "")
                        count++;
                }

                if (count > 1)
                {
                    int count_a = 0;

                    List<string> encounterDetails = Line.Split('\t').ToList();

                    // We only use the first 2 columns for encounter details
                    encounterDetails.RemoveRange(2, encounterDetails.Count - 2);

                    count_a = 0;
                    foreach (string column in encounterDetails)
                    {
                        if (column != "")
                            count_a++;
                    }
                    if (count_a > 0)
                        Encounter_Details[encounter_count].Add(encounterDetails);


                    List<string> abilitiesDetails = Line.Split('\t').ToList();

                    // We do not use the first 3 columns for encounter abilities
                    if (abilitiesDetails.Count > 2)
                        abilitiesDetails.RemoveRange(0, 3);

                    count_a = 0;
                    foreach (string column in abilitiesDetails)
                    {
                        if (column != "")
                            count_a++;
                    }
                    if (count_a > 0)
                        Abilities_Details[encounter_count].Add(abilitiesDetails);

                }
                else if (count == 1)
                {
                    Encounter_Details.Add(new List<List<string>>());
                    Abilities_Details.Add(new List<List<string>>());

                    encounter_count++;

                    //line.RemoveRange(2, line.Count - 2);
                    Encounter_Details[encounter_count].Add(line);
                }

            }

            // Get Modifier List
            Dictionary<string, string[]> KnownModiferList = new Dictionary<string, string[]>();
            foreach (string modifier in ModifierTable.Split('\n'))
            {
                if (modifier != "")
                {
                    string[] Values = new string[2];
                    Values[0] = modifier.Split('\t')[1];
                    Values[1] = modifier.Split('\t')[2];
                    KnownModiferList.Add(modifier.Split('\t')[0], Values);
                }
            }

            // Create Hero Objects
            List<Hero> Encounter_Objects = new List<Hero>();

            count = 0;
            foreach (List<List<string>> Hero in Encounter_Details)
            {
                Encounter_Objects.Add(new Hero(Hero));
                count++;
            }

            // Create Ability Objects
            List<List<HeroAbility>> Abilities_Objects = new List<List<HeroAbility>>();

            count = -1;
            foreach (List<List<string>> Hero in Abilities_Details)
            {
                Abilities_Objects.Add(new List<HeroAbility>());
                count++;

                // Get up to 6 Abilities (can be expanded)
                for (int i = 0; i < 6; i++)
                {
                    List<List<string>> abilitylines = new List<List<string>>();

                    foreach (List<string> AbilityLine in Hero)
                    {
                        // An ability uses 3 columns
                        if (AbilityLine.Count > 2)
                            abilitylines.Add(AbilityLine.GetRange(0 + (i * 3), 3));
                    }

                    Abilities_Objects[count].Add(new HeroAbility(abilitylines, Encounter_Objects[count], KnownModiferList));
                }
            }

            // Add Abilities to Hero Objects
            count = 0;
            foreach (Hero Hero in Encounter_Objects)
            {
                Hero.AddAbilities(Abilities_Objects[count]);

                count++;
            }

            // Create Files for Heroes/Abilties
            string file;
            string output_encounters = selectedpath + "\\scripts\\npc\\encounters";
            create_folder(output_encounters);

            string output_abilities = selectedpath + "\\scripts\\npc\\abilities_encounters";
            create_folder(output_abilities);

            string output_abilities_detailed = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\encounter_abilities_detailed";
            create_folder(output_abilities_detailed);

            create_folder(selectedpath + "\\addon_english");
            string addonenglishFile = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\addon_english\\addon_english_encounters.txt";
            System.IO.File.WriteAllText(addonenglishFile, "");

            string encounterTooltips = "";

            for (int i = 0; i < Encounter_Objects.Count; i++)
            {
                // Create Hero File
                file = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\default_npc_dota_encounter.txt";

                Encounter_Objects[i].CreateEncounterFile(file, output_encounters);

                // Create Ability File for Hero
                file = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\default_encounter_ability.txt";

                string contents = System.IO.File.ReadAllText(file);
                string completeContents = "\"DOTAAbilities\"\n{\n";
                foreach (HeroAbility Ability in Abilities_Objects[i])
                {
                    if (Ability.Name != null && Ability.Name != "")
                    {
                        completeContents += Ability.CreateAbility(contents);
                        completeContents += "\n\n\n";

                        string output_abilities_detailed_per_encounter = output_abilities_detailed + "\\" + Encounter_Objects[i].CodeName;
                        create_folder(output_abilities_detailed_per_encounter);

                        System.IO.File.WriteAllText(output_abilities_detailed_per_encounter + "\\" + Ability.CodeName + ".lua", Ability.CreateAbilityDetailed(selectedpath));

                        List<List<string[]>> Contents = Ability.CreateModifierDetailed(selectedpath);
                        foreach (List<string[]> modifier in Contents)
                        {
                            foreach (string[] content in modifier)
                            {
                                System.IO.File.WriteAllText(output_abilities_detailed_per_encounter + "\\" + content[0] + ".lua", content[1]);
                            }
                        }
                    }
                }
                completeContents += "}";

                System.IO.File.WriteAllText(output_abilities + "\\" + Encounter_Objects[i].CodeName + "_abilities.txt", completeContents);

                //Create Encounter Tooltips
                encounterTooltips += CreateEncounterTooltips(Encounter_Objects[i]);

                // Create Ability Tooltips
                CreateAbilityTooltips(addonenglishFile, Encounter_Objects[i]);

                // Create Ability Modifers Tooltips
                CreateAbilityModifierTooltips(addonenglishFile, Encounter_Objects[i], Abilities_Objects[i], KnownModiferList);
            }

            // Merge addon_english.txt
            string newContents = System.IO.File.ReadAllText(addonenglishFile);
            string currentContents = System.IO.File.ReadAllText(selectedpath + "\\resource\\addon_english.txt");

            string updatedContents = "";

            bool skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if (line.Contains("// ENCOUNTERS ABILITIES //") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += "\n%REPLACE%\n";
                }
                if (line.Contains("// ENCOUNTERS ABILITIES END //") && skip == true)
                {
                    skip = false;
                }


                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            updatedContents = Regex.Replace(updatedContents, "%REPLACE%", newContents);
            System.IO.File.WriteAllText(selectedpath + "\\resource\\addon_english.txt", updatedContents, Encoding.Unicode);


            // Merge addon_english.txt
            currentContents = System.IO.File.ReadAllText(selectedpath + "\\resource\\addon_english.txt");

            updatedContents = "";

            skip = false;
            foreach (string line in currentContents.Split('\n'))
            {
                if (line.Contains("// ENCOUNTERS //") && skip == false)
                {
                    skip = true;
                    updatedContents += line + "\n";
                    updatedContents += "\n%REPLACE%\n";
                }
                if (line.Contains("// ENCOUNTERS END //") && skip == true)
                {
                    skip = false;
                }


                if (!skip)
                {
                    updatedContents += line + "\n";
                }
            }

            updatedContents = Regex.Replace(updatedContents, "%REPLACE%", encounterTooltips);
            System.IO.File.WriteAllText(selectedpath + "\\resource\\addon_english.txt", updatedContents, Encoding.Unicode);
        }

        /// <summary>
        /// Creates a folder
        /// </summary>
        /// <param name="path"></param>

        public void create_folder(string path)
        {
            try
            {
                if (Directory.Exists(path) || path == null || path == "")
                {
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Creates a file
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strText"></param>

        public void create_file(string strPath, string strText)
        {
            StreamWriter myWriter = System.IO.File.CreateText(strPath);
            myWriter.WriteLine(strText);
            myWriter.Close();
        }

        /// <summary>
        /// returns Encounter Tooltips for addon_english.txt
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>

        private string CreateEncounterTooltips(Hero hero)
        {
            string contents = "";

            string str = "		\"npc_dota_hero_" + hero.CodeName + "\"";
            for (int i = str.Length; i <= 61; i++)
            {
                str += " ";
            }
            str += "\"" + hero.Name + "\"\n";
            contents += str;

            str = "		\"npc_dota_hero_" + hero.CodeName + "_Description\"";
            for (int i = str.Length; i <= 61; i++)
            {
                str += " ";
            }
            str += "\"" + hero.Description + "\"\n";
            contents += str;

            contents += "\n";

            return contents;
        }

        /// <summary>
        /// Creates Encounter Ability Tooltips for addon_english.txt
        /// </summary>
        /// <param name="addonenglishFile"></param>
        /// <param name="hero"></param>

        private void CreateAbilityTooltips(string addonenglishFile, Hero hero)
        {
            string contents = System.IO.File.ReadAllText(addonenglishFile);

            foreach (HeroAbility Ability in hero.Abilities)
            {
                // Ability name
                string str = "		\"DOTA_Tooltip_ability_" + Ability.CodeName + "\"";
                for (int i = str.Length; i <= 91; i++)
                {
                    str += " ";
                }
                str += "\"" + Ability.Name + "\"\n";
                contents += str;

                // Ability description
                str = "		\"DOTA_Tooltip_ability_" + Ability.CodeName + "_Description\"";
                for (int i = str.Length; i <= 91; i++)
                {
                    str += " ";
                }
                str += "\"" + Ability.Description.Replace("\\n", "<br>") + "\"\n";
                contents += str;

                // process Key Values pairs
                foreach (KeyValuePair<string, string[]> Special in Ability.SpecialKeyValues)
                {
                    if (Special.Key != "damage_interval" && Special.Key != "damage_instances" &&
                        Special.Key != "heal_interval" && Special.Key != "heal_instances" )
                    {
                        str = "		\"DOTA_Tooltip_ability_" + Ability.CodeName + "_" + Special.Key + "\"";
                        for (int i = str.Length; i <= 91; i++)
                        {
                            str += " ";
                        }
                        
                        if (str.Contains("DAMAGE DURATION:"))
                            str = str.Replace("DAMAGE ", "");

                        if (Special.Key.ToUpper().Contains("PERCENTAGE") || Special.Key.ToUpper().StartsWith("DAMAGE"))
                            str += "\"%" + Special.Key.ToUpper().Replace("_PERCENTAGE", "").Replace("_", " ") + ":\"\n";
                        else
                            str += "\"" + Special.Key.ToUpper().Replace("_", " ") + ":\"\n";


                        contents += str;
                    }
                }

                // Range
                if (Ability.AbilityCastRange.Contains(" "))
                {
                    str = "		\"DOTA_Tooltip_ability_" + Ability.CodeName + "_AbilityCastRange\"";
                    for (int i = str.Length; i <= 91; i++)
                    {
                        str += " ";
                    }

                    str += "\"CAST RANGE:\"\n";

                    contents += str;
                }


                // AoERadius
                if (Ability.AoERadius.Contains(" "))
                {
                    str = "		\"DOTA_Tooltip_ability_" + Ability.CodeName + "_AoERadius\"";
                    for (int i = str.Length; i <= 91; i++)
                    {
                        str += " ";
                    }

                    str += "\"AOE RADIUS:\"\n";

                    contents += str;
                }

                
            }

            contents += "\n";

            System.IO.File.WriteAllText(addonenglishFile, contents, Encoding.Unicode);
        }

        /// <summary>
        /// >>>NOT USED>>>
        /// Create modifier Tooltips for addon_english.txt
        /// </summary>
        /// <param name="heroname"></param>
        /// <param name="outputpath"></param>

        private void create_modifier_text(string heroname, string outputpath)
        {

            DirectoryInfo d = new DirectoryInfo(@"C:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\dota_addons\dungeoneer\scripts\vscripts\encounters\" + heroname); //Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*_modifier.lua"); //Getting Text files
            string str = "";
            foreach (FileInfo file in Files)
            {
                str += "		\"DOTA_Tooltip_" + file.Name.Substring(0, file.Name.Length - 4) + "\"								  \"\"\n";
                str += "		\"DOTA_Tooltip_" + file.Name.Substring(0, file.Name.Length - 4) + "_Description\"					  \"\"\n\n";
            }

            d = new DirectoryInfo(@"C:\Program Files (x86)\Steam\steamapps\common\dota 2 beta\game\dota_addons\dungeoneer\scripts\vscripts\encounters\" + heroname + "\\talents"); //Assuming Test is your Folder
            Files = d.GetFiles("*_modifier.lua"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                str += "		\"DOTA_Tooltip_" + file.Name.Substring(0, file.Name.Length - 4) + "\"								  \"\"\n";
                str += "		\"DOTA_Tooltip_" + file.Name.Substring(0, file.Name.Length - 4) + "_Description\"					  \"\"\n\n";
            }

            System.IO.File.WriteAllText(outputpath + "\\" + heroname + "_modifer_text" + ".txt", str);

        }

        /// <summary>
        /// Create modifier Tooltips for addon_english.txt
        /// </summary>
        /// <param name="addonenglishFile"></param>
        /// <param name="hero"></param>
        /// <param name="abilities"></param>
        /// <param name="KnownModiferList"></param>

        private void CreateAbilityModifierTooltips(string addonenglishFile, Hero hero, List<HeroAbility> abilities, Dictionary<string, string[]> KnownModiferList)
        {
            string contents = System.IO.File.ReadAllText(addonenglishFile);

            foreach (HeroAbility Ability in abilities)
            {
                List<Dictionary<string, string>> tanslatedModifierTxt = Ability.Modifiers;

                foreach (Dictionary<string, string> Modifier in tanslatedModifierTxt)
                {
                    string str = "		\"DOTA_Tooltip_" + Modifier["NameTranslation"] + "\"";
                    for (int i = str.Length; i <= 91; i++)
                    {
                        str += " ";
                    }
                    str += "\"" + Modifier["NameReadable"] + "\"\n";
                    contents += str;

                    str = "		\"DOTA_Tooltip_" + Modifier["NameTranslation"] + "_Description\"";
                    for (int i = str.Length; i <= 91; i++)
                    {
                        str += " ";
                    }
                    str += "\"" + Modifier["Translation"] + "\"\n";
                    contents += str;
                }
            }

            contents += "\n";

            System.IO.File.WriteAllText(addonenglishFile, contents, Encoding.Unicode);
        }

        /// <summary>
        /// >>>NOT USED>>>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="workingdirectory"></param>

        public void FileUploadSFTP(string file, string workingdirectory)
        {
            var host = "dungeoneer-dota2.com";
            var port = 0;
            var username = "";
            var password = "";

            // path for file you want to upload
            var uploadFile = file;

            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    client.ChangeDirectory(workingdirectory);

                    using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                    {

                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                        client.UploadFile(fileStream, Path.GetFileName(uploadFile));
                    }
                }

            }
        }

    }
}
