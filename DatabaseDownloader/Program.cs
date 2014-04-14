using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    -> first argument must be one of those: abilities/pokemon/attacks
    -> second argument specifies whether output should be in SQL or clear text: sql/txt
    -> third argument is:
              'cmd' - reads list of names from console input;
              '1' - fourth argument is the name of entity;
              path to file containing list of wanted entities;
    -> fourth is optionally the name to search for;";

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (args.Length != 3 && args.Length != 4)
            {
                goto helpExit;
            } try
            {

                TextReader reader;
                string format = args[1];
                switch (args[2])
                {
                    case "cmd":
                        if (args.Length != 3) goto helpExit;
                        reader = Console.In;
                        break;
                    case "1":
                        if (args.Length != 4) goto helpExit;
                        reader = new StringReader(args[3]);
                        break;
                    default:
                        if (args.Length != 3) goto helpExit;
                        reader = File.OpenText(args[2]);
                        break;
                }
                switch (args[0])
                {
                    case "abilities":
                        AbilityReader.DownloadAndPrint(reader, format, x => Console.WriteLine(help));
                        break;
                    case "pokemon":
                        PokemonReader.DownloadAndPrint(reader, format, x => Console.WriteLine(help));
                        break;
                    case "attacks":
                        AttackReader.DownloadAndPrint(reader, format, x => Console.WriteLine(help));
                        break;
                    default:
                        Console.WriteLine(help); break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            //Console.Beep(2000, 400);
            //Console.Beep(2500, 400);
            //Console.Beep(3000, 800);
            Console.WriteLine(String.Format("/* Task completed in {0} */", stopwatch.Elapsed));
            return;
            helpExit:
                Console.WriteLine(help);
                //Console.Beep(1800, 600);
                return;
        }
    }
}
