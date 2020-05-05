using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace Dota2_LUA_Helper
{
    public class Hero
    {
        public string Name { get; set; }
        public string CodeName { get; set; }
        public string Description { get; set; }

        public string HeroID { get; set; }
        public string ReplacedHero { get; set; }

        public string Model { get; set; }
        public string ModelScale { get; set; }

        public string AbilityLayout { get; set; }

        public List<HeroAbility> Abilities = new List<HeroAbility>();

        public TalentSet Talents { get; set; }

        public string ArmorPhysical { get; set; }
        public string MagicalResistance { get; set; }

        public string AttackCapabilities { get; set; }
        public string AttackCapabilitiesOrig { get; set; }
        public string AttackDamageMin { get; set; }
        public string AttackDamageMax { get; set; }
        public string AttackRange { get; set; }
        public string AttackRate { get; set; }

        public string MovementSpeed { get; set; }
        public string MovementTurnRate { get; set; }

        public string StatusHealth { get; set; }
        public string StatusHealthRegen { get; set; }
        public string StatusMana { get; set; }
        public string StatusManaRegen { get; set; }

        public string AttributePrimary { get; set; }
        public string AttributeBaseStrength { get; set; }
        public string AttributeStrengthGain { get; set; }
        public string AttributeBaseIntelligence { get; set; }
        public string AttributeIntelligenceGain { get; set; }
        public string AttributeBaseAgility { get; set; }
        public string AttributeAgilityGain { get; set; }

        public string BountyXP { get; set; }
        public string BountyGoldMin { get; set; }
        public string BountyGoldMax { get; set; }

        public string ProjectileModel { get; set; }
        public string ProjectileSpeed { get; set; }
        public string SoundSet { get; set; }

        public Hero() { }
        public Hero(List<List<string>> initialList)
        {
            Initilize(initialList);
        }

        public void Initilize(List<List<string>> initialList)
        {
            Name = initialList[0][0];

            int i = 1;
            Description = initialList[i][1]; i++;

            CodeName = initialList[i][1]; i++;

            HeroID = initialList[i][1]; i++;
            ReplacedHero = initialList[i][1]; i++;

            Model = initialList[i][1]; i++;
            ModelScale = initialList[i][1]; i++;

            AbilityLayout = initialList[i][1]; i++;

            ArmorPhysical = initialList[i][1]; i++;
            MagicalResistance = initialList[i][1]; i++;

            AttackCapabilities = GetAttackCapabilities(initialList[i][1]);
            AttackCapabilitiesOrig = initialList[i][1]; i++;

            AttackDamageMin = initialList[i][1]; i++;
            AttackDamageMax = initialList[i][1]; i++;
            AttackRange = initialList[i][1]; i++;
            AttackRate = initialList[i][1]; i++;

            MovementSpeed = initialList[i][1]; i++;
            MovementTurnRate = initialList[i][1]; i++;

            StatusHealth = initialList[i][1]; i++;
            StatusHealthRegen = initialList[i][1]; i++;
            StatusMana = initialList[i][1]; i++;
            StatusManaRegen = initialList[i][1]; i++;

            AttributePrimary = initialList[i][1]; i++;
            AttributeBaseStrength = initialList[i][1]; i++;
            AttributeStrengthGain = initialList[i][1]; i++;
            AttributeBaseIntelligence = initialList[i][1]; i++;
            AttributeIntelligenceGain = initialList[i][1]; i++;
            AttributeBaseAgility = initialList[i][1]; i++;
            AttributeAgilityGain = initialList[i][1]; i++;

            BountyXP = initialList[i][1]; i++;
            BountyGoldMin = initialList[i][1]; i++;
            BountyGoldMax = initialList[i][1]; i++;

            if (initialList.Count > i)
            {
                if (initialList[i][1] != null)
                    ProjectileModel = initialList[i][1]; i++;

                if (initialList[i][1] != null)
                    ProjectileSpeed = initialList[i][1]; i++;

                if (initialList[i][1] != null)
                    SoundSet = initialList[i][1]; i++;
            }

        }

        public void AddAbilities(List<HeroAbility> abilities)
        {
            foreach (HeroAbility ability in abilities)
            {
                if (ability.Name != null && ability.Name != "")
                    Abilities.Add(ability);
            }
        }

        public string GetAttackCapabilities(string attackCapabilities)
        {
            if (attackCapabilities == "Melee")
            {
                return "DOTA_UNIT_CAP_MELEE_ATTACK";
            }
            else if (attackCapabilities == "Ranged")
            {
                return "DOTA_UNIT_CAP_RANGED_ATTACK";
            }
            else
            {
                return "DOTA_UNIT_CAP_NO_ATTACK";
            }
        }

        // Creates a custom KV Hero file from a template file
        public void CreateEncounterFile(string pathToFile, string outputPath)
        {
            string contents = File.ReadAllText(pathToFile);

            contents = Regex.Replace(contents, "%HeroID%", HeroID);

            contents = Regex.Replace(contents, "%npc_dota_hero_heroname%", "npc_dota_hero_" + CodeName);
            contents = Regex.Replace(contents, "%npc_dota_hero_overrideheroname%", ReplacedHero);

            contents = Regex.Replace(contents, "%Model%", Model);
            contents = Regex.Replace(contents, "%ModelScale%", ModelScale);

            contents = Regex.Replace(contents, "%AbilityLayout%", AbilityLayout);

            int i = 1;
            foreach (HeroAbility ability in Abilities)
            {
                contents = Regex.Replace(contents, "%Ability" + i + "%", ability.GetCodeName(CodeName));
                i++;
            }

            for (i = 1; i <= 23; i++)
            {
                contents = Regex.Replace(contents, "%Ability" + i + "%", "");
            }

            contents = Regex.Replace(contents, "%ArmorPhysical%", ArmorPhysical);
            contents = Regex.Replace(contents, "%MagicalResistance%", MagicalResistance);

            contents = Regex.Replace(contents, "%AttackCapabilities%", AttackCapabilities);
            contents = Regex.Replace(contents, "%AttackDamageMin%", AttackDamageMin);
            contents = Regex.Replace(contents, "%AttackDamageMax%", AttackDamageMax);
            contents = Regex.Replace(contents, "%AttackRange%", AttackRange);
            contents = Regex.Replace(contents, "%AttackRate%", AttackRate);

            contents = Regex.Replace(contents, "%MovementSpeed%", MovementSpeed);
            contents = Regex.Replace(contents, "%MovementTurnRate%", MovementTurnRate);

            contents = Regex.Replace(contents, "%StatusHealth%", StatusHealth);
            contents = Regex.Replace(contents, "%StatusHealthRegen%", StatusHealthRegen);
            contents = Regex.Replace(contents, "%StatusMana%", StatusMana);
            contents = Regex.Replace(contents, "%StatusManaRegen%", StatusManaRegen);

            contents = Regex.Replace(contents, "%AttributePrimary%", AttributePrimary);
            contents = Regex.Replace(contents, "%AttributeBaseStrength%", AttributeBaseStrength);
            contents = Regex.Replace(contents, "%AttributeStrengthGain%", AttributeStrengthGain);
            contents = Regex.Replace(contents, "%AttributeBaseIntelligence%", AttributeBaseIntelligence);
            contents = Regex.Replace(contents, "%AttributeIntelligenceGain%", AttributeIntelligenceGain);
            contents = Regex.Replace(contents, "%AttributeBaseAgility%", AttributeBaseAgility);
            contents = Regex.Replace(contents, "%AttributeAgilityGain%", AttributeAgilityGain);

            contents = Regex.Replace(contents, "%BountyXP%", BountyXP);
            contents = Regex.Replace(contents, "%BountyGoldMin%", BountyGoldMin);
            contents = Regex.Replace(contents, "%BountyGoldMax%", BountyGoldMax);

            // Optional Parameters

            if (ProjectileModel != null)
            {
                contents = Regex.Replace(contents, "%ProjectileModel_Block%", "");
                contents = Regex.Replace(contents, "%ProjectileModel%", ProjectileModel);
            }
            else
            {
                contents = Regex.Replace(contents, "%ProjectileModel_Block%", "//");
                contents = Regex.Replace(contents, "%ProjectileModel%", "");
            }

            if (ProjectileSpeed != null)
            {
                contents = Regex.Replace(contents, "%ProjectileSpeed_Block%", "");
                contents = Regex.Replace(contents, "%ProjectileSpeed%", ProjectileSpeed);
            }
            else
            {
                contents = Regex.Replace(contents, "%ProjectileSpeed_Block%", "//");
                contents = Regex.Replace(contents, "%ProjectileSpeed%", "");
            }

            if (SoundSet != null)
            {
                contents = Regex.Replace(contents, "%SoundSet_Block%", "");
                contents = Regex.Replace(contents, "%SoundSet%", SoundSet);
            }
            else
            {
                contents = Regex.Replace(contents, "%SoundSet_Block%", "//");
                contents = Regex.Replace(contents, "%SoundSet%", "");
            }

            // Write File

            File.WriteAllText(outputPath + "\\" + "npc_dota_hero_" + CodeName + ".txt", contents);
        }

    }
}
