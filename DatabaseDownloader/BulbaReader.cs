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
    abstract class BulbaReader<T, F> where T : BulbaReader<T, F> where F : BulbaReader<T,F>.Factory<T>, new()
    {

        public string Infobox
        {
            get;
            protected set;
        }

        public abstract void PrintSqlInsert();

        public static void DownloadAndPrint(string listFilePath, string format, Action<string> defaultAction)
        {
            Action<string, Action<T>> download = (filePath, processor) =>
            {
                var abilityReaders = BulbaReader<T, F>.Download(filePath, (new F()).ReadFromWebPage);
                abilityReaders.ForEach(processor);
            };
            Action<T> process;
            switch (format)
            {
                case "sql":
                    process = new Action<T>(reader => reader.PrintSqlInsert());
                    break;
                case "txt":
                    process = new Action<T>(r => Console.WriteLine(r.ToString()));
                    break;
                default:
                    defaultAction(format);
                    return;
            }
            download(listFilePath, process);
        }

        public delegate T GetReader(string title);

        public static List<T> Download(string listFilePath, GetReader getReader)
        {
            var fileReader = File.OpenText(listFilePath);
            var titles = new List<string>();
            while (!fileReader.EndOfStream)
            {
                titles.Add(fileReader.ReadLine());
            }
            List<T> list = new List<T>();
            foreach (var title in titles)
            {
                var reader = getReader(title);
                if (reader == null)
                {
                    continue;
                }
                list.Add(reader);
            }
            return list;
        }

        protected static string DownloadBulbaEditPage(string title)
        {
            WebClient client = new WebClient();
            string url = @"http://bulbapedia.bulbagarden.net/w/index.php?title=" + title + @"&action=edit";
            try
            {
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                string page = reader.ReadToEnd();
                data.Close();
                reader.Close();
                return page;
            }
            catch (WebException e)
            {
                StreamReader r = new StreamReader(e.Response.GetResponseStream());
                Console.WriteLine(r.ReadToEnd());
            }
            return "error";
        }

        protected static string getInfobox(string page, string beginPattern, string endPattern)
        {
            int beginIndex = page.IndexOf(beginPattern, StringComparison.OrdinalIgnoreCase);
            if (beginIndex < 0)
            {
                throw new ArgumentOutOfRangeException("page didn't contain provided beginPattern");
            }
            string cropped = page.Substring(beginIndex);
            int endIndex = cropped.IndexOf(endPattern, StringComparison.OrdinalIgnoreCase);
            return cropped.Substring(0, cropped.Length - endIndex);
        }


        protected static string formatToSql(int? s)
        {
            if (!s.HasValue)
            {
                return "NULL";
            }
            return Convert.ToString(s);
        }

        protected static string formatToSql(int s)
        {
            return Convert.ToString(s);
        }

        protected static string SQLInsert(string s)
        {
            if (s == null || s == "" || s.Trim() == "")
            {
                return "NULL";
            }
            return @"'" + s.Replace(@"'", @"''").Trim() + @"'";
        }

        protected static string GetText(string property)
        {
            if (property == null || property == "")
            {
                return "null";
            }
            return property.Trim();
        }

        protected static string GetText(int? property)
        {
            if (!property.HasValue)
            {
                return "null";
            }
            return Convert.ToString(property.Value);

        }

        protected static string GetText(int property)
        {
            return Convert.ToString(property);
        }

        protected string getMatch(string pattern, string source)
        {
            Match match = Regex.Match(source, pattern);
            if (!match.Success)
            {
                return null;
            }
            return match.Result("$1");
        }

        protected string getMatch(string pattern)
        {
            return getMatch(pattern, Infobox);
        }

        public interface Factory<Reader>
        {
            Reader ReadFromWebPage(string title);
        }
    }

}
