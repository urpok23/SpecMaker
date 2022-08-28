using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace SpecMaker
{
    public static class Utilities
    {
        public static T ReadConfig<T>(string pathToConfig)
        {
            using StreamReader sr = new(pathToConfig, detectEncodingFromByteOrderMarks: true);

            string jsonString = string.Empty;
            if (sr.CurrentEncoding == Encoding.UTF8)
            {
                jsonString = sr.ReadToEnd();
            }
            else
            {
                Encoding enc = CodePagesEncodingProvider.Instance.GetEncoding(sr.CurrentEncoding.CodePage);
                if (enc is null) throw new NullReferenceException($"Unexpected encoding: {sr.CurrentEncoding.BodyName}");

                Span<byte> buf = default;
                sr.BaseStream.Read(buf);

                byte[] utf8Bytes = Encoding.Convert(enc, Encoding.UTF8, buf.ToArray());
                jsonString = Encoding.UTF8.GetString(utf8Bytes);
            }

            return JsonSerializer.Deserialize<T>(jsonString);
        }
        public static async Task<T> ReadConfigAsync<T>(string pathToConfig)
        {
            using StreamReader sr = new(pathToConfig, detectEncodingFromByteOrderMarks: true);
            string jsonString = string.Empty;
            if (sr.CurrentEncoding == Encoding.UTF8)
            {
                return await JsonSerializer.DeserializeAsync<T>(sr.BaseStream);
            }
            else
            {
                Encoding enc = CodePagesEncodingProvider.Instance.GetEncoding(sr.CurrentEncoding.CodePage);
                if (enc is null) throw new NullReferenceException($"Unexpected encoding: {sr.CurrentEncoding.BodyName}");

                Memory<byte> buf = new();
                await sr.BaseStream.ReadAsync(buf);

                byte[] utf8Bytes = Encoding.Convert(enc, Encoding.UTF8, buf.ToArray());
                using MemoryStream ms = new(utf8Bytes);

                return await JsonSerializer.DeserializeAsync<T>(ms);
            }
        }
        public static int SortTablesByPositions (DataTable l, DataTable r, string clmnName)
        {
            string leftPos = l.Rows[0].Field<string>(clmnName);
            string rightPos = r.Rows[0].Field<string>(clmnName);

            return leftPos.CompareTo(rightPos);
        }
        public static string GetProperEnding(int x)
        {
            switch (x)
            {
                case 1:
                case > 20 when x % 10 == 1:
                    return "ие";
                case 2 or 3 or 4:
                case > 20 when x % 10 is 2 or 3 or 4:
                    return "ия";
                default:
                    return "ий";
            }
        }
        public static bool IsQuantityPatternSatisfied(string s, string pattern)
        {
            return Regex.IsMatch(s, pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
        }        
        public static Match SearchQuantity(string s, string pattern)
        {
            return Regex.Match(s, pattern,
                RegexOptions.Compiled | RegexOptions.RightToLeft,
                TimeSpan.FromSeconds(5)); ;
        }
        public static bool IsXLRangePatternSatisfied(string s, string pattern)
        {
            return Regex.IsMatch(s, pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
        }
    }
    public record ProgressHelper(string Text, int Percent);
}
