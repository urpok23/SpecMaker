using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace SpecMaker
{
    public class SpecSettignsSchema
    {
        // to be read from config: column threshhold width, mandatory column names, etc...

        public string ExcelFilledFormSpecSheetName { get; set; }

        // mandatory columns order in config must be the same as column order in the final specification
        public SortedDictionary<int, string> MandatoryOrderedColumnNames { get; set; }
        public string DelimeterFor2ndColumn { get; set; }
        public HashSet<string> ExcludedColumnNames { get; set; }
        public string[] NoDelimeterFormatColumnNames { get; set; }
        public string[] ColumnLetterAddress{ get; set; }
        public int FirstRowIndex { get; set; }
        public int RowStep { get; set; }
        public int[] MaxRowIndex { get; set; }
        public string ExampleSheet2Name { get; set; }
        public Dictionary<string, string> FirstPageStampMap { get; set; }
        public Dictionary<string, string> FirstPageSignMap { get; set; }

        [JsonIgnore]
        public static string PathToSpecSettings { get; } = Path.Join("Settings", "SpecSettings.json");

    }
}