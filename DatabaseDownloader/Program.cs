using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseDownloader
{

    class DownloadingProgram
    {

        public static string help =
@"Usage:
    -> first argument must be one of those: abilities/pokemon/attacks/learnsets
    -> second argument specifies path to file containing list of wanted entities
    -> third argument specifies whether output should be in SQL or clear text: sql/txt";

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(help);
                Console.Beep(1800, 600);
                return;
            }
            switch (args[0])
            {
                case "abilities":
                    AbilityReader.DownloadAndPrint(args[1], args[2], x => Console.WriteLine(help));
                    break;
                case "pokemon":
                    PokemonReader.DownloadAndPrint(args[1], args[2], x => Console.WriteLine(help));
                    break;
                default:
                    Console.WriteLine(help); break;
            }
            Console.Beep(2000, 400);
            Console.Beep(2500, 400);
            Console.Beep(3000, 800);
        }
    }
}
