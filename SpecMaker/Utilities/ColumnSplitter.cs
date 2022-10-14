using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpecMaker
{
    public static class ColumnSplitter
    {
        public enum SortingStrategy
        {
            PositionsAsDouble,
            PositionsAsInt
        }
        public static string[] SplitByThreshold(string str, int threshold)
        {
            if (string.IsNullOrWhiteSpace(str)) return new[] {string.Empty};
            str = str.Trim();

            (int q, _) = Math.DivRem(str.Length, threshold);
            List<string> res = new (capacity: q+1);

            StringBuilder sb = new();
            foreach (string s in str.Split())
            {
                if (s.Length + sb.Length > threshold && sb.Length != 0)
                {
                    res.Add($"{sb.ToString(0, sb.Length-1)}");
                    sb.Clear();
                }
                sb.Append($"{s} ");
            };
            res.Add($"{sb.ToString(0, sb.Length - 1)}");

            return res.ToArray();
        }

        private static string[] DefaultSplit(in DataTable dt, string columnName)
        {
            var pos = from DataRow r in dt.Rows
                      let val = r.Field<string>(columnName)
                      where ! string.IsNullOrEmpty(val)
                      select val;

            return string.Join(", ", pos).Split(' ');
        }
        private static string[] ExtractFirst(DataTable dt, string columnName)
        {
            return new[] { dt.Rows[0].Field<string>(columnName) };
        }
        public static string[] MakeFirstColumn(in DataTable dt, string columnName)
        {
            var pos = from DataRow r in dt.Rows
                      orderby r.Field<string>(columnName) ascending
                      select r.Field<string>(columnName)
                      ;
                      
            return string.Join(", ", pos).Split(' ');
        }
        public static string[] MakeSecondColumn(in DataTable dt, SpecSettignsSchema specSettings, HashSet<string> clmnNames)
        {
            List<string> res = new (clmnNames.Count+1);
            string armType = specSettings.MandatoryOrderedColumnNames[1];
            res.Add(dt.Rows[0].Field<string>(armType));
            
            foreach (var clmnName in clmnNames) 
            {
                var v = dt.Rows[0].Field<string>(clmnName);

                if(v is null) continue;

                res.Add(specSettings.NoDelimeterFormatColumnNames.Contains(clmnName) ? 
                    $"{clmnName}{v}" : 
                    $"{clmnName}{specSettings.DelimeterFor2ndColumn}{v}");
            }

            return res.ToArray();
        }
        public static string[] MakeThirdColumn(in DataTable dt, string columnName)
        {
            return ExtractFirst(dt, columnName);
        }
        public static string[] MakeFourthColumn(in DataTable dt, string columnName)
        {
            return DefaultSplit(dt, columnName);
        }
        public static string[] MakeFifthColumn(in DataTable dt, string columnName)
        {
            return DefaultSplit(dt, columnName);
        }
        public static (string[], string[]) MakeSixthAndSeventhColumn(in DataTable dt, 
            string column6Name, 
            string column7Name, 
            ref string[] secondColumn,
            string pattern)
        {
            // second column always has "Position" column => secondColumn.Length != 0
            string[] res6 = new string[secondColumn.Length];
            string[] res7 = new string[secondColumn.Length];

            res6[0] = dt.Rows[0].Field<string>(column6Name);

            var v = from DataRow r in dt.Rows select r.Field<string>(column7Name);
            int count = v.Select(s => Convert.ToInt32(s)).Sum();
            res7[0] = $"{count}";

            foreach ( (int i, string s) in secondColumn.Select((s, i) => (i, s)) )
            {
                if (i == 0) continue;

                Match matched = Utilities.SearchQuantity(s, pattern);
                if (matched.Success)
                {
                    secondColumn[i] = secondColumn[i][..matched.Index];
                    var splittedMatch = matched.Value.Split(' ');
                    res6[i] = splittedMatch.Last();
                    res7[i] = splittedMatch[1];
                }
                else
                {
                    res6[i] = string.Empty;
                    res7[i] = string.Empty;
                }
            }
            return (res6, res7);
        }
        public static string[] MakeEighthColumn(in DataTable dt, string columnName)
        {
            return ExtractFirst(dt, columnName);
        }
        public static string[] MakeNinthColumn(in DataTable dt, string columnName)
        {
            string s = dt.Rows[0].Field<string>(columnName);
            return SplitByThreshold(s, 17);
        }
    }
}
