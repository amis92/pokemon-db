using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseDownloader
{
    class PokemonReader : BulbaReader<PokemonReader, PokemonReader.Factory>
    {

        public string EvoBox
        {
            get;
            private set;
        }

        public int NationalPokedexNumber
        {
            get {
                return Convert.ToInt32(NationalPokedexString);
            }
        }

        public string NationalPokedexString
        {
            get
            {
                return getMatch(@"ndex=(\d+)\s*\|");
            }
        }

        public string Name
        {
            get
            {
                return getMatch(@"name=(.+)\s*\|");
            }
        }

        public int GenerationIntroduced
        {
            get
            {
                int id = NationalPokedexNumber;
                if (id > 719)
                    return 7;
                if (id > 649)
                    return 6;
                if (id > 493)
                    return 5;
                if (id > 386)
                    return 4;
                if (id > 251)
                    return 3;
                if (id > 151)
                    return 2;
                return 1;
            }
        }

        public string Weight
        {
            get
            {
                return getMatch(@"weight-kg=(.+)\s*\|");
            }
        }

        public string Height
        {
            get
            {
                return getMatch(@"height-m=(.+)\s*\|");
            }
        }

        public string Type1
        {
            get
            {
                return getMatch(@"type1=(.+)\s*\|");
            }
        }

        public string Type2
        {
            get
            {
                return getMatch(@"type2=(.+)\s*\|");
            }
        }

        public string Ability1
        {
            get
            {
                return getMatch(@"ability1=(.+)\s*\|");
            }
        }

        public string Ability2
        {
            get
            {
                return getMatch(@"ability2=(.+)\s*\|");
            }
        }

        public string AbilityHidden
        {
            get
            {
                return getMatch(@"abilityd=(.+)\s*\|");
            }
        }

        public string PreviousEvolutionNationalIdString
        {
            get
            {
                if (EvoBox == null)
                {
                    return "-2";
                }
                string evoNumberString = getMatch(@"no(\d)=" + NationalPokedexString, EvoBox);
                if (evoNumberString == null)
                {
                    evoNumberString = getMatch(@"sprite(\d).?=" + NationalPokedexString, EvoBox);
                    if (evoNumberString == null)
                    {
                        return "-1";
                    }
                }
                int evoNumber = Convert.ToInt32(evoNumberString);
                if (evoNumber == 1)
                {
                    return null;
                }
                string match = getMatch("no" + (evoNumber - 1) + @"=(\d\d\d)", EvoBox);
                if (match == null)
                {
                    match = getMatch("sprite" + (evoNumber - 1) + @"=(\d\d\d)", EvoBox);
                }
                return match;
            }
        }

        public int? PreviousEvolutionNationalId
        {
            get
            {
                string id = PreviousEvolutionNationalIdString;
                if (id == null)
                {
                    return null;
                }
                return Convert.ToInt32(id);
            }
        }


        public PokemonReader(string Infobox, string EvoBox)
        {
            this.Infobox = Infobox;
            this.EvoBox = EvoBox;
        }

        public override void PrintSqlInsert()
        {
            var b = new StringBuilder();
            b.AppendLine("INSERT INTO PMOLENDA.POKEMON VALUES (");
            b.AppendFormat("{0},", formatToSql(NationalPokedexNumber)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Name)).AppendLine();
            b.AppendFormat("{0},", formatToSql(GenerationIntroduced)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Weight)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Height)).AppendLine();
            b.AppendFormat("{0}, {1},", SQLInsert(Type1), SQLInsert(Type2)).AppendLine();
            b.AppendFormat("{0}, {1},", SQLInsert(Ability1), SQLInsert(Ability2)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(AbilityHidden)).AppendLine();
            b.AppendFormat("{0});", formatToSql(PreviousEvolutionNationalId)).AppendLine();
            Console.WriteLine(b.ToString());
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(GetText(NationalPokedexNumber));
            builder.AppendLine(GetText(Name));
            builder.AppendLine(GetText(GenerationIntroduced));
            builder.AppendLine(GetText(Weight));
            builder.AppendLine(GetText(Height));
            builder.AppendFormat("{0} {1}", GetText(Type1), GetText(Type2)).AppendLine();
            builder.AppendFormat("{0}, {1}, {2}", GetText(Ability1), GetText(Ability2),
                GetText(AbilityHidden)).AppendLine();
            builder.AppendLine(GetText(PreviousEvolutionNationalId));
            return builder.ToString();
        }

        public static int EvolutionsOrderKey(PokemonReader reader)
        {
            int? prev = reader.PreviousEvolutionNationalId;
            int id = reader.NationalPokedexNumber;
            return (prev.HasValue && prev.Value != -1) ? id : -800 + id;
        }

        public class Factory : Factory<PokemonReader>
        {
            public PokemonReader ReadFromWebPage(string pokemon)
            {
                string page = DownloadBulbaEditPage(pokemon);
                string infobox = getInfobox(page, "{{Pokémon Infobox", "==Biology");
                string evobox = getInfobox(page, "{{evobox", "==Sprites");
                return new PokemonReader(infobox, evobox);
            }
        }
    }
}
