using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Dota2_LUA_Helper
{
    public class HeroAbility
    {
        public Hero Hero { get; set; }

        public string Name { get; set; }
        public string CodeName { get; set; }
        public string CodeNameNoHero { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string MaxLevel { get; set; }
        public string RequiredLevel { get; set; }
        public string LevelsBetweenUpgrades { get; set; }
        public string AbilityBehavior { get; set; }
        public string AbilityTextureName { get; set; }
        public string AbilityCastPoint { get; set; }
        public string AbilityCooldown { get; set; }
        public string AbilityManaCost { get; set; }
        public string AbilityCastRange { get; set; }
        public string AbilityChannelTime { get; set; }
        public string AbilityCastAnimation { get; set; }
        public string AbilityCastAnimation_Speed { get; set; }
        public string AbilityEndAnimation { get; set; }
        public string AoERadius { get; set; }

        public string AbilityUnitTargetType { get; set; }
        public string AbilityUnitTargetTeam { get; set; }

        public string DamageType { get; private set; }
        public string AbilityBehaviorKV { get; private set; }
        public string AbilityBehaviorFile { get; private set; }

        public Dictionary<string, string[]> SpecialKeyValues { get; set; }
        public Dictionary<string, string> SpecialKeyValuesReadable { get; set; }
        public Dictionary<string, string[]> SpecialKeyValuesForModifiers { get; set; }
        public List<Dictionary<string, string>> Modifiers { get; set; }
        public Dictionary<string, string[]> KnownModiferList { get; set; }

        public HeroAbility() { }
        public HeroAbility(List<List<string>> initialList, Hero hero, Dictionary<string, string[]> knownModiferList)
        {
            Initilize(initialList, hero, knownModiferList);
        }

        public void Initilize(List<List<string>> initialList, Hero hero, Dictionary<string, string[]> knownModiferList)
        {
            Hero = hero;

            Name = initialList[0][1];
            Description = initialList[1][1];
            Type = initialList[2][1];
            MaxLevel = initialList[3][1];
            RequiredLevel = initialList[4][1];
            LevelsBetweenUpgrades = initialList[5][1];
            AbilityBehavior = initialList[6][1];
            AbilityTextureName = initialList[7][1];
            AbilityCastPoint = initialList[8][1];
            AbilityCooldown = initialList[9][1];
            AbilityManaCost = initialList[10][1];
            AbilityCastRange = initialList[11][1];
            AbilityChannelTime = initialList[12][1];

            if (initialList[13][1] != "" && initialList[13][1] != "-")
            {
                AbilityCastAnimation = initialList[13][1].Split(',')[0].Replace(" ", "");
                AbilityCastAnimation_Speed = initialList[13][1].Split(',')[1].Replace(" ", "");
            }
            else
            {
                AbilityCastAnimation = "";
                AbilityCastAnimation_Speed = "";
            }

            AbilityEndAnimation = initialList[14][1];
            AoERadius = initialList[15][1];

            DamageType = GetDamageType();
            AbilityBehaviorKV = GetAbilityBehavior(false);
            AbilityBehaviorFile = GetAbilityBehavior(true);

            SpecialKeyValues = new Dictionary<string, string[]>();
            SpecialKeyValuesForModifiers = new Dictionary<string, string[]>();
            Modifiers = new List<Dictionary<string, string>>();
            KnownModiferList = knownModiferList;

            int ModifierCount = -1;
            for (int i = 16; i < initialList.Count; i++)
            {
                string KEY = initialList[i][0];

                if (KEY != "")
                {
                    // Optional attributes
                    if (!KEY.Contains("modifier"))
                    {
                        string[] ValueArray = new string[3];
                        ValueArray[0] = initialList[i][1];
                        ValueArray[1] = initialList[i][2];

                        if (ValueArray[0] != "" && ValueArray[0] != "-")
                        {
                            if (KEY == "AbilityUnitTargetType")
                            {
                                AbilityUnitTargetType = ValueArray[0];
                            }
                            else if (KEY == "AbilityUnitTargetTeam")
                            {
                                AbilityUnitTargetTeam = ValueArray[0];
                            }
                            else if (KEY == "AbilityEndAnimation")
                            {
                                AbilityEndAnimation = ValueArray[0];
                            }
                            else
                            {
                                string key = KEY;
                                if (key.Contains(','))
                                {
                                    key = KEY.Split(',')[0] + "_" + KEY.Split(',')[1];
                                    ValueArray[2] = KEY.Split(',')[0];
                                }

                                SpecialKeyValues.Add(key, ValueArray);

                                if (ValueArray[1] != "")
                                {
                                    SpecialKeyValuesForModifiers.Add(key, ValueArray);
                                }
                            }
                        }

                    }
                    // Ability Modifiers
                    else
                    {
                        string VALUE = initialList[i][1];

                        Dictionary<string, string> newModifier = new Dictionary<string, string>();
                        newModifier.Add(KEY, VALUE);
                        
                        bool Existent = false;
                        foreach (Dictionary<string, string> Dict in Modifiers)
                        {
                            foreach (KeyValuePair<string, string> Pair in Dict)
                            {
                                string Substring = KEY.Substring(0, 9) + "_name";
                                if (Pair.Key.Contains(Substring))
                                {
                                    Existent = true;
                                }
                            }
                        }

                        if (!Existent)
                        {
                            Modifiers.Add(newModifier);
                            ModifierCount++;
                        }
                        else
                        {
                            Modifiers[ModifierCount].Add(KEY, VALUE);
                        }

                    }
                }


            }

            GetModifierTranslatedDescription();

            SpecialKeyValuesReadable = new Dictionary<string, string>();
            GetSpecialKeyValuesReadable();
        }

        public void GetSpecialKeyValuesReadable()
        {
            //string name = SpecialKeyValue.Key;
            //name = char.ToUpper(name[0]) + name.Substring(1);

            foreach (KeyValuePair<string, string[]> Special in SpecialKeyValues)
            {
                string str = "";
                if (Special.Key != "damage_interval" && Special.Key != "damage_instances")
                {
                    string value = Special.Value[0];

                    if (Special.Key.ToUpper().Contains("PERCENTAGE"))
                    {
                        str += Special.Key.ToUpper().Replace("_PERCENTAGE", "").Replace("_", " ") + ":";
                        value += "%";
                    }
                    else
                        str += Special.Key.ToUpper().Replace("_", " ");

                    if (str.Contains("DAMAGE DURATION:"))
                        str = str.Replace("DAMAGE ", "");

                    if (str.Contains("DURATION"))
                        value += "s";

                    SpecialKeyValuesReadable.Add(str, value);
                }
            }

        }

        public string GetCodeName(string HeroName)
        {
            CodeName = Name.ToLower();
            CodeName = CodeName.Replace(" ", "_");
            CodeName = CodeName.Replace("\'", "");
            CodeName = CodeName.Replace(":", "");
            CodeName = HeroName + "_" + CodeName;

            return CodeName;
        }

        public string GetCodeNameNoHero()
        {
            CodeNameNoHero = Name.ToLower();
            CodeNameNoHero = CodeNameNoHero.Replace(" ", "_");
            CodeNameNoHero = CodeNameNoHero.Replace("\'", "");
            CodeNameNoHero = CodeNameNoHero.Replace(":", "");

            return CodeNameNoHero;
        }

        public string GetDamageType()
        {
            if (Type.Contains("Physical"))
                return "DAMAGE_TYPE_PHYSICAL";
            else if (Type.Contains("Magical"))
                return "DAMAGE_TYPE_MAGICAL";
            else if (Type.Contains("Pure"))
                return "DAMAGE_TYPE_PURE";
            else
                return "";
        }

        public string GetAbilityBehavior()
        {
            return GetAbilityBehavior(false);
        }

        public string GetAbilityBehavior(bool ForFile)
        {
            string DOTA_ABILITY_BEHAVIOR = "";
            string AbilityBehaviorForFiles = "";

            if (AbilityBehavior.Contains("NoTarget"))
            {
                DOTA_ABILITY_BEHAVIOR = "DOTA_ABILITY_BEHAVIOR_NO_TARGET";
                AbilityBehaviorForFiles += "notarget";
            }
            else if (AbilityBehavior.Contains("TargetSystem"))
            {
                DOTA_ABILITY_BEHAVIOR = "DOTA_ABILITY_BEHAVIOR_NO_TARGET";
                AbilityBehaviorForFiles += "targetsystem";
            }
            else if (AbilityBehavior.Contains("Point"))
            {
                DOTA_ABILITY_BEHAVIOR = "DOTA_ABILITY_BEHAVIOR_POINT";
                AbilityBehaviorForFiles += "point";
            }
            else if (AbilityBehavior.Contains("Targeted"))
            {
                DOTA_ABILITY_BEHAVIOR = "DOTA_ABILITY_BEHAVIOR_UNIT_TARGET";
                AbilityBehaviorForFiles += "targeted";
            }

            if (AbilityBehavior.Contains("Instant"))
            {
                //DOTA_ABILITY_BEHAVIOR += " | DOTA_ABILITY_BEHAVIOR_DONT_CANCEL_MOVEMENT";
                AbilityBehaviorForFiles += "_instant";
            }
            if (AbilityBehavior.Contains("Channeled"))
            {
                DOTA_ABILITY_BEHAVIOR += " | DOTA_ABILITY_BEHAVIOR_CHANNELLED";
                AbilityBehaviorForFiles += "_channeled";
            }

            if (AbilityBehavior.Contains("AOE"))
            {
                DOTA_ABILITY_BEHAVIOR += " | DOTA_ABILITY_BEHAVIOR_AOE";
                //AbilityBehaviorForFiles += "_aoe";
            }

            if (AbilityBehavior.Contains("Toggle"))
            {
                DOTA_ABILITY_BEHAVIOR += " | DOTA_ABILITY_BEHAVIOR_TOGGLE";
                //AbilityBehaviorForFiles += "_aoe";
            }


            if (ForFile)
                return AbilityBehaviorForFiles;

            return DOTA_ABILITY_BEHAVIOR;
        }

        private List<Dictionary<string, string>> GetModifierTranslatedDescription()
        {
            List<Dictionary<string, string>> Return = new List<Dictionary<string, string>>();

            int i = -1;
            foreach (Dictionary<string, string> modifier in Modifiers)
            {
                foreach (KeyValuePair<string, string> ModifierPair in modifier)
                {
                    if (ModifierPair.Key.Contains("name"))
                    {
                        Return.Add(new Dictionary<string, string>());
                        i++;

                        Return[i].Add("NameReadable", ModifierPair.Value);

                        string nameTranslation = ModifierPair.Value.ToLower();
                        nameTranslation = nameTranslation.Replace(" ", "_");
                        nameTranslation = nameTranslation.Replace("\'", "");
                        nameTranslation = nameTranslation.Replace(":", "");
                        nameTranslation = nameTranslation.Replace("_-", "");
                        nameTranslation = nameTranslation + "_modifier";
                        nameTranslation = Hero.CodeName + "_" + nameTranslation;

                        Return[i].Add("NameTranslation", nameTranslation);
                    }

                    if (ModifierPair.Key.Contains("descr"))
                    {
                        string descr = ModifierPair.Value;
                        string descrReadable = descr;
                        string descrTranslation = descr.Replace("%", "%%");
                        
                        foreach (KeyValuePair<string, string[]> Pair in SpecialKeyValuesForModifiers)
                        {
                            // Description - Human Readable
                            descrReadable = descrReadable.Replace(Pair.Value[1], Pair.Value[0]);
                            descrReadable = descrReadable.Replace("-", "");

                            // Description - For Translation
                            string replacement = "";

                            string Key = Pair.Key;
                            if (Pair.Value[2] != null)
                            {
                                Key = Pair.Key.Replace(Pair.Value[2] + "_", "");
                            }

                            string VarType = "%d";

                            if (Pair.Value[0].Contains("."))
                            {
                                VarType = "%f";
                            }

                            if (KnownModiferList.Keys.Any(Key.Equals))
                            {
                                replacement = VarType + KnownModiferList[Key][0] + "%";
                            }
                            else
                            {
                                replacement = VarType + "MODIFIER_PROPERTY_TOOLTIP%";
                            }

                            descrTranslation = descrTranslation.Replace(Pair.Value[1], replacement);

                        }
                        Return[i].Add("Readable", descrReadable);
                        Return[i].Add("Translation", descrTranslation);
                        
                    }
                }

                // Modifer Type
                foreach (KeyValuePair<string, string[]> Pair in SpecialKeyValuesForModifiers)
                {
                    string Key = Pair.Key;
                    if (Pair.Value[2] != null)
                    {
                        Key = Pair.Key.Replace(Pair.Value[2] + "_", "");
                    }

                    if (Return[i].Keys.Contains("ModifierType"))
                    {
                        Return[i]["ModifierType"] += "," + Key;
                        Return[i]["ModifierSpecials"] += "," + Pair.Key;
                    }
                    else
                    {
                        Return[i].Add("ModifierType", Key);
                        Return[i].Add("ModifierSpecials", Key);
                    }
                }
            }

            Modifiers = Return;

            return Return;
        }

        // Creates a custom KV Ability file from a template file
        public string CreateAbility(string contents)
        {
            contents = Regex.Replace(contents, "%AbilityName%", CodeName);
            contents = Regex.Replace(contents, "%AbilityBehavior%", AbilityBehaviorKV);

            contents = ReplaceWithBlock(AbilityUnitTargetType, "AbilityUnitTargetType", contents);

            contents = ReplaceWithBlock(AbilityUnitTargetTeam, "AbilityUnitTargetTeam", contents);

            contents = ReplaceWithBlock(DamageType, "AbilityUnitDamageType", contents);

            contents = Regex.Replace(contents, "%AbilityTextureName%", AbilityTextureName);

            contents = Regex.Replace(contents, "%HeroName%", Hero.CodeName);
            contents = Regex.Replace(contents, "%AbilityName%", CodeName);

            contents = Regex.Replace(contents, "%MaxLevel%", MaxLevel);
            contents = Regex.Replace(contents, "%RequiredLevel%", RequiredLevel);
            contents = Regex.Replace(contents, "%LevelsBetweenUpgrades%", LevelsBetweenUpgrades);

            contents = Regex.Replace(contents, "%AbilityCastPoint%", AbilityCastPoint);
            contents = Regex.Replace(contents, "%AbilityCooldown%", AbilityCooldown);
            contents = Regex.Replace(contents, "%AbilityManaCost%", AbilityManaCost);

            contents = ReplaceWithBlock(AbilityCastRange, "AbilityCastRange", contents);

            contents = ReplaceWithBlock(AbilityChannelTime, "AbilityChannelTime", contents);

            contents = ReplaceWithBlock(AbilityCastAnimation, "AbilityCastAnimation", contents);

            contents = Regex.Replace(contents, "%AbilityCastAnimation_Speed%", AbilityCastAnimation_Speed);
 
            contents = ReplaceWithBlock(AoERadius, "AoERadius", contents);

            string Specials = "";

            int index = 1;

            if (AbilityCastRange != null && AbilityCastRange != "" && AbilityCastRange != "-")
            {
                Specials += CreateSpecial("AbilityCastRange", AbilityCastRange, index);
                index++;
            }

            if (AoERadius != null && AoERadius != "" && AoERadius != "-")
            {
                Specials += CreateSpecial("AoERadius", AoERadius, index);
                index++;
            }

            foreach (KeyValuePair<string, string[]> Special in SpecialKeyValues)
            {
                Specials += CreateSpecial(Special.Key, Special.Value[0],  index);
                index++;
            }
            contents = Regex.Replace(contents, "%AbilitySpecial%", Specials);


            return contents;
        }

        private string CreateSpecial(string key, string value, int index)
        {
            string Return = "";
            string vartype = "";

            if (value.Contains('.'))
                vartype = "FIELD_FLOAT";
            else
                vartype = "FIELD_INTEGER";

            if (index < 10)
                Return += "        \"0" + index.ToString() + "\"\n";
            else
                Return += "        \"" + index.ToString() + "\"\n";

            Return += "        {\n";
            Return += "            \"var_type\"   \"" + vartype + "\"\n";
            Return += "            \"" + key + "\"   \"" + value + "\"\n";
            Return += "        }\n";

            return Return;
        }

        private string ReplaceWithBlock(string ReplaceVar, string Key, string contents)
        {
            if (ReplaceVar != "" && ReplaceVar != "-" && ReplaceVar != null)
            {
                contents = Regex.Replace(contents, "%" + Key + "%", ReplaceVar);
                contents = Regex.Replace(contents, "%" + Key + "_Block%", "");
            }
            else
            {
                contents = Regex.Replace(contents, "%" + Key + "%", "");
                contents = Regex.Replace(contents, "%" + Key + "_Block%", "//");
            }

            return contents;
        }


        // Creates a custom lua Ability file from a template file
        public string CreateAbilityDetailed(string selectedpath)
        {
            string file = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\defaultability.lua";
            string contents = System.IO.File.ReadAllText(file);

            contents = Regex.Replace(contents, "%AbilityClass%", CodeName);

            contents = Regex.Replace(contents, "%LinkLuaModifier%", LinkLuaModifier());

            contents = Regex.Replace(contents, "%Target%", Target());

            contents = Regex.Replace(contents, "%Point%", Point());

            contents = Regex.Replace(contents, "%SpecialValues%", SpecialValues());

            contents = Regex.Replace(contents, "%EndAnimation%", EndAnimation());

            contents = Regex.Replace(contents, "%GetVictims%", GetVictims());

            contents = Regex.Replace(contents, "%Foreach%", Foreach());

            contents = Regex.Replace(contents, "%Modifier%", Modifier());

            contents = Regex.Replace(contents, "%VictimSound%", VictimSound());

            contents = Regex.Replace(contents, "%Particles%", Particles());

            contents = Regex.Replace(contents, "%ApplyDamage%", ApplyDamage());

            contents = Regex.Replace(contents, "%ApplyHeal%", ApplyHeal());

            contents = Regex.Replace(contents, "%ForeachEnd%", ForeachEnd());

            contents = Regex.Replace(contents, "%GetAOERadius%", GetAOERadius());

            contents = Regex.Replace(contents, "%CheckDistanceToTarget%", CheckDistanceToTarget());

            return contents;
        }

        private string LinkLuaModifier()
        {
            string Return = "";

            if (Modifiers.Count > 0)
                Return += "\n";

            foreach (Dictionary<string, string> Modifier in Modifiers)
            {
                string modifierName = Modifier["NameTranslation"];
                Return += "\nLinkLuaModifier( \'" + modifierName + "\', \'encounters/" + Hero.CodeName + "/" + modifierName + "\', LUA_MODIFIER_MOTION_NONE )";
            }

            return Return;
        }

        private string Target()
        {
            string Return = "";

            if (AbilityBehavior.Contains("NoTarget"))
            {
                Return += "\n	local victim		= caster";
                Return += "\n	local victim_loc	= victim:GetAbsOrigin()";
            }
            else if (AbilityBehavior.Contains("Targeted"))
            {
                Return += "\n	local victim		= self:GetCursorTarget()";
                Return += "\n	local victim_loc	= victim:GetAbsOrigin()";
            }
            else if (AbilityBehavior.Contains("TargetSystem"))
            {
                Return += "\n	local victim		= GameRules.PLAYER_HEROES_TARGET[playerID+1]";
                Return += "\n	local victim_loc	= victim:GetAbsOrigin()";
            }

            return Return;
        }

        private string Point()
        {
            string Return = "";

            if (AbilityBehavior.Contains("Point"))
            {
                Return += "\n	local point			= self:GetCursorPosition()";
            }

            return Return;
        }

        private string SpecialValues()
        {
            string Return = "";
            string str = "";

            if (AbilityCastRange != null && AbilityCastRange != "" && AbilityCastRange != "-")
            {
                str = "\n	local " + "AbilityCastRange";
                for (int i = str.Length; i <= 35; i++)
                {
                    str += " ";
                }
                str += "= self:GetSpecialValueFor(\"" + "AbilityCastRange" + "\")";
                Return += str;
            }

            if (AoERadius != null && AoERadius != "" && AoERadius != "-")
            {
                str = "\n	local " + "AoERadius";
                for (int i = str.Length; i <= 35; i++)
                {
                    str += " ";
                }
                str += "= self:GetSpecialValueFor(\"" + "AoERadius" + "\")";
                Return += str;
            }

            foreach (KeyValuePair<string, string[]> Pair in SpecialKeyValues)
            {
                str = "\n	local " + Pair.Key;
                for (int i = str.Length; i <= 35; i++)
                {
                    str += " ";
                }
                str += "= self:GetSpecialValueFor(\"" + Pair.Key + "\")";
                Return += str;
            }

            return Return;
        }

        private string EndAnimation()
        {
            string Return = "";

            if (AbilityEndAnimation != "" && AbilityEndAnimation != "-" && AbilityEndAnimation != null)
            {
                Return += "\n";
                Return += "\n	-- Animation --";
                Return += "\n	caster:StartGestureWithPlaybackRate(" + AbilityEndAnimation + ")";
            }

            return Return;
        }

        private string GetVictims()
        {
            string Return = "";

            if (AbilityBehavior.Contains("AOE"))
            {
                Return += "\n	-- DOTA_UNIT_TARGET_TEAM_FRIENDLY; DOTA_UNIT_TARGET_TEAM_ENEMY; DOTA_UNIT_TARGET_TEAM_BOTH";
                Return += "\n	local units	= FindUnitsInRadius(team, point, nil, AoERadius, DOTA_UNIT_TARGET_TEAM_FRIENDLY, DOTA_UNIT_TARGET_HERO + DOTA_UNIT_TARGET_BASIC, DOTA_UNIT_TARGET_FLAG_NONE, FIND_ANY_ORDER, false)";
            }
            else if (AoERadius != "" && AoERadius != "-")
            {
                Return += "\n	-- DOTA_UNIT_TARGET_TEAM_FRIENDLY; DOTA_UNIT_TARGET_TEAM_ENEMY; DOTA_UNIT_TARGET_TEAM_BOTH";
                Return += "\n	local units	= FindUnitsInRadius(team, caster_loc, nil, AoERadius, DOTA_UNIT_TARGET_TEAM_FRIENDLY, DOTA_UNIT_TARGET_HERO + DOTA_UNIT_TARGET_BASIC, DOTA_UNIT_TARGET_FLAG_NONE, FIND_ANY_ORDER, false)";
            }

            return Return;
        }

        private string Foreach()
        {
            string Return = "";

            if (GetVictims() != "")
            {
                Return += "\n	local particle = {}";
                Return += "\n	for _,victim in pairs(units) do";
            }

            return Return;
        }

        private string Modifier()
        {
            string Return = "";
            string Tabs = "";

            if (LinkLuaModifier() != "")
            {
                if (GetVictims() != "")
                    Tabs = "		";
                else
                    Tabs = "	";

                foreach (Dictionary<string, string> Modifier in Modifiers)
                {
                    string modifierName = Modifier["NameTranslation"];
                    Return += "\n" + Tabs + "-- Modifier --";
                    Return += "\n" + Tabs + "victim:AddNewModifier(caster, self, \"" + modifierName + "\", {duration = duration})";
                }

            }

            return Return;
        }

        private string VictimSound()
        {
            string Return = "";
            string Tabs = "";
            if (GetVictims() != "")
                Tabs = "		";
            else
                Tabs = "	";


            Return += "\n" + Tabs + "-- Sound --";
            Return += "\n" + Tabs + "victim:EmitSound(\"\")";

            return Return;
        }

        private string Particles()
        {
            string Return = "";

            if (GetVictims() != "")
            {
                Return += "\n";
                Return += "\n		-- Particle --";
                Return += "\n		particle[_] = ParticleManager:CreateParticle(\"\", PATTACH_ABSORIGIN, victim)";
                Return += "\n		ParticleManager:SetParticleControl( particle[_], 0, victim:GetAbsOrigin() )";
                Return += "\n		ParticleManager:ReleaseParticleIndex( particle[_] )";
            }
            else
            {
                Return += "\n";
                Return += "\n	-- Particle --";
                Return += "\n	local particle = ParticleManager:CreateParticle(\"\", PATTACH_ABSORIGIN, victim)";
                Return += "\n	ParticleManager:SetParticleControl( particle, 0, victim:GetAbsOrigin() )";
                Return += "\n	ParticleManager:ReleaseParticleIndex( particle )";
            }

            return Return;
        }

        private string ApplyDamage()
        {
            string Return = "";
            string Tabs = "";
            if (GetVictims() != "")
                Tabs = "		";
            else
                Tabs = "	";

            if (SpecialKeyValues.ContainsKey("damage"))
            //if (GetVictims() != "")
            {
                Return += "\n";
                Return += "\n" + Tabs + "-- Apply Damage --";

                if (SpecialKeyValues.ContainsKey("damage_duration") && SpecialKeyValues.ContainsKey("damage_interval") &&
                        SpecialKeyValues.ContainsKey("damage_instances"))
                {
                    if (SpecialKeyValues["damage_duration"][0] != "" && SpecialKeyValues["damage_duration"][0] != "-" &&
                        SpecialKeyValues["damage_interval"][0] != "" && SpecialKeyValues["damage_interval"][0] != "-" &&
                        SpecialKeyValues["damage_instances"][0] != "" && SpecialKeyValues["damage_instances"][0] != "-"
                        )
                    {
                        Return += "\n" + Tabs + "EncounterApplyDamage(victim, caster, self, damage/damage_instances, " + DamageType + ", DOTA_DAMAGE_FLAG_NONE)";
                    }

                }
                else
                {
                    Return += "\n" + Tabs + "EncounterApplyDamage(victim, caster, self, damage, " + DamageType + ", DOTA_DAMAGE_FLAG_NONE)";
                }
            }



            return Return;
        }

        private string ApplyHeal()
        {
            string Return = "";
            string Tabs = "";
            if (GetVictims() != "")
                Tabs = "		";
            else
                Tabs = "	";

            if (SpecialKeyValues.ContainsKey("heal"))
            //if (GetVictims() != "")
            {
                Return += "\n";
                Return += "\n" + Tabs + "-- Apply Heal --";

                if (SpecialKeyValues.ContainsKey("heal_duration") && SpecialKeyValues.ContainsKey("heal_interval") &&
                        SpecialKeyValues.ContainsKey("heal_instances"))
                {
                    if (SpecialKeyValues["heal_duration"][0] != "" && SpecialKeyValues["heal_duration"][0] != "-" &&
                        SpecialKeyValues["heal_interval"][0] != "" && SpecialKeyValues["heal_interval"][0] != "-" &&
                        SpecialKeyValues["heal_instances"][0] != "" && SpecialKeyValues["heal_instances"][0] != "-"
                        )
                    {
                        Return += "\n" + Tabs + "ApplyDamageOrHeal(\"heal\", victim, caster, playerID, self, heal/heal_instances, nil, true, 0.0, heal_interval, heal_duration)";
                    }

                }
                else
                {
                    Return += "\n" + Tabs + "ApplyDamageOrHeal(\"heal\", victim, caster, playerID, self, heal, nil, true, nil, nil, nil)";
                }
            }



            return Return;
        }

        private string ForeachEnd()
        {
            string Return = "";

            if (GetVictims() != "")
            {
                Return += "\n	end";
            }

            return Return;
        }

        private string GetAOERadius()
        {
            string Return = "";

            if (AbilityBehavior.Contains("AOE"))
            {
                Return += "\n";
                Return += "\nfunction " + CodeName + ":GetAOERadius()";
                Return += "\n	return self:GetSpecialValueFor( \"AoERadius\" )";
                Return += "\nend";
            }

            return Return;
        }

        private string CheckDistanceToTarget()
        {
            string Return = "";

            if (AbilityBehavior.Contains("TargetSystem") && AbilityCastRange != "" && AbilityCastRange != "-")
            {
                Return += "\n	local AbilityCastRange = self:GetSpecialValueFor(\"AbilityCastRange\")";
                Return += "\n";
                Return += "\n	-- Check Range --";
                Return += "\n	local timer1 = Timers:CreateTimer(0, function()";
                Return += "\n		local result = CheckDistanceToTarget(playerID, player, caster, AbilityCastRange,  \"" + CodeName + "\")";
                Return += "\n		if result then return 0.02";
                Return += "\n		else caster:Stop() return false end";
                Return += "\n	end)";
                Return += "\n	Timers:CreateTimer(self:GetCastPoint()-0.05, function()";
                Return += "\n		Timers:RemoveTimer(timer1)";
                Return += "\n	end)";
            }

            return Return;
        }

        

        public List<List<string[]>> CreateModifierDetailed(string selectedpath)
        {
            List<List<string[]>> Contents = new List<List<string[]>>();

            string file = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\defaultability_modifier.lua";

            int i = 0;
            foreach (Dictionary<string, string> Modifier in Modifiers)
            {
                string[] ContentsArray = new string[2];

                Contents.Add(new List<string[]>());

                string contents = System.IO.File.ReadAllText(file);

                contents = Regex.Replace(contents, "%GetSpecialValueFor%", GetSpecialValueFor(Modifier));

                contents = Regex.Replace(contents, "%DeclareFunctions%", DeclareFunctions(Modifier));

                contents = Regex.Replace(contents, "%GetModifier%", GetModifier(Modifier));


                contents = Regex.Replace(contents, "%ModifierClass%", Modifier["NameTranslation"]);

                ContentsArray[0] = Modifier["NameTranslation"];
                ContentsArray[1] = contents;

                Contents[i].Add(ContentsArray);
                i++;
            }

            return Contents;
        }

        private string GetSpecialValueFor(Dictionary<string, string> Modifier)
        {
            string Return = "";

            foreach (string ModifierType in Modifier["ModifierSpecials"].Split(',').ToList())
            {
                string str = "";
                str = "\n	self." + ModifierType;
                for (int i = str.Length; i <= 35; i++)
                {
                    str += " ";
                }
                str += "= self:GetAbility():GetSpecialValueFor(\"" + ModifierType + "\")";
                Return += str;
            }

            return Return;
        }

        private string DeclareFunctions(Dictionary<string, string> Modifier)
        {
            string Return = "";

            foreach (string ModifierType in Modifier["ModifierType"].Split(',').ToList())
            {
                string replacement = "";
                if (KnownModiferList.Keys.Contains(ModifierType))
                {
                    replacement = KnownModiferList[ModifierType][0];
                }
                else
                {
                    replacement = "MODIFIER_PROPERTY_TOOLTIP";
                }

                string str = "";
                str += "\n		" + replacement + ",";
                Return += str;
            }

            return Return;
        }

        private string GetModifier(Dictionary<string, string> Modifier)
        {
            string Return = "";

            foreach (string ModifierType in Modifier["ModifierSpecials"].Split(',').ToList())
            {
                string replacement = "";
                if (KnownModiferList.Keys.Contains(ModifierType))
                {
                    replacement = KnownModiferList[ModifierType][1];
                }
                else
                {
                    replacement = "OnTooltip";
                }

                string str = "";
                str += "\nfunction %ModifierClass%:" + replacement + "( params )";
                str += "\n	return self." + ModifierType;
                str += "\nend";
                str += "\n";
                Return += str;
            }

            return Return;
        }


    }
}
