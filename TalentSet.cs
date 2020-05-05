using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// >>> NOT USED >> REPLACED BY PERKS >>> NOT USED >> REPLACED BY PERKS >>> NOT USED >> REPLACED BY PERKS
/// </summary>

namespace Dota2_LUA_Helper
{
    public class TalentSet
    {
        public Hero Hero { get; set; }

        public List<CommonTalent> CommonTalents { get; set; }
        public List<BasicTalent> BasicTalents { get; set; }
        public List<AdvancedTalent> AdvancedTalents { get; set; }
        public List<ExpertTalent> ExpertTalents { get; set; }


        public TalentSet() { }
        public TalentSet(Hero hero, List<List<Dictionary<string, string>>> talents)
        {
            CommonTalents = new List<CommonTalent>();
            BasicTalents = new List<BasicTalent>();
            AdvancedTalents = new List<AdvancedTalent>();
            ExpertTalents = new List<ExpertTalent>();

            Hero = hero;

            int i = 0;
            foreach (List<Dictionary<string, string>> list in talents)
            {
                // Common Talents
                if (i == 0)
                {
                    //CommonTalents = new CommonTalents(list);
                    int j = 0;
                    foreach (Dictionary<string, string> dict in list)
                    {
                        CommonTalents.Add(new CommonTalent(j, dict));
                        j++;
                    }
                }
                // Basic Talents
                if (i == 1)
                {
                    int j = 0;
                    foreach(Dictionary<string, string> dict in list)
                    {
                        BasicTalents.Add(new BasicTalent(j, dict));
                        j++;
                    }
                }
                // Advanced Talents
                if (i == 2)
                {
                    int j = 0;
                    foreach (Dictionary<string, string> dict in list)
                    {
                        AdvancedTalents.Add(new AdvancedTalent(j, dict));
                        j++;
                    }
                }
                // Expert Talents
                if (i == 3)
                {
                    int j = 0;
                    foreach (Dictionary<string, string> dict in list)
                    {
                        ExpertTalents.Add(new ExpertTalent(j, dict));
                        j++;
                    }
                }

                i++;
            }
        }
    }

    /*
    public class CommonTalents
    {
        public List<string> Talents { get; set; }

        public CommonTalents() { }
        public CommonTalents(List<Dictionary<string, string>> talents)
        {
            Talents = new List<string>();
            foreach (Dictionary<string, string> Dict in talents)
            {
                foreach (KeyValuePair<string, string> Pair in Dict)
                {
                    Talents.Add(Pair.Key);
                }
            }
        }
    }
    */

    public class CommonTalent
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> Bonuses { get; set; }

        public CommonTalent() { }
        public CommonTalent(int order, Dictionary<string, string> bonuses)
        {
            Order = order;

            string type = "";

            int i = 0;
            foreach (KeyValuePair<string, string> Pair in bonuses)
            {
                if (i != 0)
                    type += Pair.Key + "-";

                i++;
            }
            type = type.Remove(type.Length - 1);
            Type = type;

            Category = bonuses.First().Value;

            bonuses.Remove(bonuses.First().Key);

            Bonuses = bonuses;
        }
    }

    public class BasicTalent
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> Bonuses { get; set; }

        public BasicTalent() { }
        public BasicTalent(int order, Dictionary<string, string> bonuses)
        {
            Bonuses = new Dictionary<string, string>();

            Order = order;

            int i = 0;
            foreach (KeyValuePair<string, string> Pair in bonuses)
            {
                if (i == 0)
                {
                    Category = Pair.Value;
                }
                else if (i == 1)
                {
                    Type = Pair.Key;
                }
                else
                    Bonuses.Add(Pair.Key, Pair.Value);

                i++;
            }
        }
    }

    public class AdvancedTalent
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> Bonuses { get; set; }

        public AdvancedTalent() { }
        public AdvancedTalent(int order, Dictionary<string, string> bonuses)
        {
            Bonuses = new Dictionary<string, string>();

            Order = order;

            int i = 0;
            foreach (KeyValuePair<string, string> Pair in bonuses)
            {
                if (i == 0)
                {
                    Category = Pair.Value;
                }
                else if (i == 1)
                {
                    Type = Pair.Key;
                }
                else
                    Bonuses.Add(Pair.Key, Pair.Value);

                i++;
            }
        }
    }

    public class ExpertTalent
    {
        public int Order { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> Bonuses { get; set; }

        public ExpertTalent() { }
        public ExpertTalent(int order, Dictionary<string, string> bonuses)
        {
            Bonuses = new Dictionary<string, string>();

            Order = order;

            int i = 0;
            foreach (KeyValuePair<string, string> Pair in bonuses)
            {
                if (i == 0)
                {
                    Category = Pair.Value;
                }
                else if (i == 1)
                {
                    Type = Pair.Key;
                }
                else
                    Bonuses.Add(Pair.Key, Pair.Value);

                i++;
            }
        }
    }

}
