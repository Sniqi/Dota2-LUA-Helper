using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Dota2_LUA_Helper
{
    public class Perk
    {
        public string Codename { get; set; }
        public string ReadableName { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Bonuses { get; set; }

        public Perk() { }
        public Perk(List<string> content)
        {
            Codename = content[0];
            ReadableName = content[1];
            Type1 = content[2];
            Type2 = content[3];
            Description = content[4];

            Bonuses = new Dictionary<string, string>();

            for (int i = 5; i < content.Count - 1; i = i + 2)
            {
                Bonuses.Add(content[i], content[i + 1]);

                if ( Bonuses.Values.Last() == "" )
                {
                    Bonuses.Remove(Bonuses.Values.Last());
                }
            }


        }

        public string GetModifier(string selectedpath)
        {
            string file = selectedpath + "\\scripts\\vscripts\\encounters\\_OUTPUT\\defaultperk_modifier.lua";

            string contents = System.IO.File.ReadAllText(file);

            contents = Regex.Replace(contents, "%ModifierClass%", this.Codename);

            contents = Regex.Replace(contents, "%GetSpecialValueFor%", GetSpecialValues());

            return contents;
        }

        public string GetSpecialValues()
        {
            string bonuses = "\n";
            foreach (KeyValuePair<string, string> Bonus in this.Bonuses)
            {
                bonuses += GetSpecialValueFor(Bonus.Key, Bonus.Value);
                bonuses += "\n";
            }

            return bonuses;
        }


        private string GetSpecialValueFor(string modifierName, string modifierValue)
        {
            string str = "";
            str = "	self." + modifierName;
            for (int i = str.Length; i <= 35; i++)
            {
                str += " ";
            }
            str += "= " + modifierValue;

            return str;
        }

        public string GetTranslatedDescription()
        {
            string output = this.Description;
            int i = 1;
            foreach (KeyValuePair<string, string> Bonus in this.Bonuses)
            {
                string placeholder = "&" + i.ToString();
                output = output.Replace(placeholder, Bonus.Value);
                i++;
            }

            return output;
        }

    }
}
