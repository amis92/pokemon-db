using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseDownloader
{
    public class PokemonReader : BulbaReader<PokemonReader, PokemonReader.Factory>
    {

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

        private string EvoBox;

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

        private string Locations;

        public IReadOnlyCollection<Location> LocationAppearances
        {
            get;
            private set;
        }

        public IReadOnlyCollection<string> GameAppearances
        {
            get;
            private set;
        }


        private string Learnsets;


        public PokemonReader(string Infobox, string locations, string learnsets, string EvoBox)
        {
            this.Infobox = Infobox;
            this.Locations = locations;
            this.Learnsets = learnsets;
            this.EvoBox = EvoBox;
            setLocationsAndGames();
        }

        private void setLocationsAndGames()
        {
            String pattern =
@"(?x) \{\{ Availability/ Entry\d \| v= (?<Version1> [^\|]+ ) ( \| v2= (?<Version2> [^\|]+ ) )?" +
@".* \| area= (\[\[ Route \]\] s? )? .*?" +
@"(" +
    @"  ( \{\{ (?<Routes> rtn? \| \d+ \| [^\}]+ ) \}\} .*? )" +
    @"| ( \[\[ (?<Places> [^\|\]]+ ) [^\]]* \]\] .*? )" +
    @"| ( \{\{ (?<Places> [^\|\}]+ ) [^\}]* \}\} .*? )" +
@")+ \}\}";
            var matches = Regex.Matches(Locations, pattern);
            List<Location> locsList = new List<Location>();
            List<string> gamesList = new List<string>();
            foreach (Match m in matches)
            {
                foreach (Capture version in m.Groups["Version1"].Captures)
                {
                    gamesList.Add(version.Value);
                }
                foreach (Capture version in m.Groups["Version2"].Captures)
                {
                    gamesList.Add(version.Value);
                }
                foreach (Capture routeCapture in m.Groups["Routes"].Captures)
                {
                    string rString = routeCapture.Value;
                    string rNumber = getMatch(@"rtn?\|(\d+)\|.+", rString);
                    string rRegion = getMatch(@"rtn?\|\d+\|(.+)", rString);
                    locsList.Add(new Location("Route " + rNumber, rRegion));
                }
                foreach (Capture location in m.Groups["Places"].Captures)
                {
                    if (Regex.Match("Trade|p|pkmn|DL|pw|dwa|Evolution", location.Value).Success)
                    {
                        continue;
                    }
                    locsList.Add(new Location(location.Value, GetRegionOfVersion(gamesList[gamesList.Count - 1])));
                }
            }
            LocationAppearances = (new HashSet<Location>(locsList)).ToList();
            GameAppearances = (new HashSet<string>(gamesList)).ToList();
        }

        public override string GetSqlInsert()
        {
            var b = new StringBuilder();
            b.AppendLine("INSERT INTO PMOLENDA.POKEMON VALUES (");
            b.AppendFormat("{0},", SQLInsert(NationalPokedexNumber)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Name)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(GenerationIntroduced)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Weight)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(Height)).AppendLine();
            b.AppendFormat("{0}, {1},", SQLInsert(Type1), SQLInsert(Type2)).AppendLine();
            b.AppendFormat("{0}, {1},", SQLInsert(Ability1), SQLInsert(Ability2)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(AbilityHidden)).AppendLine();
            b.AppendFormat("{0});", SQLInsert(PreviousEvolutionNationalId)).AppendLine();
            return b.ToString();
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
            builder.AppendLine();
            builder.AppendLine("#Locations:");
            foreach (Location l in LocationAppearances)
            {
                builder.AppendFormat("{0}\t\t{1}", l.Region, l.Name).AppendLine();
            }
            builder.AppendLine();
            builder.AppendLine("#Games:");
            foreach (string game in GameAppearances)
            {
                builder.AppendLine(game);
            }
            return builder.ToString();
        }

        public static int EvolutionsOrderKey(PokemonReader reader)
        {
            int? prev = reader.PreviousEvolutionNationalId;
            int id = reader.NationalPokedexNumber;
            return (prev.HasValue && prev.Value != -1) ? id : -800 + id;
        }

        private string GetRegionOfVersion(string version)
        {
            switch (version)
            {
                case "Y":
                case "X":
                    return "Kalos";
                case "Black 2":
                case "White 2":
                case "Black":
                case "White":
                    return "Unova";
                case "HeartGold":
                case "SoulSilver":
                    return "Johto-Kanto";
                case "Diamond":
                case "Platinum":
                case "Pearl":
                    return "Sinnoh";
                case "Emerald":
                case "Ruby":
                case "Sapphire":
                    return "Hoenn";
                case "Silver":
                case "Gold":
                case "Crystal":
                    return "Johto";
                default:
                    return "Kanto";
            }
        }

        public class Factory : Factory<PokemonReader>
        {
            public PokemonReader ReadFromWebPage(string pokemonName)
            {
                if (!pokemonName.EndsWith("_(Pokémon)"))
                {
                    pokemonName += "_(Pokémon)";
                }
                string page = DownloadBulbaEditPage(pokemonName);
                //read basic info
                string infobox = getInfobox(page, "{{Pokémon Infobox", "==Biology");
                //read game locations
                string locations = getInfobox(page, "{{Availability/Header", "{{Availability/Footer}}");
                //read learnsets
                string learnsets = getInfobox(page, "==Learnset", "==Side game data");
                //read evolution
                string evobox = getInfobox(page, "{{evobox", "==Sprites");
                return new PokemonReader(infobox, locations, learnsets, evobox);
            }
        }

        public class Location
        {
            public string Name
            {
                get;
                private set;
            }

            public string Region
            {
                get;
                private set;
            }

            public Location(string Name, string Region)
            {
                this.Name = Regex.Match(Name, "([^#]*)(#.*)?").Result("$1");
                this.Region = Region;
            }

            public override string ToString()
            {
                return Region + "\t\t" + Name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Location)) return false;
                Location other = obj as Location;
                return Name.Equals(other.Name) && Region.Equals(other.Region);
            }

            public override int GetHashCode()
            {
                return (Name + Region).GetHashCode();
            }
        }
    }
}
