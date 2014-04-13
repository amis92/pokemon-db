using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDownloader
{
    public class AttackReader : BulbaReader<AttackReader, AttackReader.Factory>
    {

        public class Factory : Factory<AttackReader>
        {

            public AttackReader ReadFromWebPage(string moveName)
            {
                if (!moveName.EndsWith("_(move)"))
                {
                    moveName += "_(move)";
                }
                string page = DownloadBulbaEditPage(moveName);
                string infobox = getInfobox(page, "{{MoveInfobox", "==Effect");
                return new AttackReader(infobox);
            }
        }


        public string Name
        {
            get
            {
                return getMatch(@"name=(.+)");
            }
        }

        public string PowerPointsString
        {
            get
            {
                return getMatch(@"basepp=(\d+)");
            }
        }

        public int PowerPoints
        {
            get
            {
                return Convert.ToInt32(PowerPointsString);
            }
        }

        public string BasePowerString
        {
            get
            {
                return getMatch(@"power=.*(\d+)\|");
            }
        }

        public int? BasePower
        {
            get
            {
                string power = BasePowerString;
                if (power.StartsWith("-"))
                {
                    return null;
                }
                return Convert.ToInt32(BasePowerString);
            }
        }

        public string AccuracyString
        {
            get
            {
                return getMatch(@"accuracy=.*(\d+)\|");
            }
        }

        public int? Accuracy
        {
            get
            {
                string acc = AccuracyString;
                if (acc.StartsWith("-"))
                {
                    return null;
                }
                return Convert.ToInt32(AccuracyString);
            }
        }

        public AttackReader(string Infobox)
        {
            this.Infobox = Infobox;
        }

        public override string GetSqlInsert()
        {
            var b = new StringBuilder();
            b.AppendLine("INSERT INTO PMOLENDA.ATTACKS VALUES (");
            b.AppendFormat("{0},", SQLInsert(Name)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(PowerPoints)).AppendLine();
            b.AppendFormat("{0},", SQLInsert(BasePower)).AppendLine();
            b.AppendFormat("{0});", SQLInsert(Accuracy)).AppendLine();
            return b.ToString();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder(Name);
            b.AppendLine(PowerPointsString);
            b.AppendLine(BasePowerString);
            b.AppendLine(AccuracyString);
            return b.ToString();
        }
    }
}
