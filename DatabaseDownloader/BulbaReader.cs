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
    public abstract class BulbaReader<T, F> where T : BulbaReader<T, F> where F : BulbaReader<T,F>.Factory<T>, new()
    {

        public string Infobox
        {
            get;
            protected set;
        }

        public abstract string GetSqlInsert();

        public static void DownloadAndPrint(TextReader nameListReader, string format, Action<string> defaultAction)
        {
            Action<Action<T>> download = processor =>
            {
                var readerList = BulbaReader<T, F>.Download(nameListReader, (new F()).ReadFromWebPage);
                readerList.ForEach(processor);
            };
            Action<T> process;
            switch (format)
            {
                case "sql":
                    process = new Action<T>(r => Console.WriteLine(r.GetSqlInsert()));
                    break;
                case "txt":
                    process = new Action<T>(r => Console.WriteLine(r.ToString()));
                    break;
                default:
                    defaultAction(format);
                    return;
            }
            download(process);
        }

        public delegate T GetReader(string title);

        public static List<T> Download(TextReader nameListReader, GetReader getReader)
        {
            var titles = new List<string>();
            string line;
            while ((line = nameListReader.ReadLine()) != null)
            {
                titles.Add(line);
            }
            List<T> list = new List<T>();
            foreach (string title in titles)
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
                throw new ArgumentOutOfRangeException("title", title, "No webpage of given title found.");
            }
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
            return cropped.Substring(0, endIndex > 0 ? endIndex + endPattern.Length : cropped.Length);
        }


        protected static string SQLInsert(int? s)
        {
            if (!s.HasValue)
            {
                return "NULL";
            }
            return Convert.ToString(s);
        }

        protected static string SQLInsert(int s)
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
