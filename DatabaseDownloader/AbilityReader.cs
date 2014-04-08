﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseDownloader
{
    class AbilityReader : BulbaReader<AbilityReader, AbilityReader.Factory>
    {
        public string Name
        {
            get
            {
                return getMatch(@"name=(.+)");
            }
        }

        public string Desc
        {
            get
            {
                string desc = Description6thEd;
                if (desc == null)
                {
                    desc = DescriptionAnyEd;
                }
                return desc;
            }
        }

        public string Description6thEd
        {
            get
            {
                string desc = getMatch(@"text6=(.+)");
                return desc;
            }
        }

        public string DescriptionAnyEd
        {
            get
            {
                string desc = getMatch(@"text\d=(.+)");
                return desc;
            }
        }

        public AbilityReader(string Infobox)
        {
            this.Infobox = Infobox;
        }

        public override void PrintSqlInsert()
        {
            var builder = new StringBuilder();
            builder.AppendLine("INSERT INTO PMOLENDA.ABILITIES VALUES (");
            builder.AppendFormat("{0},", SQLInsert(Name)).AppendLine();
            builder.AppendFormat("{0});", SQLInsert(Desc)).AppendLine();
            Console.WriteLine(builder.ToString());
        }

        public override string ToString()
        {
            return (new StringBuilder(Name)).Append('\n').Append(Desc).ToString();
        }

        public class Factory : Factory<AbilityReader>
        {
            public AbilityReader ReadFromWebPage(string title)
            {
                string page = DownloadBulbaEditPage(title);
                string infobox = getInfobox(page, "{{AbilityInfobox", "==Effect");
                return new AbilityReader(infobox);
            }
        }
    }


}
