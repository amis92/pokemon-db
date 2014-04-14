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
            get;
            private set;
        }

        public string NationalPokedexString
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int GenerationIntroduced
        {
            get;
            private set;
        }

        public string Weight
        {
            get;
            private set;
        }

        public string Height
        {
            get;
            private set;
        }

        public string Type1
        {
            get;
            private set;
        }

        public string Type2
        {
            get;
            private set;
        }

        public string Ability1
        {
            get;
            private set;
        }

        public string Ability2
        {
            get;
            private set;
        }

        public string AbilityHidden
        {
            get;
            private set;
        }

        private string EvoBox;

        public string PreviousEvolutionNationalIdString
        {
            get;
            private set;
        }

        public int? PreviousEvolutionNationalId
        {
            get;
            private set;
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

        public IReadOnlyCollection<string> Learnset
        {
            get;
            private set;
        }

        public PokemonReader(string Infobox, string locations, string learnsets, string EvoBox)
        {
            this.Infobox = Infobox;
            this.Locations = locations;
            this.Learnsets = learnsets;
            this.EvoBox = EvoBox;
            this.Name = getMatch(@"name=(.+)\s*\|");
            this.NationalPokedexString = getMatch(@"ndex=(\d+)\s*\|");
            this.NationalPokedexNumber = Convert.ToInt32(NationalPokedexString);
            this.GenerationIntroduced = GetGeneration();
            this.Weight = getMatch(@"weight-kg=(.+)\s*\|");
            this.Height = getMatch(@"height-m=(.+)\s*\|");
            this.Type1 = getMatch(@"type1=(.+)\s*\|");
            this.Type2 = getMatch(@"type2=(.+)\s*\|");
            this.Ability1 = getMatch(@"ability1=(.+)\s*\|");
            this.Ability2 = getMatch(@"ability2=(.+)\s*\|");
            this.AbilityHidden = getMatch(@"abilityd=(.+)\s*\|");
            this.PreviousEvolutionNationalIdString = CalcPrevEvoString();
            this.PreviousEvolutionNationalId = CalcPrevEvo();
            setLocationsAndGames();
            Learnset = GetLearnset();
        }

        private int GetGeneration()
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

        private string CalcPrevEvoString()
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

        private int? CalcPrevEvo()
        {
            string id = PreviousEvolutionNationalIdString;
            if (id == null)
            {
                return null;
            }
            return Convert.ToInt32(id);
        }

        private void setLocationsAndGames()
        { //TODO get to discover b2w2 area pattern (etc.)
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
            List<string> gameSet = new List<string>();
            foreach (Match m in matches)
            {
                foreach (Capture version in m.Groups["Version1"].Captures)
                {
                    if (Regex.Match(version.Value, "Pal Park|Pokéwalker|Dream World").Success)
                        continue;
                    gameSet.Add(version.Value);
                }
                foreach (Capture version in m.Groups["Version2"].Captures) gameSet.Add(version.Value);
                foreach (Capture routeCapture in m.Groups["Routes"].Captures)
                {
                    string rString = routeCapture.Value;
                    string rNumber = getMatch(@"rtn?\|(\d+)\|.+", rString);
                    string rRegion = getMatch(@"rtn?\|\d+\|(.+)", rString);
                    locsList.Add(new Location("Route " + rNumber, rRegion));
                }
                foreach (Capture location in m.Groups["Places"].Captures)
                {
                    if (Regex.Match(location.Value, "Trade|p|pkmn|DL|pw|dwa|Evolution").Success)
                        continue;
                    locsList.Add(new Location(location.Value, GetRegionOfVersion(gameSet[gameSet.Count - 1])));
                }
            }
            LocationAppearances = (new HashSet<Location>(locsList)).ToList();
            GameAppearances = (new HashSet<string>(gameSet)).ToList();
        }

        private IReadOnlyCollection<string> GetLearnset()
        {
            String pattern = @"(?x) \{\{ learnlist/ (tm|level|breed|tutor)5 \| (([^\|\{]+) | (( \{\{ [^\}]+ \}\} [^\|\{]* )+)) \| (?<attack>[^\|]+) \| .*";
            HashSet<string> attackSet = new HashSet<string>();
            var matches = Regex.Matches(Learnsets, pattern);
            foreach (Match m in matches)
            {
                attackSet.Add(m.Groups["attack"].Value);
            }
            return attackSet.ToList();
        }

        public override string GetSqlInsert()
        {
            var b = new StringBuilder();
            b.Append("INSERT INTO PMOLENDA.POKEMON VALUES (");
            b.AppendFormat(" {0},", SQLInsert(NationalPokedexNumber));
            b.AppendFormat(" {0},", SQLInsert(Name));
            b.AppendFormat(" {0},", SQLInsert(GenerationIntroduced));
            b.AppendFormat(" {0},", SQLInsert(Weight));
            b.AppendFormat(" {0},", SQLInsert(Height));
            b.AppendFormat(" {0}, {1},", SQLInsert(Type1), SQLInsert(Type2));
            b.AppendFormat(" {0}, {1},", SQLInsert(Ability1), SQLInsert(Ability2));
            b.AppendFormat(" {0},", SQLInsert(AbilityHidden));
            b.AppendFormat(" {0});", SQLInsert(PreviousEvolutionNationalId));
            b.AppendLine();
            foreach (Location l in LocationAppearances)
            {
                b.Append("INSERT INTO PMOLENDA.LOCATION_APPEARANCES VALUES (");
                b.AppendFormat(" {0}, {1}, {2} );", SQLInsert(l.Name), SQLInsert(l.Region), SQLInsert(NationalPokedexNumber)).AppendLine();
            }
            foreach (string game in GameAppearances)
            {
                b.Append("INSERT INTO PMOLENDA.GAME_APPEARANCES VALUES (");
                b.AppendFormat(" {0}, {1} );", SQLInsert(game), SQLInsert(NationalPokedexNumber)).AppendLine();
            }
            foreach (string attack in Learnset)
            {
                b.Append("INSERT INTO PMOLENDA.ATTACK_LEARNABILITY VALUES (");
                b.AppendFormat(" {0}, {1} );", SQLInsert(attack), SQLInsert(NationalPokedexNumber)).AppendLine();
            }
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
            builder.AppendLine("# Locations:");
            foreach (Location l in LocationAppearances)
            {
                builder.AppendFormat("{0}\t\t{1}", l.Region, l.Name).AppendLine();
            }
            builder.AppendLine();
            builder.AppendLine("# Games:");
            foreach (string game in GameAppearances)
            {
                builder.AppendLine(game);
            }
            builder.AppendLine();
            builder.AppendLine("# Attacks:");
            foreach (string attack in Learnset)
            {
                builder.AppendLine(attack);
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
                string locations = getInfobox(page, "{{Availability", "{{Availability/Footer}}");
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
